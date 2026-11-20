"""Test de lazo del device-agent. Corre igual en SIL (GPIO simulado) y en HIL (Raspberry Pi real).

El orden importa: el último test detiene el agente.
"""
import os

from harness import wait_until

TIMEOUT = float(os.environ.get("LOOP_TIMEOUT", "20"))
LATENCY_BUDGET = float(os.environ.get("LATENCY_BUDGET", "5"))


def test_entrada_gpio_llega_al_backend(harness, backend, record_property):
    """Lazo 1: el banco cambia la entrada del agente y el backend debe reflejarlo."""
    for level in (1, 0):
        harness.set_agent_input(level)
        elapsed = wait_until(lambda: backend.door() == level, timeout=TIMEOUT)
        record_property(f"latencia_entrada_{level}_s", round(elapsed, 3))
        assert elapsed <= LATENCY_BUDGET, f"entrada={level} tardó {elapsed:.2f}s (máx {LATENCY_BUDGET}s)"


def test_comando_del_backend_llega_a_la_salida_gpio(harness, backend, record_property):
    """Lazo 2: el backend ordena activar el actuador y el banco debe leerlo en el pin."""
    for level in (1, 0):
        elapsed = wait_until(lambda: harness.read_agent_output() == level,
                             timeout=TIMEOUT, on_retry=lambda: backend.set_relay(level))
        record_property(f"latencia_salida_{level}_s", round(elapsed, 3))
        assert elapsed <= LATENCY_BUDGET, f"salida={level} tardó {elapsed:.2f}s (máx {LATENCY_BUDGET}s)"


def test_salida_vuelve_a_cero_al_detener_el_agente(harness, backend, agent_stop):
    """Fail-safe: con el actuador activo, detener el agente debe dejar la salida en 0."""
    wait_until(lambda: harness.read_agent_output() == 1,
               timeout=TIMEOUT, on_retry=lambda: backend.set_relay(1))
    agent_stop()
    wait_until(lambda: harness.read_agent_output() == 0, timeout=TIMEOUT)
