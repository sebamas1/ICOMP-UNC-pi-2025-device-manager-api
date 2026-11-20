import logging
import signal
import threading

from .client import BackendClient
from .config import Config
from .core import Agent
from .gpio import open_pins


def main() -> None:
    logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(message)s")
    cfg = Config.from_env()
    stop = threading.Event()
    signal.signal(signal.SIGTERM, lambda *_: stop.set())
    signal.signal(signal.SIGINT, lambda *_: stop.set())
    Agent(cfg, open_pins(cfg), BackendClient(cfg.backend_url, cfg.device_id)).run(stop)


if __name__ == "__main__":
    main()
