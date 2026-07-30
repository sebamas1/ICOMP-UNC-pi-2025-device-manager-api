import sys
import json
import os
import requests

endpoint = os.environ.get("EKS_ENDPOINT")
if not endpoint:
    print("ERROR: EKS_ENDPOINT no definido", file=sys.stderr)
    sys.exit(1)

with open("hil/sensor_data.json") as f:
    data = json.load(f)

health = requests.get(f"{endpoint}/health", timeout=10)
health.raise_for_status()
print(f"Backend healthy: {health.status_code}")

temp_response = requests.get(f"{endpoint}/sensors/temperature/current", timeout=10)
temp_response.raise_for_status()
hum_response = requests.get(f"{endpoint}/sensors/humidity/current", timeout=10)
hum_response.raise_for_status()

backend_data = {
    "temperature": temp_response.json(),
    "humidity": hum_response.json(),
    "sensor_reading": data,
}

with open("hil/backend_data.json", "w") as f:
    json.dump(backend_data, f)

print(f"Backend respondio OK. Temperatura backend: {temp_response.json()}")
