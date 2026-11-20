import os
from dataclasses import dataclass
from typing import Mapping


@dataclass(frozen=True)
class Config:
    backend_url: str
    device_id: int
    door_sensor_id: int
    relay_sensor_id: int
    # "gpiod" = GPIO real via libgpiod (HIL) | "file" = GPIO simulado en archivos (SIL)
    gpio_backend: str
    gpio_chip: str
    input_line: int
    output_line: int
    fake_gpio_dir: str
    poll_interval: float
    report_interval: float

    @classmethod
    def from_env(cls, env: Mapping[str, str] = os.environ) -> "Config":
        backend_url = env.get("BACKEND_URL")
        if not backend_url:
            raise ValueError("BACKEND_URL no definido")
        return cls(
            backend_url=backend_url.rstrip("/"),
            device_id=int(env.get("DEVICE_ID", "1")),
            door_sensor_id=int(env.get("DOOR_SENSOR_ID", "11")),
            relay_sensor_id=int(env.get("RELAY_SENSOR_ID", "12")),
            gpio_backend=env.get("GPIO_BACKEND", "gpiod"),
            gpio_chip=env.get("GPIO_CHIP", "/dev/gpiochip0"),
            input_line=int(env.get("GPIO_INPUT_LINE", "27")),
            output_line=int(env.get("GPIO_OUTPUT_LINE", "22")),
            fake_gpio_dir=env.get("FAKE_GPIO_DIR", "/tmp/fake-gpio"),
            poll_interval=float(env.get("POLL_INTERVAL", "0.5")),
            report_interval=float(env.get("REPORT_INTERVAL", "2")),
        )
