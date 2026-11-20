"""Acceso a los pines del agente: una línea de entrada (sensor) y una de salida (actuador).

GpiodPins usa el GPIO real de la Raspberry Pi (HIL). FilePins guarda el nivel de cada
línea en un archivo, para correr el mismo agente sin hardware (SIL).
"""
import os
from pathlib import Path
from typing import Protocol

from .config import Config


class Pins(Protocol):
    def read_input(self) -> int: ...
    def write_output(self, value: int) -> None: ...
    def close(self) -> None: ...


class GpiodPins:
    def __init__(self, chip: str, input_line: int, output_line: int):
        # Import diferido: libgpiod sólo existe en la Raspi / imagen del agente
        import gpiod
        from gpiod.line import Bias, Direction, Value

        self._value = Value
        self._in = input_line
        self._out = output_line
        self._request = gpiod.request_lines(
            chip,
            consumer="device-agent",
            config={
                input_line: gpiod.LineSettings(direction=Direction.INPUT, bias=Bias.PULL_DOWN),
                output_line: gpiod.LineSettings(direction=Direction.OUTPUT, output_value=Value.INACTIVE),
            },
        )

    def read_input(self) -> int:
        return 1 if self._request.get_value(self._in) == self._value.ACTIVE else 0

    def write_output(self, value: int) -> None:
        self._request.set_value(self._out, self._value.ACTIVE if value else self._value.INACTIVE)

    def close(self) -> None:
        self._request.release()


class FilePins:
    """Cada línea es un archivo <dir>/<linea> con "0" o "1"."""

    def __init__(self, directory: str, input_line: int, output_line: int):
        self._dir = Path(directory)
        self._dir.mkdir(parents=True, exist_ok=True)
        self._in = self._dir / str(input_line)
        self._out = self._dir / str(output_line)
        self.write_output(0)

    def read_input(self) -> int:
        try:
            return 1 if self._in.read_text().strip() == "1" else 0
        except FileNotFoundError:
            return 0

    def write_output(self, value: int) -> None:
        tmp = self._out.with_suffix(".tmp")
        tmp.write_text("1" if value else "0")
        os.replace(tmp, self._out)

    def close(self) -> None:
        pass


def open_pins(cfg: Config) -> Pins:
    if cfg.gpio_backend == "gpiod":
        return GpiodPins(cfg.gpio_chip, cfg.input_line, cfg.output_line)
    if cfg.gpio_backend == "file":
        return FilePins(cfg.fake_gpio_dir, cfg.input_line, cfg.output_line)
    raise ValueError(f"GPIO_BACKEND desconocido: {cfg.gpio_backend}")
