# ConceptGuard XR Unity Client

이 디렉터리는 Unity Hub에서 직접 열 수 있는 Unity 프로젝트입니다. 실행 시 코드가 교육용 회로 실험실을 구성하며, 분석 결과는 반드시 실제 FastAPI 서버의 `/analyze` 응답을 사용합니다. 오프라인 분석 결과나 가짜 응답을 생성하는 로직은 없습니다.

## 권장 환경

- Unity 2022.3 LTS
- OpenXR 호환 HMD 및 컨트롤러
- Python FastAPI 백엔드 또는 `infra/docker-compose.yml`

Unity Hub에서 다음 폴더를 프로젝트로 여세요.

```text
ConceptGuard-XR/apps/xr-client-unity
```

처음 열면 Package Manager가 `Packages/manifest.json`의 XR Management 및 OpenXR 패키지를 설치합니다. `ConceptGuardProjectInstaller`가 Standalone과 Android 빌드 대상에 OpenXR 로더를 할당합니다. Console에 OpenXR 할당 오류가 나타나면 `Edit > Project Settings > XR Plug-in Management`에서 현재 플랫폼의 OpenXR 체크박스를 직접 켜세요.

## 실행 순서

저장소 루트에서 백엔드를 먼저 실행합니다.

```bash
cd infra
docker compose up --build
```

다른 터미널에서 상태를 확인합니다.

```bash
curl http://127.0.0.1:8000/health
```

Unity에서 `Assets/ConceptGuardXR/Scenes/ConceptGuardLab.unity`를 열고 Play를 누릅니다.

## Quest에서 실행할 때

Quest의 `127.0.0.1`은 개발 PC가 아니라 Quest 자체를 가리킵니다. 아래 파일의 `api_base_url`을 개발 PC의 LAN IP로 변경하세요.

```text
Assets/ConceptGuardXR/StreamingAssets/conceptguard_xr_config.json
```

예시:

```json
{
  "api_base_url": "http://192.168.0.25:8000",
  "request_timeout_seconds": 10,
  "session_id_prefix": "conceptguard-session",
  "mission_id": "M2_SERIES_PARALLEL",
  "locale": "ko-KR"
}
```

개발 PC의 방화벽에서 TCP 8000 포트를 허용하고 Quest와 PC를 같은 네트워크에 연결해야 합니다.

## XR 조작

- Grip: 회로 부품 잡기 및 이동
- 오른손 Trigger: 버튼 누르기, 첫 번째 단자 선택, 두 번째 단자 선택
- 오른손 B/Secondary 버튼: 진행 중인 단자 선택 취소
- `연결 취소`: 마지막 전선 삭제
- `전선 지우기`: 모든 전선 삭제
- `전체 초기화`: 전선, 시각화, 부품 위치 초기화
- `회로 분석`: 현재 회로를 FastAPI `/analyze`로 전송

## 코드 구조

```text
Assets/ConceptGuardXR/
├── Editor/
│   └── ConceptGuardProjectInstaller.cs
├── Scenes/
│   └── ConceptGuardLab.unity
├── Scripts/
│   ├── Core/            # 설정 로딩, 앱 부트스트랩
│   ├── Environment/     # 교육 공간과 팔레트 생성
│   ├── Interaction/     # OpenXR 추적, 잡기, 포인터, 버튼
│   ├── Circuit/         # 단자, 전선 작성, 전구 표현
│   └── UI/              # 코치 패널과 노드 오버레이
└── StreamingAssets/
    └── conceptguard_xr_config.json
```

## 오류 처리 원칙

설정 파일이 없거나 JSON이 잘못되었거나 FastAPI 연결에 실패하면 분석 버튼이 비활성화되고 코치 패널에 실제 오류가 표시됩니다. 클라이언트는 서버 실패를 성공 응답으로 바꾸거나 로컬 샘플 결과를 대신 표시하지 않습니다.
