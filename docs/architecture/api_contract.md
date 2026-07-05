# API Contract

## GET /health

```json
{
  "status": "ok",
  "service": "conceptguard-xr-api",
  "version": "0.3.0-prototype"
}
```

## GET /xr/config

Unity XR 클라이언트가 미션, 허용 부품, XR 기능 플래그를 받아오는 엔드포인트입니다.

```json
{
  "api_version": "0.3.0-prototype",
  "missions": [
    {
      "id": "M2_SERIES_PARALLEL",
      "title": "직렬/병렬 밝기 비교",
      "goal": "전류 경로 수와 전구 밝기 변화를 XR 전류 흐름으로 비교한다.",
      "allowed_components": ["battery", "wire", "bulb", "resistor"]
    }
  ],
  "xr_features": {
    "hand_tracking": "OpenXR XR Hands optional",
    "interaction": "XR Interaction Toolkit grab + socket attach",
    "visualization": "LineRenderer current-flow pulse, node overlays, ghost connection hints"
  }
}
```

## POST /sessions/events

Unity가 grab, connect, disconnect, explain, retry 같은 조작 이벤트를 저장합니다. 시제품에서는 인메모리 저장이며, 제품화 시 DB로 교체해야 합니다.

```json
{
  "session_id": "xr-demo-001",
  "mission_id": "M2_SERIES_PARALLEL",
  "event_type": "connect",
  "timestamp_ms": 3412,
  "payload": {"from": "battery_1", "to": "bulb_1"}
}
```

## POST /analyze

### Request

```json
{
  "session_id": "xr-demo-001",
  "mission_id": "M2_SERIES_PARALLEL",
  "circuit_graph": {
    "nodes": [
      {"id": "battery_1", "type": "battery", "voltage_v": 3.0},
      {"id": "bulb_1", "type": "bulb", "resistance_ohm": 10.0},
      {"id": "bulb_2", "type": "bulb", "resistance_ohm": 10.0}
    ],
    "edges": [
      {"from": "battery_1", "to": "bulb_1", "terminal_from": "+", "terminal_to": "A"},
      {"from": "bulb_1", "to": "bulb_2", "terminal_from": "B", "terminal_to": "A"},
      {"from": "bulb_2", "to": "battery_1", "terminal_from": "B", "terminal_to": "-"}
    ]
  },
  "learner_explanation": "전류가 첫 번째 전구에서 조금 소모될 것 같아요.",
  "prediction": {"brightness": "dim", "topology": "series"},
  "manipulation_log": []
}
```

### Response

```json
{
  "session_id": "xr-demo-001",
  "mission_id": "M2_SERIES_PARALLEL",
  "closed_circuit": true,
  "topology": "series",
  "misconceptions": ["current_consumption_misconception"],
  "scores": {
    "SCD": 0.33,
    "MRR": 0.492,
    "core_error_impact": 1.0,
    "prediction_observation_diff": 0.0
  },
  "feedback_mode": "check_question",
  "feedback_text": "전류가 전구에서 사라지는지 확인해 볼까요?...",
  "electrical_values": {
    "source_voltage_v": 3.0,
    "total_current_a": 0.15,
    "equivalent_resistance_ohm": 20.0,
    "nodes": [
      {"node_id": "bulb_1", "node_type": "bulb", "voltage_v": 1.5, "current_a": 0.15, "power_w": 0.225, "brightness": "dim"}
    ],
    "flow_paths": [
      {"node_ids": ["battery_1", "bulb_1", "bulb_2"], "current_a": 0.15, "voltage_v": 3.0, "label": "main_loop"}
    ],
    "notes": [],
    "missing_connections": []
  },
  "xr_scene": {
    "scene_state": "closed_loop",
    "severity": "warning",
    "feedback_mode": "check_question",
    "coach_message": "전류가 전구에서 사라지는지 확인해 볼까요?...",
    "current_flow": [
      {"node_ids": ["battery_1", "bulb_1", "bulb_2"], "current_a": 0.15, "voltage_v": 3.0, "pulse_speed": 0.95, "label": "main_loop"}
    ],
    "overlays": [
      {"target_id": "bulb_1", "label": "0.150 A / 1.50 V / dim", "severity": "warning"}
    ],
    "ghost_actions": []
  },
  "teacher_summary": {
    "risk_level": "low",
    "headline": "M2_SERIES_PARALLEL: series, closed circuit",
    "misconception_count": 1,
    "recommended_next_action": "다음 미션 진행 가능"
  }
}
```
