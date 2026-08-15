import sys
import json
import os
import random

# TODO: reemplazar con lectura real cuando el sensor DHT22 esté disponible
# import Adafruit_DHT
# SENSOR = Adafruit_DHT.DHT22
# GPIO_PIN = 4
# humidity, temperature = Adafruit_DHT.read_retry(SENSOR, GPIO_PIN)

SIMULATED = os.environ.get("HIL_SIMULATED", "true").lower() == "true"

if SIMULATED:
    temperature = round(random.uniform(18.0, 30.0), 2)
    humidity = round(random.uniform(40.0, 80.0), 2)
    print("WARN: Usando valores simulados (sensor fisico no disponible)")
else:
    import Adafruit_DHT
    humidity, temperature = Adafruit_DHT.read_retry(Adafruit_DHT.DHT22, 4)
    if temperature is None or humidity is None:
        print("ERROR: No se pudo leer el sensor DHT22", file=sys.stderr)
        sys.exit(1)

data = {"temperature": temperature, "humidity": humidity, "simulated": SIMULATED}

with open("hil/sensor_data.json", "w") as f:
    json.dump(data, f)

print(f"Sensor: {data['temperature']}°C  {data['humidity']}%HR  (simulado={SIMULATED})")
