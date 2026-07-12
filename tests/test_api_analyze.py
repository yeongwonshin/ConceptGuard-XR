import sys
from pathlib import Path

from fastapi.testclient import TestClient

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "services" / "api-fastapi"))

from app.main import app

client = TestClient(app)


def test_analyze_returns_xr_scene_directives():
    response = client.post(
        "/analyze",
        json={
            "session_id": "test-session",
            "mission_id": "M2_SERIES_PARALLEL",
            "circuit_graph": {
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
            },
            "learner_explanation": "Current is used up by the first bulb.",
            "prediction": {"brightness": "dim", "topology": "series"},
            "manipulation_log": [],
        },
    )
    assert response.status_code == 200
    payload = response.json()
    assert payload["closed_circuit"] is True
    assert payload["xr_scene"]["current_flow"]
    assert "current_consumption_misconception" in payload["misconceptions"]


def test_analyze_localizes_feedback_for_korean_unclosed_circuit():
    response = client.post(
        "/analyze",
        json={
            "session_id": "test-session-ko",
            "mission_id": "M1_CLOSED_CIRCUIT",
            "locale": "ko-KR",
            "circuit_graph": {
                "nodes": [
                    {"id": "battery_1", "type": "battery", "voltage_v": 3.0},
                    {"id": "bulb_1", "type": "bulb", "resistance_ohm": 10.0},
                ],
                "edges": [{"from": "battery_1", "to": "bulb_1"}],
            },
            "learner_explanation": "전류가 전구에서 소모된다.",
            "prediction": {"brightness": "bright", "topology": "series"},
            "manipulation_log": [],
        },
    )
    assert response.status_code == 200
    payload = response.json()
    assert payload["closed_circuit"] is False
    assert "닫힌 경로" in payload["feedback_text"]
    assert payload["teacher_summary"]["recommended_next_action"]
