from dataclasses import dataclass
from typing import Literal

FeedbackMode = Literal[
    "minimal_hint",
    "check_question",
    "counterexample_simulation",
    "direct_explanation_and_retry",
]

@dataclass
class RiskSignals:
    scd: float
    core_error_impact: float
    repeated_error_rate: float
    prediction_observation_diff: float
    uncertainty_confidence_signal: float


def compute_mrr(signals: RiskSignals) -> float:
    score = (
        0.30 * signals.scd
        + 0.25 * signals.core_error_impact
        + 0.20 * signals.repeated_error_rate
        + 0.15 * signals.prediction_observation_diff
        + 0.10 * signals.uncertainty_confidence_signal
    )
    return max(0.0, min(1.0, score))


def select_feedback_mode(mrr: float) -> FeedbackMode:
    if mrr < 0.25:
        return "minimal_hint"
    if mrr < 0.50:
        return "check_question"
    if mrr < 0.75:
        return "counterexample_simulation"
    return "direct_explanation_and_retry"
