# System Architecture

```text
Unity OpenXR Client
  ├─ XR Origin / controller or hand tracking
  ├─ XRGrabInteractable component manipulation
  ├─ XRSocketInteractor terminal snapping
  ├─ XRComponentNode metadata
  ├─ XRWireConnection graph edges
  ├─ CurrentFlowVisualizer LineRenderer pulse
  └─ MisconceptionCoachPanel world-space feedback
        │ JSON over HTTP
        ▼
FastAPI Backend
  ├─ /xr/config runtime mission config
  ├─ /sessions/events manipulation log ingest
  ├─ /analyze circuit + explanation analysis
  ├─ CircuitAnalyzer graph/topology/electrical values
  ├─ Misconception detector
  ├─ CUG-XR risk scorer
  └─ XR scene directive generator
        │
        ├─ Unity overlays/current flow/ghost actions
        └─ Teacher summary API
```

## Main Data Flow

1. The learner grabs and places batteries, bulbs, resistors, switches, and wires in XR space.
2. `XRSocketInteractor` creates terminal attachments, and `XRWireConnection` maintains connection lines.
3. `CircuitGraphBuilder` converts scene components and wires into circuit-graph JSON.
4. Unity sends the circuit graph, learner explanation, prediction, and manipulation log to `POST /analyze`.
5. The server calculates whether the circuit is closed, its topology, current, voltage, brightness, and misconception risk.
6. The server returns current paths, overlays, ghost actions, and a coach message in `xr_scene`.
7. Unity displays immediate feedback through `CurrentFlowVisualizer` and `MisconceptionCoachPanel`.

## Production Expansion Points

- Replace the in-memory session store with PostgreSQL or a school-specific tenant database.
- Restrict the RAG feedback generator to teacher-reviewed materials.
- Show each student's misconception trajectory in the teacher dashboard.
- Use Unity Addressables for remote mission and component content updates.
- Separate build pipelines by deployment channel, such as Quest Store, PICO, and Apple Vision Pro.
