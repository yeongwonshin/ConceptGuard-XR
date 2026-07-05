# ConceptGuard XR Unity Prototype

이 폴더는 단순 문서용 Unity placeholder가 아니라, OpenXR 기반 시제품을 만들기 위한 런타임 스크립트 세트입니다. 목표는 학생이 XR 공간에서 배터리·전구·저항·스위치·전선을 직접 잡고 연결하면, FastAPI 백엔드가 회로 상태와 오개념 위험도를 분석하고 Unity가 전류 흐름·전압/전류 오버레이·피드백 패널을 표시하는 것입니다.

## 필요한 Unity 패키지

Unity 2022 LTS 또는 2023 LTS 프로젝트에서 다음 패키지를 설치합니다.

- XR Interaction Toolkit
- OpenXR Plugin
- TextMeshPro
- Input System

Meta Quest 계열은 Android/OpenXR 빌드 타깃으로 설정하고, 에디터 테스트는 XR Device Simulator를 사용합니다.

## 씬 구성

1. `XR Origin`을 추가합니다.
2. 실험대 역할의 빈 GameObject를 만들고 `CircuitGraphBuilder`, `ConceptGuardXRApiClient`, `CurrentFlowVisualizer`, `MisconceptionCoachPanel`을 배치합니다.
3. 배터리, 전구, 저항, 스위치 Prefab에 `XRGrabInteractable`과 `XRComponentNode`를 붙입니다.
4. 각 부품 단자 위치에 빈 Transform을 만들고 `XRComponentNode.Terminals`에 등록합니다.
5. 연결 지점에는 `XRSocketInteractor` 두 개와 `XRSocketWireConnector`를 붙입니다.
6. 분석 버튼, 손 제스처, 또는 키 입력에서 `ConceptGuardXRApiClient.AnalyzeCurrentCircuit()`를 호출합니다.

## 백엔드 연결

로컬 개발에서는 백엔드를 먼저 실행합니다.

```bash
cd infra
docker compose up --build
```

Unity의 `ConceptGuardXRApiClient.apiBaseUrl` 기본값은 다음과 같습니다.

```text
http://localhost:8000
```

Quest 실기기에서 Mac/PC의 백엔드를 호출하려면 `localhost`가 아니라 개발 머신의 LAN IP를 넣어야 합니다.

```text
http://192.168.x.x:8000
```

## 런타임 데이터 흐름

```text
XRGrabInteractable / XRSocketInteractor
  → XRComponentNode / XRWireConnection
  → CircuitGraphBuilder.BuildAnalyzeJson()
  → POST /analyze
  → xr_scene.current_flow + overlays + coach_message
  → CurrentFlowVisualizer + MisconceptionCoachPanel
```

## 시제품 데모 시나리오

1. 학생이 배터리와 전구를 한쪽만 연결합니다.
2. 분석 버튼을 누르면 열린 회로로 감지됩니다.
3. 코치 패널이 “되돌아오는 길”을 묻고, ghost action이 반환 경로를 제안합니다.
4. 학생이 회로를 닫으면 파란 전류 흐름이 한 바퀴 돌고 전구 밝기 오버레이가 표시됩니다.
5. 학생 설명에 “전류가 전구에서 소모된다”가 들어가면 전구 앞뒤 전류 비교 피드백이 표시됩니다.

## 판매 전 필수 보강 항목

이 코드는 시제품 수준입니다. 유료 판매 전에는 다음을 반드시 보강해야 합니다.

- Unity Prefab/Scene 완성본 제작
- Quest 실기기 OpenXR QA
- 학생 개인정보 저장 정책 및 학부모/학교 동의 플로우
- 교사용 대시보드 인증/권한
- 오개념 판정 정확도 평가 데이터
- 앱스토어/교육기관 배포용 라이선스, 개인정보처리방침, 환불 정책
