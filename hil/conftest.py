"""Fixtures del test de lazo. HIL_MODE=hil usa el GPIO real; HIL_MODE=sil (default) el simulado."""
import os
import subprocess

import pytest

from harness import Backend, FileHarness, GpiodHarness, wait_until

MODE = os.environ.get("HIL_MODE", "sil")
TIMEOUT = float(os.environ.get("LOOP_TIMEOUT", "20"))


@pytest.fixture(scope="session")
def harness():
    if MODE == "hil":
        h = GpiodHarness(
            os.environ.get("GPIO_CHIP", "/dev/gpiochip0"),
            drive_line=int(os.environ.get("HARNESS_DRIVE_LINE", "17")),
            sense_line=int(os.environ.get("HARNESS_SENSE_LINE", "23")),
        )
    elif MODE == "sil":
        h = FileHarness(
            os.environ.get("FAKE_GPIO_DIR", "/tmp/fake-gpio"),
            agent_input_line=int(os.environ.get("GPIO_INPUT_LINE", "27")),
            agent_output_line=int(os.environ.get("GPIO_OUTPUT_LINE", "22")),
        )
    else:
        raise ValueError(f"HIL_MODE desconocido: {MODE}")
    yield h
    h.close()


@pytest.fixture(scope="session")
def backend(harness):
    url = os.environ.get("BACKEND_URL")
    if not url:
        pytest.fail("BACKEND_URL no definido")
    b = Backend(url, device_id=1, door_sensor_id=11, relay_sensor_id=12)
    # Estado inicial conocido: entrada en 0, comando en 0, y el agente ya sincronizado
    harness.set_agent_input(0)
    wait_until(lambda: b.door() == 0 and harness.read_agent_output() == 0,
               timeout=TIMEOUT, on_retry=lambda: b.set_relay(0))
    yield b
    try:
        b.set_relay(0)
    except Exception:
        pass


@pytest.fixture
def agent_stop():
    """Comando para detener el agente (docker stop en HIL, kill en SIL)."""
    cmd = os.environ.get("AGENT_STOP_CMD")
    if not cmd:
        pytest.skip("AGENT_STOP_CMD no definido")
    return lambda: subprocess.run(cmd, shell=True, check=True)
