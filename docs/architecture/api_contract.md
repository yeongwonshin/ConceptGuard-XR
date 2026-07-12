# API Contract

## GET /health

```json
{
  "status": "ok",
  "service": "conceptguard-xr-api",
  "version": "1.0.0"
}
```

## GET /xr/config

This endpoint lets the Unity XR client retrieve missions, allowed components, and XR feature flags.

```json
{
  "api_version": "1.0.0",
  "missions": [
    {
      "id": "M2_SERIES_PARALLEL",
      "title": "Compare Series and Parallel Brightness",
      "goal": "Compare the number of current paths and bulb brightness changes through XR current flow.",
      "allowed_components": ["battery", "wire", "bulb", "resistor"]
    }
  ],
  "xr_features": {
    "hand_tracking": "OpenXR XR Hands optional",
    "interaction": "OpenXR device tracking + custom grip/trigger interaction",
    "visualization": "LineRenderer current-flow pulse, node overlays, ghost connection hints"
  }
}
```

## POST /sessions/events

Unity stores manipulation events such as grab, connect, disconnect, explain, and retry. The current API process stores session events in memory; deploy a persistent event store before multi-instance production use.

```json
{
  "session_id": "conceptguard-session-001",
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
  "session_id": "conceptguard-session-001",
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
  "learner_explanation": "I think some current will be used up by the first bulb.",
  "prediction": {"brightness": "dim", "topology": "series"},
  "manipulation_log": []
}
```

### Response

```json
{
  "session_id": "conceptguard-session-001",
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
  "feedback_text": "Let's check whether current disappears in the bulb...",
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
    "coach_message": "Let's check whether current disappears in the bulb...",
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
    "recommended_next_action": "Ready for the next mission"
  }
}
```
