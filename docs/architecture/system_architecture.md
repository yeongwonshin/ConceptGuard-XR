# System Architecture

```text
Unity XR Client
  ├─ component manipulation
  ├─ connection detector
  ├─ current/voltage visualization
  └─ session event logger
        │ JSON
        ▼
FastAPI Backend
  ├─ circuit graph analyzer
  ├─ misconception detector
  ├─ CUG-XR risk scorer
  ├─ RAG feedback generator
  └─ session storage
        │
        ├─ Teacher Dashboard
        └─ Evaluation Notebook
```

## 주요 데이터 흐름

1. Unity가 부품 배치와 연결 이벤트를 기록한다.
2. 이벤트를 회로 그래프로 변환해 API에 전달한다.
3. 서버가 회로 상태와 오개념 후보를 계산한다.
4. LLM/RAG가 검증된 근거에 기반해 피드백 문장을 생성한다.
5. Unity가 XR 공간에서 시각화와 질문을 표시한다.
6. 세션 결과가 교사용 대시보드에 요약된다.
