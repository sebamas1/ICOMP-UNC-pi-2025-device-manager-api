import threading

import pytest
import requests

from agent.config import Config
from agent.core import Agent
from agent.gpio import FilePins


class FakePins:
    def __init__(self):
        self.input = 0
        self.writes: list[int] = []
        self.closed = False

    def read_input(self) -> int:
        return self.input

    def write_output(self, value: int) -> None:
        self.writes.append(value)

    def close(self) -> None:
        self.closed = True


class FakeBackend:
    def __init__(self):
        self.sensors = {
            11: {"id": 11, "name": "DOOR1", "type": "Door", "value": 0.0, "status": "Active"},
            12: {"id": 12, "name": "RELAY1", "type": "Relay", "value": 0.0, "status": "Active"},
        }
        self.puts: list[dict] = []
        self.fail = False

    def get_sensor(self, sensor_id: int) -> dict:
        if self.fail:
            raise requests.ConnectionError("backend caído")
        return dict(self.sensors[sensor_id])

    def put_sensor(self, sensor: dict) -> dict:
        if self.fail:
            raise requests.ConnectionError("backend caído")
        self.puts.append(sensor)
        self.sensors[sensor["id"]] = dict(sensor)
        return sensor


class FakeClock:
    def __init__(self):
        self.now = 0.0

    def __call__(self) -> float:
        return self.now


@pytest.fixture
def cfg():
    return Config.from_env({"BACKEND_URL": "http://backend", "REPORT_INTERVAL": "2", "POLL_INTERVAL": "0.01"})


@pytest.fixture
def parts(cfg):
    pins, backend, clock = FakePins(), FakeBackend(), FakeClock()
    return Agent(cfg, pins, backend, clock=clock), pins, backend, clock


def test_reporta_entrada_al_cambiar(parts):
    agent, pins, backend, _ = parts
    agent.step()
    pins.input = 1
    agent.step()
    assert [p["value"] for p in backend.puts] == [0, 1]
    assert backend.puts[-1]["name"] == "DOOR1"


def test_no_reporta_sin_cambio_hasta_el_heartbeat(parts):
    agent, _, backend, clock = parts
    agent.step()
    clock.now = 1.0
    agent.step()
    assert len(backend.puts) == 1
    clock.now = 2.5
    agent.step()
    assert len(backend.puts) == 2


def test_aplica_comando_del_relay(parts):
    agent, pins, backend, _ = parts
    backend.sensors[12]["value"] = 1.0
    agent.step()
    assert pins.writes == [1]
    agent.step()
    assert pins.writes == [1], "no reescribe la salida si el comando no cambió"
    backend.sensors[12]["value"] = 0.0
    agent.step()
    assert pins.writes == [1, 0]


def test_sobrevive_a_backend_caido(parts):
    agent, pins, backend, _ = parts
    backend.fail = True
    agent.step()
    backend.fail = False
    pins.input = 1
    agent.step()
    assert backend.puts[-1]["value"] == 1


def test_run_deja_salida_en_cero_al_detenerse(parts):
    agent, pins, backend, _ = parts
    backend.sensors[12]["value"] = 1.0
    stop = threading.Event()
    t = threading.Thread(target=agent.run, args=(stop,))
    t.start()
    stop.set()
    t.join(timeout=2)
    assert pins.writes[-1] == 0
    assert pins.closed


def test_file_pins(tmp_path):
    pins = FilePins(str(tmp_path), input_line=27, output_line=22)
    assert pins.read_input() == 0
    (tmp_path / "27").write_text("1")
    assert pins.read_input() == 1
    pins.write_output(1)
    assert (tmp_path / "22").read_text() == "1"


def test_config_requiere_backend_url():
    with pytest.raises(ValueError):
        Config.from_env({})
