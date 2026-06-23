# CUG-XR Algorithm

## 목적

CUG-XR은 학생의 오개념을 단순 정오로 판정하지 않고, 오개념이 반복되어 강화될 위험을 계산해 피드백 강도를 결정합니다.

## 점수 산식 초안

```text
MRR = 0.30*SCD + 0.25*CEI + 0.20*RER + 0.15*POD + 0.10*UCS
```

- SCD: 학생이 만든 회로 구조와 말로 설명한 개념의 불일치
- CEI: 핵심 개념 오류의 심각도
- RER: 동일 오류 반복률
- POD: 학생 예측과 시뮬레이션 관찰 결과의 차이
- UCS: 확신 표현과 실제 오류의 충돌 정도

## 피드백 정책

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

## 설계 원칙

1. 정답을 바로 주지 않는다.
2. 위험이 낮을수록 학생의 탐구권을 보존한다.
3. 위험이 높을수록 오개념 고착을 막기 위해 개입한다.
4. LLM 답변은 항상 회로 엔진의 검증 결과에 묶는다.
