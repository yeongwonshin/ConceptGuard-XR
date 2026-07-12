import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "services" / "circuit-engine"))

from conceptguard_circuit.graph_analyzer import CircuitAnalyzer


def test_closed_series_circuit_values():
    graph = {
        "nodes": [
            {"id": "battery_1", "type": "battery", "voltage_v": 3.0},
            {"id": "bulb_1", "type": "bulb", "resistance_ohm": 10.0},
            {"id": "bulb_2", "type": "bulb", "resistance_ohm": 10.0},
        ],
        "edges": [
            {"from": "battery_1", "to": "bulb_1"},
            {"from": "bulb_1", "to": "bulb_2"},
            {"from": "bulb_2", "to": "battery_1"},
        ],
    }
    result = CircuitAnalyzer().analyze(graph)
    assert result.closed_circuit is True
    assert result.topology == "series"
    assert result.equivalent_resistance_ohm == 20.0
    assert result.total_current_a == 0.15


def test_open_circuit_missing_return_path():
    graph = {
        "nodes": [
            {"id": "battery_1", "type": "battery", "voltage_v": 3.0},
            {"id": "bulb_1", "type": "bulb", "resistance_ohm": 10.0},
        ],
        "edges": [{"from": "battery_1", "to": "bulb_1"}],
    }
    result = CircuitAnalyzer().analyze(graph)
    assert result.closed_circuit is False
    assert result.topology == "open"
    assert "return_path" in result.missing_connections


def test_closed_circuit_ignores_unconnected_components_outside_power_loop():
    analyzer = CircuitAnalyzer()
    result = analyzer.analyze(
        {
            "nodes": [
                {"id": "battery_1", "type": "battery", "voltage_v": 3.0},
                {"id": "bulb_1", "type": "bulb", "resistance_ohm": 10.0},
                {"id": "switch_1", "type": "switch", "state": "closed"},
                {"id": "bulb_unused", "type": "bulb", "resistance_ohm": 10.0},
            ],
            "edges": [
                {"from": "battery_1", "to": "bulb_1"},
                {"from": "bulb_1", "to": "switch_1"},
                {"from": "switch_1", "to": "battery_1"},
            ],
        }
    )
    assert result.closed_circuit is True
    assert {value.node_id for value in result.values} == {"battery_1", "bulb_1", "switch_1"}
    assert result.total_current_a > 0
