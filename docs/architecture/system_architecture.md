# System Architecture

```text
Unity OpenXR Client
  ├─ XR Origin / controller or hand tracking
  ├─ XRGrabInteractable component manipulation
  ├─ XRSocketInteractor terminal snapping
  ├─ XRComponentNode metadata
  ├─ XRWireConnection graph edges
  ├─ CurrentFlowVisualizer LineRenderer pulse
  └─ MisconceptionCoachPanel world-space feedback
        │ JSON over HTTP
        ▼
FastAPI Backend
  ├─ /xr/config runtime mission config
  ├─ /sessions/events manipulation log ingest
  ├─ /analyze circuit + explanation analysis
  ├─ CircuitAnalyzer graph/topology/electrical values
  ├─ Misconception detector
  ├─ CUG-XR risk scorer
  └─ XR scene directive generator
        │
        ├─ Unity overlays/current flow/ghost actions
        └─ Teacher summary API
```

## 주요 데이터 흐름

1. 학생이 XR 공간에서 배터리, 전구, 저항, 스위치, 전선을 잡고 배치한다.
2. `XRSocketInteractor`가 단자 결합을 만들고 `XRWireConnection`이 연결선을 유지한다.
3. `CircuitGraphBuilder`가 씬의 부품과 전선을 회로 그래프 JSON으로 변환한다.
4. Unity가 `POST /analyze`로 회로 그래프, 학생 설명, 예측, 조작 로그를 보낸다.
5. 서버가 닫힌 회로 여부, topology, 전류/전압/밝기, 오개념 위험도를 계산한다.
6. 서버가 `xr_scene`에 전류 경로, 오버레이, ghost action, 코치 메시지를 담아 반환한다.
7. Unity가 `CurrentFlowVisualizer`와 `MisconceptionCoachPanel`로 즉시 피드백을 표시한다.

## 제품화 시 확장 지점

- 인메모리 세션 저장소를 PostgreSQL 또는 학교별 테넌트 DB로 교체
- RAG 피드백 생성기를 교사 검수 자료 기반으로 제한
- 교사용 대시보드에 학생별 misconception trajectory 표시
- Unity Addressables로 미션/부품 콘텐츠 원격 업데이트
- Quest Store, PICO, Apple Vision Pro 등 배포 채널별 빌드 파이프라인 분리
