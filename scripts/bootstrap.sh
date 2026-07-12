#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
python -m venv services/api-fastapi/.venv
cat <<'MSG'
Virtual environment created.

Next steps:
  source services/api-fastapi/.venv/bin/activate
  pip install -r services/api-fastapi/requirements.txt
  PYTHONPATH="$PWD/services/circuit-engine:$PWD/services" uvicorn services/api-fastapi/app/main:app --reload

Docker backend:
  cd infra
  docker compose up --build
MSG
