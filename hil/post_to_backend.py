import sys
import json
import os
import requests

endpoint = os.environ.get("EKS_ENDPOINT")
if not endpoint:
    print("ERROR: EKS_ENDPOINT no definido", file=sys.stderr)
    sys.exit(1)

with open("hil/sensor_data.json") as f:
    sensor_data = json.load(f)

# Health check
health = requests.get(f"{endpoint}/api/devices/health", timeout=10)
health.raise_for_status()
print(f"Backend saludable: {health.status_code}")

# Actualizar sensor de temperatura del Device 1 (Raspberry Pi), Sensor 1 (T1)
temp_resp = requests.put(
    f"{endpoint}/api/devices/1/sensors/1",
    json={"id": 1, "name": "T1", "type": "Temperature", "value": sensor_data["temperature"], "status": "online"},
    timeout=10,
)
temp_resp.raise_for_status()
backend_temp = temp_resp.json()["value"]
print(f"Temperatura enviada: {sensor_data['temperature']}°C → Backend respondió: {backend_temp}°C")

# Actualizar sensor de humedad del Device 1 (Raspberry Pi), Sensor 2 (H1)
hum_resp = requests.put(
    f"{endpoint}/api/devices/1/sensors/2",
    json={"id": 2, "name": "H1", "type": "Humidity", "value": sensor_data["humidity"], "status": "online"},
    timeout=10,
)
hum_resp.raise_for_status()
backend_hum = hum_resp.json()["value"]
print(f"Humedad enviada: {sensor_data['humidity']}%HR → Backend respondió: {backend_hum}%HR")

with open("hil/backend_data.json", "w") as f:
    json.dump({"temperature": backend_temp, "humidity": backend_hum, "sensor_reading": sensor_data}, f)
