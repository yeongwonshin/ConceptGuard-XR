from __future__ import annotations

import sys
import uuid
from datetime import datetime, timezone
from pathlib import Path
from typing import Any, Dict, List, Optional

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / "services" / "circuit-engine"))
sys.path.insert(0, str(ROOT / "services"))

from conceptguard_circuit.graph_analyzer import CircuitAnalyzer, CircuitResult  # noqa: E402
from llm_feedback.feedback_policy import RiskSignals, compute_mrr, select_feedback_mode  # noqa: E402

app = FastAPI(
    title="ConceptGuard XR API",
    version="0.3.0-prototype",
    description="XR circuit misconception guardrail API for Unity/OpenXR prototypes.",
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=False,
    allow_methods=["*"],
    allow_headers=["*"],
)

analyzer = CircuitAnalyzer()
SESSION_EVENTS: Dict[str, List[Dict[str, Any]]] = {}


class AnalyzeRequest(BaseModel):
    session_id: str = Field(default_factory=lambda: f"anon-{uuid.uuid4().hex[:10]}")
    mission_id: str
    circuit_graph: Dict[str, Any]
    learner_explanation: Optional[str] = ""
    prediction: Optional[Dict[str, Any]] = Field(default_factory=dict)
    manipulation_log: List[Dict[str, Any]] = Field(default_factory=list)
    locale: str = "en-US"


class AnalyzeResponse(BaseModel):
    session_id: str
    mission_id: str
    closed_circuit: bool
    topology: str
    misconceptions: List[str]
    scores: Dict[str, float]
    feedback_mode: str
    feedback_text: str
    electrical_values: Dict[str, Any]
    xr_scene: Dict[str, Any]
    teacher_summary: Dict[str, Any]


class SessionEvent(BaseModel):
    session_id: str
    mission_id: str
    event_type: str
    timestamp_ms: int
    payload: Dict[str, Any] = Field(default_factory=dict)


@app.get("/health")
def health() -> Dict[str, str]:
    return {"status": "ok", "service": "conceptguard-xr-api", "version": app.version}


@app.get("/xr/config")
def xr_config() -> Dict[str, Any]:
    """Runtime config consumed by the Unity XR client."""
    return {
        "api_version": app.version,
        "missions": [
            {
                "id": "M1_CLOSED_CIRCUIT",
                "title": "Build a Closed Circuit",
                "goal": "Complete a path where current leaves the battery and returns to the battery.",
                "allowed_components": ["battery", "wire", "switch", "bulb"],
            },
            {
                "id": "M2_SERIES_PARALLEL",
                "title": "Compare Series and Parallel Brightness",
                "goal": "Compare the number of current paths and bulb brightness changes through XR current flow.",
                "allowed_components": ["battery", "wire", "bulb", "resistor"],
            },
            {
                "id": "M3_RESISTANCE_BRIGHTNESS",
                "title": "Predict Resistance and Brightness",
                "goal": "Predict and observe how current and bulb brightness change when resistance increases.",
                "allowed_components": ["battery", "wire", "bulb", "resistor", "switch"],
            },
        ],
        "xr_features": {
            "hand_tracking": "OpenXR XR Hands optional",
            "interaction": "XR Interaction Toolkit grab + socket attach",
            "visualization": "LineRenderer current-flow pulse, node overlays, ghost connection hints",
        },
    }


@app.post("/sessions/events")
def append_event(event: SessionEvent) -> Dict[str, Any]:
    events = SESSION_EVENTS.setdefault(event.session_id, [])
    item = event.model_dump()
    item["received_at"] = datetime.now(timezone.utc).isoformat()
    events.append(item)
    return {"stored": True, "session_id": event.session_id, "event_count": len(events)}


@app.get("/sessions/{session_id}/summary")
def session_summary(session_id: str) -> Dict[str, Any]:
    events = SESSION_EVENTS.get(session_id, [])
    counts: Dict[str, int] = {}
    for event in events:
        counts[event["event_type"]] = counts.get(event["event_type"], 0) + 1
    return {"session_id": session_id, "event_count": len(events), "event_type_counts": counts, "events": events[-25:]}


