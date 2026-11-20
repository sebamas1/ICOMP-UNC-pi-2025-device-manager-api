"""Lazo del agente.

Cada iteración:
  1. Lee la entrada GPIO y la reporta al backend como valor del sensor DOOR
     (al cambiar y, además, cada report_interval como heartbeat).
  2. Lee del backend el valor del sensor RELAY (comando) y lo aplica a la salida GPIO.

Un error de red no detiene el agente: se registra y se reintenta en la próxima iteración.
"""
import logging
import threading
import time
from typing import Callable

import requests

from .client import BackendClient
from .config import Config
from .gpio import Pins

log = logging.getLogger("device-agent")


class Agent:
    def __init__(self, cfg: Config, pins: Pins, client: BackendClient,
                 clock: Callable[[], float] = time.monotonic):
        self._cfg = cfg
        self._pins = pins
        self._client = client
        self._clock = clock
        self._door: dict | None = None
        self._last_reported: int | None = None
        self._last_report_at = float("-inf")
        self.output: int = 0

    def step(self) -> None:
        self._report_input()
        self._apply_command()

    def _report_input(self) -> None:
        level = self._pins.read_input()
        now = self._clock()
        if level == self._last_reported and now - self._last_report_at < self._cfg.report_interval:
            return
        try:
            if self._door is None:
                self._door = self._client.get_sensor(self._cfg.door_sensor_id)
            self._client.put_sensor({**self._door, "value": level})
        except requests.RequestException as exc:
            log.warning("no se pudo reportar la entrada: %s", exc)
            return
        if level != self._last_reported:
            log.info("entrada GPIO%d=%d reportada", self._cfg.input_line, level)
        self._last_reported = level
        self._last_report_at = now

    def _apply_command(self) -> None:
        try:
            relay = self._client.get_sensor(self._cfg.relay_sensor_id)
        except requests.RequestException as exc:
            log.warning("no se pudo leer el comando: %s", exc)
            return
        desired = 1 if relay["value"] >= 0.5 else 0
        if desired != self.output:
            self._pins.write_output(desired)
            self.output = desired
            log.info("salida GPIO%d=%d aplicada", self._cfg.output_line, desired)

    def run(self, stop: threading.Event) -> None:
        log.info("agente iniciado: backend=%s gpio=%s in=%d out=%d",
                 self._cfg.backend_url, self._cfg.gpio_backend, self._cfg.input_line, self._cfg.output_line)
        try:
            while not stop.is_set():
                self.step()
                stop.wait(self._cfg.poll_interval)
        finally:
            # Estado seguro al apagar: actuador desactivado
            self._pins.write_output(0)
            self.output = 0
            self._pins.close()
            log.info("agente detenido, salida en 0")
