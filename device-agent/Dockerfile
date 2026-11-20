# Imagen del agente. En CI se construye directamente en la Raspberry Pi 2 (linux/arm/v7).
# Se usan los paquetes de Debian (python3-libgpiod, python3-requests) para no compilar
# extensiones nativas en la placa.
FROM debian:trixie-slim

RUN apt-get update \
    && apt-get install -y --no-install-recommends python3 python3-libgpiod python3-requests \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY agent ./agent

# Usuario no-root; el acceso a /dev/gpiochip* se da con --device y --group-add <gid de gpio>
RUN useradd --create-home --uid 10001 agent
USER agent

ENTRYPOINT ["python3", "-m", "agent"]
