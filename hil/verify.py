import sys
import json

with open("hil/backend_data.json") as f:
    data = json.load(f)

sensor_temp = data["sensor_reading"]["temperature"]
sensor_hum = data["sensor_reading"]["humidity"]
simulated = data["sensor_reading"].get("simulated", False)
backend_temp = data["temperature"]
backend_hum = data["humidity"]

errors = []

if not (-40 <= sensor_temp <= 80):
    errors.append(f"Temperatura fuera de rango DHT22: {sensor_temp}°C")

if not (0 <= sensor_hum <= 100):
    errors.append(f"Humedad fuera de rango DHT22: {sensor_hum}%")

if backend_temp is None:
    errors.append("Backend no devolvió temperatura")
elif abs(backend_temp - sensor_temp) > 0.01:
    errors.append(f"Round-trip falló: enviado {sensor_temp}°C, recibido {backend_temp}°C")

if backend_hum is None:
    errors.append("Backend no devolvió humedad")
elif abs(backend_hum - sensor_hum) > 0.01:
    errors.append(f"Round-trip falló: enviado {sensor_hum}%HR, recibido {backend_hum}%HR")

if errors:
    for e in errors:
        print(f"FAIL: {e}", file=sys.stderr)
    sys.exit(1)

mode = "SIMULADO" if simulated else "SENSOR FISICO"
print(f"HIL tests OK [{mode}]")
print(f"  Lectura  -> {sensor_temp}°C | {sensor_hum}%HR")
print(f"  Backend  -> {backend_temp}°C | {backend_hum}%HR (round-trip OK)")
