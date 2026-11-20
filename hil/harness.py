"""Banco de prueba del lazo GPIO: lo que el test usa para simular el entorno físico del agente.

En HIL el banco usa dos pines propios cableados a los del agente:

    banco GPIO17 (salida) ──[1kΩ]──▶ GPIO27 (entrada del agente)
    agente GPIO22 (salida) ──[1kΩ]──▶ GPIO23 (entrada del banco)

En SIL no hay cables: el banco escribe y lee directamente los archivos del GPIO simulado.
El test es el mismo en los dos casos; sólo cambia esta clase.
"""
import time
from pathlib import Path
from typing import Callable

import requests


class GpiodHarness:
    def __init__(self, chip: str, drive_line: int, sense_line: int):
        import gpiod
        from gpiod.line import Bias, Direction, Value

        self._value = Value
        self._drive = drive_line
        self._sense = sense_line
        self._request = gpiod.request_lines(
            chip,
            consumer="hil-harness",
            config={
                drive_line: gpiod.LineSettings(direction=Direction.OUTPUT, output_value=Value.INACTIVE),
                sense_line: gpiod.LineSettings(direction=Direction.INPUT, bias=Bias.PULL_DOWN),
            },
        )

    def set_agent_input(self, level: int) -> None:
        self._request.set_value(self._drive, self._value.ACTIVE if level else self._value.INACTIVE)

    def read_agent_output(self) -> int:
        return 1 if self._request.get_value(self._sense) == self._value.ACTIVE else 0

    def close(self) -> None:
        self.set_agent_input(0)
        self._request.release()


class FileHarness:
    def __init__(self, directory: str, agent_input_line: int, agent_output_line: int):
        self._dir = Path(directory)
        self._dir.mkdir(parents=True, exist_ok=True)
        self._in = self._dir / str(agent_input_line)
        self._out = self._dir / str(agent_output_line)

    def set_agent_input(self, level: int) -> None:
        self._in.write_text("1" if level else "0")

    def read_agent_output(self) -> int:
        try:
            return 1 if self._out.read_text().strip() == "1" else 0
        except FileNotFoundError:
            return 0

    def close(self) -> None:
        self.set_agent_input(0)


class Backend:
    def __init__(self, base_url: str, device_id: int, door_sensor_id: int, relay_sensor_id: int):
        self._base = f"{base_url.rstrip('/')}/api/devices/{device_id}/sensors"
        self._door = door_sensor_id
        self._relay = relay_sensor_id
        self._http = requests.Session()

    def _get(self, sensor_id: int) -> dict:
        resp = self._http.get(f"{self._base}/{sensor_id}", timeout=5)
        resp.raise_for_status()
        return resp.json()

    def door(self) -> float:
        return self._get(self._door)["value"]

    def set_relay(self, value: int) -> None:
        sensor = {**self._get(self._relay), "value": value}
        self._http.put(f"{self._base}/{self._relay}", json=sensor, timeout=5).raise_for_status()


def wait_until(condition: Callable[[], bool], timeout: float, interval: float = 0.2,
               on_retry: Callable[[], None] | None = None) -> float:
    """Espera a que condition() sea verdadera y devuelve los segundos transcurridos.

    on_retry se invoca en cada intento; se usa para reenviar comandos idempotentes
    (el backend guarda estado en memoria por pod, ver docs de HIL/SIL).
    """
    start = time.monotonic()
    while True:
        if on_retry:
            on_retry()
        try:
            if condition():
                return time.monotonic() - start
        except requests.RequestException:
            pass
        if time.monotonic() - start > timeout:
            raise TimeoutError(f"la condición no se cumplió en {timeout}s")
        time.sleep(interval)
