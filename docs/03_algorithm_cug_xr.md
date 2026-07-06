# CUG-XR Algorithm

## Purpose

CUG-XR does not judge a learner's misconception as simply right or wrong. It estimates the risk that a repeated misconception will become reinforced and uses that risk to select feedback intensity.

## Draft Scoring Formula

```text
MRR = 0.30*SCD + 0.25*CEI + 0.20*RER + 0.15*POD + 0.10*UCS
```

- SCD: mismatch between the circuit structure the learner built and the concept described in words
- CEI: severity of the core concept error
- RER: repeated error rate
- POD: difference between learner prediction and simulated observation
- UCS: conflict between confidence expressions and actual error

## Feedback Policy

```python
if mrr < 0.25:
    mode = "minimal_hint"
elif mrr < 0.50:
    mode = "check_question"
elif mrr < 0.75:
    mode = "counterexample_simulation"
else:
    mode = "direct_explanation_and_retry"
```

## Design Principles

1. Do not provide the correct answer immediately.
2. Preserve learner inquiry as much as possible when risk is low.
3. Intervene more strongly as risk increases to prevent misconception reinforcement.
4. Always bind LLM responses to circuit-engine verification results.
