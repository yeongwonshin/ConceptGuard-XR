# ConceptGuard XR

**Not an AI that explains misconceptions after the fact, but an AI·XR science tutor that prevents misconceptions before they become reinforced.** When students manipulate circuits in XR space and explain their reasoning, the system analyzes their interaction logs, circuit graph, lightweight circuit simulation, and natural-language explanation together to provide personalized feedback and XR visualizations.

![Detailed Setup](apps/xr-client-unity/README_XR_PROTOTYPE.md)

## Core Value

- Goes beyond the limits of text-only AI tutors by analyzing students' actual hands-on behavior in XR.
- Connects abstract concepts such as series/parallel circuits, current conservation, voltage division, and closed-circuit conditions to XR current-flow visualization and component overlays.
- Instead of giving the answer immediately, it progressively selects hints, confirmation questions, counterexample simulations, and student self-explanation prompts.
- Can be extended into a teacher dashboard for repeated misconceptions, correction success rates, hint dependency, and risk levels by student.

## Currently Implemented Features

1. **FastAPI Backend**
   - `POST /analyze`: Analyzes the circuit graph, student explanation, prediction, and manipulation logs
   - `GET /xr/config`: Provides runtime configuration for the Unity XR client
   - `POST /sessions/events`: Stores XR interaction events
   - `GET /sessions/{session_id}/summary`: Summarizes session events

2. **Circuit Analysis Engine**
   - Detects closed and open circuits
   - Classifies single-load, series, parallel, and mixed-circuit candidates
   - Calculates battery voltage, equivalent resistance, total current, and per-component current, voltage, power, and brightness

3. **Misconception Risk Analysis**
   - Open-circuit confusion
   - Series/parallel confusion
   - Current-consumption misconception
   - Voltage/current confusion
   - Prediction-observation mismatch

4. **Unity XR Client Scripts**
   - `XRComponentNode`: XR component metadata
   - `XRWireConnection`: Wire connection representation
   - `XRSocketWireConnector`: Terminal connection using XR Socket interaction
   - `CircuitGraphBuilder`: Converts the XR scene into an API circuit graph
   - `ConceptGuardXRApiClient`: Sends analysis requests to the backend
   - `CurrentFlowVisualizer`: Visualizes current flow with LineRenderer
   - `MisconceptionCoachPanel`: Displays XR coaching feedback

## Quick Start

### 1. Run the Backend

```bash
cd infra
docker compose up --build
```

Check the server:

```bash
curl http://localhost:8000/health
curl http://localhost:8000/xr/config
```

### 2. Test the Analysis API

```bash
curl -X POST http://localhost:8000/analyze \
  -H 'Content-Type: application/json' \
  -d '{
    "session_id":"xr-demo-001",
    "mission_id":"M2_SERIES_PARALLEL",
    "circuit_graph":{
      "nodes":[
        {"id":"battery_1","type":"battery","voltage_v":3.0},
        {"id":"bulb_1","type":"bulb","resistance_ohm":10.0},
        {"id":"bulb_2","type":"bulb","resistance_ohm":10.0}
      ],
      "edges":[
        {"from":"battery_1","to":"bulb_1"},
        {"from":"bulb_1","to":"bulb_2"},
        {"from":"bulb_2","to":"battery_1"}
      ]
    },
    "learner_explanation":"I think the current will be partly consumed by the first bulb.",
    "prediction":{"brightness":"dim","topology":"series"},
    "manipulation_log":[]
  }'
```

### 3. Connect the Unity XR Prototype

Use Unity 2022 LTS or 2023 LTS and import the scripts under `apps/xr-client-unity` to build the scene.

Required packages:

- XR Interaction Toolkit
- OpenXR Plugin
- TextMeshPro
- Input System

