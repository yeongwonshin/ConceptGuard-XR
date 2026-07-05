# ConceptGuard XR Unity Prototype

This folder is not a simple Unity placeholder for documentation. It is a runtime script set for building an OpenXR-based prototype. The goal is for students to directly grab and connect batteries, bulbs, resistors, switches, and wires in XR space, while the FastAPI backend analyzes the circuit state and misconception risk, and Unity displays current flow, voltage/current overlays, and feedback panels.

## Required Unity Packages

Install the following packages in a Unity 2022 LTS or 2023 LTS project.

- XR Interaction Toolkit
- OpenXR Plugin
- TextMeshPro
- Input System

For Meta Quest devices, set the build target to Android/OpenXR. For editor testing, use the XR Device Simulator.

## Scene Setup

1. Add an `XR Origin`.
2. Create an empty GameObject to serve as the lab bench, and attach `CircuitGraphBuilder`, `ConceptGuardXRApiClient`, `CurrentFlowVisualizer`, and `MisconceptionCoachPanel`.
3. Attach `XRGrabInteractable` and `XRComponentNode` to the battery, bulb, resistor, and switch prefabs.
4. Create empty Transforms at each component terminal position and register them in `XRComponentNode.Terminals`.
5. Add two `XRSocketInteractor` components and `XRSocketWireConnector` to the connection points.
6. Call `ConceptGuardXRApiClient.AnalyzeCurrentCircuit()` from an analyze button, hand gesture, or keyboard input.

## Backend Connection

For local development, start the backend first.

```bash
cd infra
docker compose up --build
```

The default value of `ConceptGuardXRApiClient.apiBaseUrl` in Unity is:

```text
http://localhost:8000
```

When calling the backend from a physical Quest device, use the LAN IP address of the development machine instead of `localhost`.

```text
http://192.168.x.x:8000
```

## Runtime Data Flow

```text
XRGrabInteractable / XRSocketInteractor
  → XRComponentNode / XRWireConnection
  → CircuitGraphBuilder.BuildAnalyzeJson()
  → POST /analyze
  → xr_scene.current_flow + overlays + coach_message
  → CurrentFlowVisualizer + MisconceptionCoachPanel
```

## Prototype Demo Scenario

1. The student connects only one side of a battery and a bulb.
2. When the analyze button is pressed, the system detects an open circuit.
3. The coach panel asks about the “return path,” and a ghost action suggests the missing return connection.
4. When the student closes the circuit, blue current flow circles around the loop and a bulb brightness overlay is displayed.
5. If the student's explanation includes the idea that “current is consumed by the bulb,” feedback comparing the current before and after the bulb is shown.

## Required Improvements Before Commercial Release

This code is at the prototype stage. Before paid release, the following items must be completed.

- Complete Unity prefabs and scenes
- OpenXR QA on physical Quest devices
- Student data privacy policy and parent/school consent flow
- Teacher dashboard authentication and authorization
- Evaluation dataset for misconception-detection accuracy
- App store or institutional deployment license, privacy policy, and refund policy
