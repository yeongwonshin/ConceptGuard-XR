from services.llm_feedback.feedback_policy import RiskSignals, compute_mrr, select_feedback_mode


def test_feedback_mode_high_risk():
    signals = RiskSignals(1, 1, 1, 1, 1)
    assert compute_mrr(signals) == 1.0
    assert select_feedback_mode(0.8) == "direct_explanation_and_retry"
