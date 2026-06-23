#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
python -m venv services/api-fastapi/.venv
echo "Virtual environment created. Install requirements from services/api-fastapi/requirements.txt"
