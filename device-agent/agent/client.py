import requests


class BackendClient:
    def __init__(self, base_url: str, device_id: int, timeout: float = 5.0, session: requests.Session | None = None):
        self._base = f"{base_url}/api/devices/{device_id}/sensors"
        self._timeout = timeout
        self._http = session or requests.Session()

    def get_sensor(self, sensor_id: int) -> dict:
        resp = self._http.get(f"{self._base}/{sensor_id}", timeout=self._timeout)
        resp.raise_for_status()
        return resp.json()

    def put_sensor(self, sensor: dict) -> dict:
        resp = self._http.put(f"{self._base}/{sensor['id']}", json=sensor, timeout=self._timeout)
        resp.raise_for_status()
        return resp.json()
