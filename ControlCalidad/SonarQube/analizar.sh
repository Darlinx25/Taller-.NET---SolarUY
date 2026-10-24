#!/bin/bash
set -euo pipefail

# =========================================================================
# Análisis estático con SonarQube.
#
# El servidor de SonarQube vive en el stack de calidad
# (ControlCalidad/SonarQube/docker-compose.yml) y el scanner corre en el
# stack de runtime (servicio "tools"), que alcanza al server por
# host.docker.internal:9000.
#
# Uso:
#   export SONAR_TOKEN=squ_...        # o dejalo en el .env de la raíz
#   ./ControlCalidad/SonarQube/analizar.sh
# =========================================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/../../.." && pwd)"
RUNTIME_COMPOSE="$ROOT_DIR/docker-compose.yml"
QUALITY_COMPOSE="$ROOT_DIR/ControlCalidad/SonarQube/docker-compose.yml"
ROOT_ENV="$ROOT_DIR/.env"

PROJECT_KEY="${SONAR_PROJECT_KEY:-SolarUY}"
HOST_URL="${SONAR_HOST_URL:-http://localhost:9000}"
TOKEN="${SONAR_TOKEN:-}"

if [ -z "$TOKEN" ] && [ -f "$ROOT_ENV" ]; then
  TOKEN="$(grep -E '^SONAR_TOKEN=' "$ROOT_ENV" | head -1 | cut -d= -f2- | tr -d "\"'")"
fi

if [ -z "$TOKEN" ]; then
  echo "ERROR: falta SONAR_TOKEN."
  echo "Generá un token personal (ver ControlCalidad/SonarQube/README.md) y guardalo en el .env de la raíz:"
  echo "  echo \"SONAR_TOKEN=squ_...\" >> .env"
  exit 1
fi

ENV_FILE_ARGS=()
if [ -f "$ROOT_ENV" ]; then
  ENV_FILE_ARGS=(--env-file "$ROOT_ENV")
fi

echo "==> Levantando el stack de calidad (SonarQube)"
docker compose -f "$QUALITY_COMPOSE" "${ENV_FILE_ARGS[@]}" up -d

echo "==> Esperando a que SonarQube responda..."
READY=0
for i in $(seq 1 60); do
  if curl -fsS "$HOST_URL/api/system/status" >/dev/null 2>&1; then
    READY=1
    break
  fi
  sleep 5
done

if [ "$READY" -ne 1 ]; then
  echo "ERROR: SonarQube no respondió a tiempo en $HOST_URL"
  docker compose -f "$QUALITY_COMPOSE" "${ENV_FILE_ARGS[@]}" logs --tail 50 sonarqube
  exit 1
fi

echo "==> Analizando el proyecto $PROJECT_KEY"
docker compose -f "$RUNTIME_COMPOSE" "${ENV_FILE_ARGS[@]}" --profile tools run --rm tools \
  bash -lc 'set -e
    dotnet tool restore
    dotnet sonarscanner begin \
      /k:"$SONAR_PROJECT_KEY" \
      /d:sonar.host.url="$SONAR_HOST_URL" \
      /d:sonar.token="$SONAR_TOKEN"
    dotnet build
    dotnet sonarscanner end /d:sonar.token="$SONAR_TOKEN"'

echo
echo "==> Resultados en $HOST_URL/projects"
