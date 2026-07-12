# ConceptGuard XR

ConceptGuard XR은 학습자가 XR 공간에서 회로 부품을 직접 배치하고 연결하면, 회로 구조와 학습자 설명을 분석해 오개념 교정 피드백과 전류 흐름 시각화를 제공하는 교육 시스템입니다.

## Repository Layout

```text
ConceptGuard-XR/
├── apps/
│   ├── xr-client-unity/        # 실행 가능한 Unity/OpenXR 프로젝트
│   └── teacher-dashboard-next/
├── services/
│   ├── api-fastapi/            # Unity가 호출하는 REST API
│   ├── circuit-engine/         # 회로 그래프 분석
│   └── llm_feedback/           # 피드백 위험도 정책
├── packages/shared-schema/     # 공통 이벤트 스키마
├── infra/docker-compose.yml
└── tests/
```

Unity 설정과 실행 방법은 [`apps/xr-client-unity/README_UNITY.md`](apps/xr-client-unity/README_UNITY.md)를 참고하세요.

## Backend Quick Start

```bash
cd infra
docker compose up --build
```

```bash
curl http://127.0.0.1:8000/health
curl http://127.0.0.1:8000/xr/config
```

## Main API Flow

```text
Unity component manipulation
  -> CircuitGraphBuilder
  -> POST /analyze
  -> circuit engine and misconception-risk policy
  -> XR current flow, overlays, bulb state, coach feedback
```

Unity 클라이언트에는 서버 장애 시 가짜 결과를 반환하는 fallback이나 오프라인 데모 분석 로직이 없습니다. 설정 또는 네트워크 오류는 사용자에게 오류 상태로 표시됩니다.
