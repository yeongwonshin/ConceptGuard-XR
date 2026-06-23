# API Contract

## POST /analyze

### Request

```json
{
  "session_id": "anon-session-001",
  "mission_id": "M2_SERIES_PARALLEL",
  "circuit_graph": {
    "nodes": [
      {"id": "battery_1", "type": "battery"},
      {"id": "bulb_1", "type": "bulb"}
    ],
    "edges": [
      {"from": "battery_1", "to": "bulb_1", "terminal_from": "+", "terminal_to": "A"}
    ]
  },
  "learner_explanation": "전구 두 개를 나란히 연결했으니 둘 다 밝을 것 같아요.",
  "prediction": {"brightness": "same"},
  "manipulation_log": []
}
```

### Response

```json
{
  "closed_circuit": true,
  "topology": "parallel",
  "misconceptions": ["series_parallel_confusion"],
  "scores": {"SCD": 0.62, "MRR": 0.58},
  "feedback_mode": "counterexample_simulation",
  "feedback_text": "두 전구가 같은 길에 있는지, 서로 다른 길에 있는지 전류 경로를 따라가 볼까요?"
}
```