@app.post("/analyze", response_model=AnalyzeResponse)
def analyze(req: AnalyzeRequest) -> AnalyzeResponse:
    result = analyzer.analyze(req.circuit_graph)
    misconceptions = _detect_misconceptions(req, result)
    signals = _risk_signals(req, result, misconceptions)
    mrr = compute_mrr(signals)
    feedback_mode = select_feedback_mode(mrr)
    feedback_text = _feedback_text(req, result, misconceptions, feedback_mode)
    xr_scene = _xr_scene(result, misconceptions, feedback_mode, feedback_text)

    return AnalyzeResponse(
        session_id=req.session_id,
        mission_id=req.mission_id,
        closed_circuit=result.closed_circuit,
        topology=result.topology,
        misconceptions=misconceptions,
        scores={
            "SCD": signals.scd,
            "MRR": mrr,
            "core_error_impact": signals.core_error_impact,
            "prediction_observation_diff": signals.prediction_observation_diff,
        },
        feedback_mode=feedback_mode,
        feedback_text=feedback_text,
        electrical_values=_electrical_payload(result),
        xr_scene=xr_scene,
        teacher_summary=_teacher_summary(req, result, misconceptions, mrr),
    )


def _detect_misconceptions(req: AnalyzeRequest, result: CircuitResult) -> List[str]:
    text = (req.learner_explanation or "").lower().replace(" ", "")
    misconceptions: List[str] = []

    if not result.closed_circuit:
        misconceptions.append("open_circuit_confusion")
    if any(keyword in text for keyword in ["usedup", "consumed", "currentdrops", "currentdecreases", "weakerafter", "lesscurrentafter"]):
        misconceptions.append("current_consumption_misconception")
    if any(keyword in text for keyword in ["voltageflows", "voltageandcurrent", "voltage=current", "voltageiscurrent"]):
        misconceptions.append("voltage_current_confusion")
    if result.topology in {"series", "parallel"}:
        says_parallel = any(keyword in text for keyword in ["parallel", "sidebyside", "separatepaths", "twopaths", "branches"])
        says_series = any(keyword in text for keyword in ["series", "oneline", "singlepath", "onepath", "inorder"])
        if result.topology == "series" and says_parallel:
            misconceptions.append("series_parallel_confusion")
        if result.topology == "parallel" and says_series:
            misconceptions.append("series_parallel_confusion")

    prediction = req.prediction or {}
    if "brightness" in prediction:
        predicted = str(prediction.get("brightness", "")).lower()
        observed = _dominant_brightness(result)
        if predicted and observed and predicted not in {observed, "unknown"}:
            misconceptions.append("prediction_observation_mismatch")

    return sorted(set(misconceptions))


def _risk_signals(req: AnalyzeRequest, result: CircuitResult, misconceptions: List[str]) -> RiskSignals:
    repeated_errors = sum(1 for event in req.manipulation_log if event.get("event_type") in {"disconnect", "retry", "failed_connect"})
    prediction_diff = 1.0 if "prediction_observation_mismatch" in misconceptions else 0.0
    core_error = 1.0 if any(m in misconceptions for m in ["open_circuit_confusion", "current_consumption_misconception"]) else 0.35
    uncertainty = 0.5 if any(k in (req.learner_explanation or "").lower() for k in ["not sure", "maybe", "roughly", "confused", "i guess"]) else 0.1
    scd = 0.15 + 0.18 * len(misconceptions)
    if not result.closed_circuit:
        scd += 0.25
    return RiskSignals(
        scd=min(1.0, round(scd, 4)),
        core_error_impact=core_error,
        repeated_error_rate=min(1.0, repeated_errors / 5.0),
        prediction_observation_diff=prediction_diff,
        uncertainty_confidence_signal=uncertainty,
    )


