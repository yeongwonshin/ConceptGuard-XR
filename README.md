# ConceptGuard XR

**오개념을 설명하는 AI가 아니라, 오개념이 강화되기 전에 막는 AI·XR 과학 튜터**입니다. 학생이 XR 공간에서 회로를 직접 조작하고 자신의 이유를 설명하면, 시스템은 조작 로그·회로 그래프·간이 회로 시뮬레이션·자연어 설명을 함께 분석하여 맞춤형 피드백과 XR 시각화를 제공합니다.

이번 버전은 문서형 아이디어가 아니라, **Unity OpenXR 클라이언트와 FastAPI 분석 서버가 연결되는 시제품 구조**를 포함합니다.

![상세 구성](apps/xr-client-unity/README_XR_PROTOTYPE.md)

## 핵심 가치

- 텍스트 답변만 보는 AI 튜터의 한계를 넘어, 학생의 실제 XR 조작 행동까지 분석합니다.
- 직렬/병렬, 전류 보존, 전압 분배, 닫힌 회로 조건 같은 추상 개념을 XR 전류 흐름과 부품 오버레이로 연결합니다.
- 정답을 바로 알려주기보다 힌트, 확인 질문, 반례 시뮬레이션, 직접 설명을 단계적으로 선택합니다.
- 교사는 학생별 반복 오개념, 수정 성공률, 힌트 의존도, 위험도를 대시보드로 확장할 수 있습니다.

## 현재 구현된 기능

1. **FastAPI 백엔드**
   - `POST /analyze`: 회로 그래프, 학생 설명, 예측, 조작 로그 분석
   - `GET /xr/config`: Unity XR 클라이언트 런타임 설정 제공
   - `POST /sessions/events`: XR 조작 이벤트 저장
   - `GET /sessions/{session_id}/summary`: 세션 이벤트 요약

2. **회로 분석 엔진**
   - 닫힌 회로/열린 회로 판정
   - 단일 부하, 직렬, 병렬, 혼합 후보 분류
   - 배터리 전압, 등가저항, 총전류, 부품별 전류/전압/전력/밝기 계산

3. **오개념 위험도 분석**
   - 열린 회로 혼동
   - 직렬/병렬 혼동
   - 전류 소모 오개념
   - 전압/전류 혼동
   - 예측-관찰 불일치

4. **Unity XR 클라이언트 스크립트**
   - `XRComponentNode`: XR 부품 메타데이터
   - `XRWireConnection`: 전선 연결 표현
   - `XRSocketWireConnector`: XR Socket 기반 단자 연결
   - `CircuitGraphBuilder`: XR 씬을 API 회로 그래프로 변환
   - `ConceptGuardXRApiClient`: 백엔드 분석 요청
   - `CurrentFlowVisualizer`: 전류 흐름 LineRenderer 시각화
   - `MisconceptionCoachPanel`: XR 코치 피드백 패널

## 빠른 실행

### 1. 백엔드 실행

```bash
cd infra
docker compose up --build
```

확인:

```bash
curl http://localhost:8000/health
curl http://localhost:8000/xr/config
```

### 2. 분석 API 테스트

```bash
curl -X POST http://localhost:8000/analyze \
  -H 'Content-Type: application/json' \
  -d '{
    "session_id":"xr-demo-001",
    "mission_id":"M2_SERIES_PARALLEL",
    "circuit_graph":{
      "nodes":[
        {"id":"battery_1","type":"battery","voltage_v":3.0},
        {"id":"bulb_1","type":"bulb","resistance_ohm":10.0},
        {"id":"bulb_2","type":"bulb","resistance_ohm":10.0}
      ],
      "edges":[
        {"from":"battery_1","to":"bulb_1"},
        {"from":"bulb_1","to":"bulb_2"},
        {"from":"bulb_2","to":"battery_1"}
      ]
    },
    "learner_explanation":"전류가 첫 번째 전구에서 조금 소모될 것 같아요.",
    "prediction":{"brightness":"dim","topology":"series"},
    "manipulation_log":[]
  }'
```

### 3. Unity XR 시제품 연결

Unity 2022 LTS 또는 2023 LTS에서 `apps/xr-client-unity` 아래 스크립트를 가져와 씬을 구성합니다.

필수 패키지:

- XR Interaction Toolkit
- OpenXR Plugin
- TextMeshPro
- Input System


