from __future__ import annotations

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


def _clamp(value: float) -> float:
    return max(0.0, min(1.0, float(value)))


def compute_mrr(signals: RiskSignals) -> float:
    """Misconception Reinforcement Risk.

    Higher scores mean the learner is likely to reinforce a wrong mental model
    if the system only provides a light hint.
    """
    score = (
        0.30 * _clamp(signals.scd)
        + 0.25 * _clamp(signals.core_error_impact)
        + 0.20 * _clamp(signals.repeated_error_rate)
        + 0.15 * _clamp(signals.prediction_observation_diff)
        + 0.10 * _clamp(signals.uncertainty_confidence_signal)
    )
    return round(_clamp(score), 4)


def select_feedback_mode(mrr: float) -> FeedbackMode:
    mrr = _clamp(mrr)
    if mrr < 0.25:
        return "minimal_hint"
    if mrr < 0.50:
        return "check_question"
    if mrr < 0.75:
        return "counterexample_simulation"
    return "direct_explanation_and_retry"
