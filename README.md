# ConceptGuard XR

**An AI-assisted XR science tutor that helps learners recognize and correct circuit misconceptions through hands-on experimentation, verified circuit analysis, and adaptive visual feedback.**

ConceptGuard XR is an educational system in which learners build electrical circuits by placing and connecting components in an immersive XR laboratory. The system interprets the circuit structure together with the learner's explanation and prediction, then returns misconception-aware coaching, electrical measurements, and animated current-flow visualizations.

Rather than immediately revealing the correct answer, ConceptGuard XR encourages learners to inspect their own circuit, compare predictions with observable results, and revise their reasoning.

[Unity Setup and Usage](apps/xr-client-unity/README_UNITY.md) · [System Architecture](docs/architecture/system_architecture.md) · [API Contract](docs/architecture/api_contract.md)

## Why ConceptGuard XR?

Many circuit misconceptions are difficult to identify from a written answer alone. A learner may use the correct terminology while still wiring a circuit incorrectly, or may build a working circuit while holding an inaccurate mental model of current and voltage.

ConceptGuard XR combines multiple forms of evidence:

- the circuit the learner physically constructs in XR;
- the connections and manipulation events created during the activity;
- the learner's explanation and prediction;
- circuit-engine results such as topology, current, voltage, resistance, power, and brightness.

This allows feedback to address not only whether an answer is correct, but also **where the learner's reasoning and observable circuit behavior diverge**.

## Core Learning Experience

1. **Build** — The learner grabs batteries, bulbs, resistors, switches, and wires in an XR laboratory.
2. **Connect** — Terminal-level connections are converted into a structured circuit graph.
3. **Explain** — The learner provides a prediction or explanation of what should happen.
4. **Analyze** — The backend verifies the circuit topology and calculates electrical values.
5. **Reflect** — The system identifies misconception signals and selects an appropriate coaching strategy.
6. **Observe** — Current paths, component values, bulb states, and feedback are displayed directly in the XR scene.
7. **Retry** — The learner modifies the circuit and tests a revised idea.

```text
XR component manipulation
  -> CircuitGraphBuilder
  -> POST /analyze
  -> circuit analysis + misconception-risk policy
  -> current-flow animation + overlays + coaching feedback
```

## Key Features

### Immersive Circuit Construction

The Unity/OpenXR client provides a spatial learning environment in which students can move components, select terminals, create wires, toggle switches, clear connections, and reset the experiment.

### Verified Circuit Analysis

The circuit engine evaluates whether a circuit is open or closed, identifies series and parallel structures, and calculates:

- source voltage;
- equivalent resistance;
- total current;
- per-component current and voltage;
- power and approximate bulb brightness;
- current-flow paths and missing connections.

### Misconception-Aware Coaching

The system detects learning signals associated with topics such as:

- open- and closed-circuit confusion;
- series and parallel circuit confusion;
- the belief that current is consumed by a component;
- confusion between current and voltage;
- disagreement between a learner's prediction and the simulated observation.

### Adaptive Feedback with CUG-XR

The CUG-XR policy estimates misconception-reinforcement risk instead of treating every error in the same way. Depending on the risk level, the system can select a minimal hint, a checking question, a counterexample-oriented prompt, or a more direct explanation followed by another attempt.

See the [CUG-XR Algorithm](docs/03_algorithm_cug_xr.md) for the scoring model and feedback policy.

### In-Scene Visual Explanation

Analysis results are returned as XR scene directives and presented through:

- animated current-flow paths;
- current and voltage overlays;
- bulb brightness changes;
- highlighted missing or incorrect connections;
- a world-space misconception coach panel.

### Fail-Closed Client Behavior

ConceptGuard XR does not replace server failures with fabricated success results or offline demo analysis. Invalid configuration and network failures are shown as explicit errors, and analysis remains unavailable until the real API is reachable.

### Privacy-Oriented Learning Logs

The project is designed around anonymous session identifiers and data minimization. Raw voice recordings are not required by default, and learning feedback is constrained by circuit-engine verification.

See [Data Privacy and Safety](docs/06_data_privacy_safety.md).

## Learning Scenarios

The current learning design focuses on foundational circuit concepts:

| Scenario | Learning goal | Example misconception signal |
|---|---|---|
| Closed circuit | Understand that current requires a complete return path | The learner expects current to flow after connecting only one side of a bulb |
| Series and parallel | Relate circuit structure to the number of current paths and bulb behavior | The learner describes separate paths while constructing a series circuit |
| Current conservation | Understand that current is not consumed by a bulb | The learner predicts less current after the first bulb in a single loop |

Detailed activity descriptions are available in [Learning Scenarios](docs/04_learning_scenarios.md).

## System Architecture