def _feedback_text(req: AnalyzeRequest, result: CircuitResult, misconceptions: List[str], feedback_mode: str) -> str:
    if "open_circuit_confusion" in misconceptions:
        return "Trace the path with your hand: does current leave the battery, pass through the bulb, and return to the battery? Use the blue XR ghost line to make a closed loop."
    if "current_consumption_misconception" in misconceptions:
        return "Let's check whether current disappears in the bulb. Compare the current values before and after the bulb; in one closed path, the same current should flow through the path."
    if "series_parallel_confusion" in misconceptions:
        return "Count the XR current paths. Are the bulbs on the same path one after another, or are they on separate branches? Separate branches indicate a possible parallel circuit."
    if "prediction_observation_mismatch" in misconceptions:
        return "The predicted brightness and the simulated brightness do not match. Compare the resistance value and current-arrow speed, then state your prediction again."
    if feedback_mode == "minimal_hint":
        return "Good. Now check whether the current arrows complete a full loop without any break."
    if feedback_mode == "check_question":
        return "How many paths can current take in this circuit? Trace the path before and after the bulb with your hand."
    return "XR current flow and bulb-brightness simulation are now on. Compare your observation with your explanation, then revise the circuit once."


def _electrical_payload(result: CircuitResult) -> Dict[str, Any]:
    return {
        "source_voltage_v": result.source_voltage_v,
        "total_current_a": result.total_current_a,
        "equivalent_resistance_ohm": result.equivalent_resistance_ohm,
        "nodes": [value.__dict__ for value in result.values],
        "flow_paths": [path.__dict__ for path in result.flow_paths],
        "notes": result.notes,
        "missing_connections": result.missing_connections,
    }


def _xr_scene(result: CircuitResult, misconceptions: List[str], feedback_mode: str, feedback_text: str) -> Dict[str, Any]:
    severity = "success" if result.closed_circuit and not misconceptions else "warning" if result.closed_circuit else "danger"
    overlays = []
    for value in result.values:
        if value.node_type in {"bulb", "resistor"}:
            overlays.append(
                {
                    "target_id": value.node_id,
                    "label": f"{value.current_a:.3f} A / {value.voltage_v:.2f} V / {value.brightness}",
                    "severity": severity,
                }
            )
    for missing in result.missing_connections:
        overlays.append({"target_id": "workbench", "label": f"Missing connection: {missing}", "severity": "danger"})

    return {
        "scene_state": "closed_loop" if result.closed_circuit else "open_loop",
        "severity": severity,
        "feedback_mode": feedback_mode,
        "coach_message": feedback_text,
        "current_flow": [
            {
                "node_ids": path.node_ids,
                "current_a": path.current_a,
                "voltage_v": path.voltage_v,
                "pulse_speed": min(2.5, 0.35 + path.current_a * 4),
                "label": path.label,
            }
            for path in result.flow_paths
        ],
        "overlays": overlays,
        "ghost_actions": _ghost_actions(result),
    }


def _ghost_actions(result: CircuitResult) -> List[Dict[str, Any]]:
    if result.closed_circuit:
        return []
    actions = []
    if "return_path" in result.missing_connections:
        actions.append(
            {
                "type": "complete_return_path",
                "label": "Connect a return wire from the bulb to the opposite terminal of the battery.",
                "highlight": ["battery", "bulb"],
            }
        )
    if "battery" in result.missing_connections:
        actions.append({"type": "place_component", "component_type": "battery", "label": "Place a power battery."})
    if "load" in result.missing_connections:
        actions.append({"type": "place_component", "component_type": "bulb", "label": "Place a bulb or resistor."})
    return actions


def _teacher_summary(req: AnalyzeRequest, result: CircuitResult, misconceptions: List[str], mrr: float) -> Dict[str, Any]:
    return {
        "risk_level": "high" if mrr >= 0.75 else "medium" if mrr >= 0.5 else "low",
        "headline": f"{req.mission_id}: {result.topology}, {'closed' if result.closed_circuit else 'open'} circuit",
        "misconception_count": len(misconceptions),
        "recommended_next_action": "Provide individual intervention" if mrr >= 0.75 else "Ask for another XR retry" if mrr >= 0.5 else "Ready for the next mission",
    }


def _dominant_brightness(result: CircuitResult) -> str:
    brightness_rank = {"off": 0, "dim": 1, "medium": 2, "bright": 3}
    load_values = [value for value in result.values if value.node_type in {"bulb", "resistor"}]
    if not load_values:
        return "unknown"
    return max(load_values, key=lambda value: brightness_rank.get(value.brightness, 0)).brightness
