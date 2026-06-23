from fastapi import FastAPI
from pydantic import BaseModel, Field
from typing import Any, Dict, List, Optional

app = FastAPI(title="ConceptGuard XR API", version="0.1.0")

class AnalyzeRequest(BaseModel):
    session_id: str
    mission_id: str
    circuit_graph: Dict[str, Any]
    learner_explanation: Optional[str] = ""
    prediction: Optional[Dict[str, Any]] = Field(default_factory=dict)
    manipulation_log: List[Dict[str, Any]] = Field(default_factory=list)

class AnalyzeResponse(BaseModel):
    closed_circuit: bool
    topology: str
    misconceptions: List[str]
    scores: Dict[str, float]
    feedback_mode: str
    feedback_text: str

@app.get("/health")
def health():
    return {"status": "ok", "service": "conceptguard-xr-api"}

@app.post("/analyze", response_model=AnalyzeResponse)
def analyze(req: AnalyzeRequest):
    # MVP stub: replace with circuit-engine + CUG-XR scorer integration.
    node_types = [n.get("type") for n in req.circuit_graph.get("nodes", [])]
    edges = req.circuit_graph.get("edges", [])
    closed = "battery" in node_types and "bulb" in node_types and len(edges) >= 2
    topology = "unknown"
    misconceptions = []

    explanation = (req.learner_explanation or "").lower()
    if "소모" in explanation or "줄어" in explanation:
        misconceptions.append("current_consumption_misconception")

    scd = 0.35 if not misconceptions else 0.65
    mrr = min(1.0, scd + (0.15 if not closed else 0.0))
    feedback_mode = "check_question" if mrr < 0.5 else "counterexample_simulation"
    feedback_text = "전류가 지나갈 수 있는 닫힌 길이 있는지 먼저 확인해 볼까요?" if not closed else "예측과 실제 밝기를 비교해 보고, 전류 경로를 손으로 따라가 봅시다."

    return AnalyzeResponse(
        closed_circuit=closed,
        topology=topology,
        misconceptions=misconceptions,
        scores={"SCD": scd, "MRR": mrr},
        feedback_mode=feedback_mode,
        feedback_text=feedback_text,
    )