```text
Unity OpenXR Client
  ├─ XR tracking and component interaction
  ├─ circuit graph construction
  ├─ learner prediction and explanation
  └─ current-flow, overlay, and coach presentation
           │
           │ JSON over HTTP
           ▼
FastAPI Backend
  ├─ runtime XR configuration
  ├─ session event collection
  ├─ circuit and explanation analysis
  ├─ CUG-XR misconception-risk scoring
  └─ XR scene directive generation
           │
           ▼
Circuit Analysis Engine
  ├─ connectivity and topology verification
  ├─ electrical-value calculation
  └─ component-level results
```

For a more detailed description, see [System Architecture](docs/architecture/system_architecture.md).

## Repository Layout

```text
ConceptGuard-XR/
├── apps/
│   ├── xr-client-unity/        # Executable Unity/OpenXR client
│   └── teacher-dashboard-next/ # Planned teacher-facing dashboard
├── services/
│   ├── api-fastapi/            # REST API used by the Unity client
│   ├── circuit-engine/         # Circuit graph and electrical analysis
│   └── llm_feedback/           # Feedback-risk policy
├── packages/
│   └── shared-schema/          # Shared circuit-event schema
├── data/                       # Misconception rules, rubrics, and samples
├── docs/                       # Product, learning, architecture, and safety docs
├── infra/
│   └── docker-compose.yml      # Local backend environment
└── tests/                      # API, analyzer, and feedback-policy tests
```

## Quick Start

### 1. Start the Backend

From the repository root:

```bash
cd infra
docker compose up --build
```

Verify that the API is available:

```bash
curl http://127.0.0.1:8000/health
curl http://127.0.0.1:8000/xr/config
```

### 2. Open the Unity Project

Open the following directory in Unity Hub:

```text
ConceptGuard-XR/apps/xr-client-unity
```

Recommended environment:

- Unity 2022.3 LTS;
- an OpenXR-compatible headset and controllers;
- the FastAPI backend running locally or on a reachable network host.

Open the scene below and enter Play mode:

```text
Assets/ConceptGuardXR/Scenes/ConceptGuardLab.unity
```

For headset networking, controls, OpenXR configuration, and troubleshooting, follow the [Unity Client Guide](apps/xr-client-unity/README_UNITY.md).

## API Overview

| Method | Endpoint | Purpose |
|---|---|---|
| `GET` | `/health` | Check backend health and version |
| `GET` | `/xr/config` | Retrieve mission and XR runtime configuration |
| `POST` | `/sessions/events` | Record learner interaction events |
| `POST` | `/analyze` | Analyze the circuit, explanation, prediction, and interaction context |
| `GET` | `/sessions/{session_id}/summary` | Retrieve a session-level learning summary |

Request and response examples are documented in the [API Contract](docs/architecture/api_contract.md).

## Documentation

| Document | Description |
|---|---|
| [Improved Development Plan](docs/01_improved_development_plan.md) | Project vision, service scenario, roadmap, technical structure, and development plan |
| [Product Requirements](docs/02_product_requirements.md) | Target users, user stories, feature priorities, and non-functional requirements |
| [CUG-XR Algorithm](docs/03_algorithm_cug_xr.md) | Misconception-risk scoring and adaptive feedback policy |
| [Learning Scenarios](docs/04_learning_scenarios.md) | Instructional scenarios for closed circuits, topology, and current conservation |
| [Evaluation Plan](docs/05_evaluation_plan.md) | Research questions, comparison groups, metrics, and ablation design |
| [Data Privacy and Safety](docs/06_data_privacy_safety.md) | Data-minimization rules, AI safeguards, and educational ethics |
| [System Architecture](docs/architecture/system_architecture.md) | End-to-end data flow and production expansion points |
| [API Contract](docs/architecture/api_contract.md) | Backend endpoints and JSON examples |
| [Prototype Release Checklist](docs/deployment/prototype_release_checklist.md) | Demo readiness and productization requirements |
| [Unity Client Guide](apps/xr-client-unity/README_UNITY.md) | Unity setup, OpenXR configuration, controls, and error behavior |

## Current Project Scope

The repository currently contains an executable Unity/OpenXR learning client, a FastAPI service, a circuit-analysis engine, misconception-risk logic, shared schemas, sample learning data, and automated backend tests.

The teacher dashboard directory is reserved for future development. Before production classroom deployment, the in-memory event store should be replaced with persistent storage, authentication and authorization should be added, and target-headset builds should be validated on physical devices.

## Design Principles

- Analyze learner actions, not only final written answers.
- Keep circuit-engine verification authoritative.
- Encourage reflection before revealing an answer.
- Increase intervention only when misconception-reinforcement risk rises.
- Expose configuration and network failures instead of hiding them.
- Treat misconceptions as opportunities for learning, not as labels attached to students.
