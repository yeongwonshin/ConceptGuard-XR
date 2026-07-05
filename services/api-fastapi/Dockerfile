FROM python:3.12-slim AS runtime

ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    PYTHONPATH=/app/services/circuit-engine:/app/services

WORKDIR /app

COPY services/api-fastapi/requirements.txt /app/services/api-fastapi/requirements.txt
RUN pip install --no-cache-dir -r /app/services/api-fastapi/requirements.txt

COPY services/api-fastapi /app/services/api-fastapi
COPY services/circuit-engine /app/services/circuit-engine
COPY services/llm_feedback /app/services/llm_feedback

WORKDIR /app/services/api-fastapi
EXPOSE 8000
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
