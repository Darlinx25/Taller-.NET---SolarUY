#!/bin/bash
set -euo pipefail

# =========================================================================
# Análisis estático con SonarQube.
#
# El servidor de SonarQube vive en el stack de calidad
# (ControlCalidad/SonarQube/docker-compose.yml) y el scanner corre en un
# contenedor de herramientas conectado a la red de ese stack. Así el server
# sigue publicado solo en 127.0.0.1 y no hace falta exponerlo en la LAN.
#
# Uso:
#   export SONAR_TOKEN=squ_...        # o dejalo en el .env de la raíz
#   ./ControlCalidad/SonarQube/analizar.sh
# =========================================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"
RUNTIME_COMPOSE="$ROOT_DIR/docker-compose.yml"
QUALITY_COMPOSE="$ROOT_DIR/ControlCalidad/SonarQube/docker-compose.yml"
ROOT_ENV="$ROOT_DIR/.env"

leer_env() {
  if [ -f "$ROOT_ENV" ]; then
    grep -E "^$1=" "$ROOT_ENV" | head -1 | cut -d= -f2- | tr -d "\"'"
  fi
}

PROJECT_KEY="${SONAR_PROJECT_KEY:-$(leer_env SONAR_PROJECT_KEY)}"
PROJECT_KEY="${PROJECT_KEY:-SolarUY}"
HOST_URL="${SONAR_HOST_URL:-$(leer_env SONAR_HOST_URL)}"
HOST_URL="${HOST_URL:-http://localhost:9000}"
TOKEN="${SONAR_TOKEN:-$(leer_env SONAR_TOKEN)}"
LOCAL_UID="${LOCAL_UID:-$(leer_env LOCAL_UID)}"
LOCAL_UID="${LOCAL_UID:-$(id -u)}"
LOCAL_GID="${LOCAL_GID:-$(leer_env LOCAL_GID)}"
LOCAL_GID="${LOCAL_GID:-$(id -g)}"

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

TOOLS_IMAGE="$(docker compose -f "$RUNTIME_COMPOSE" "${ENV_FILE_ARGS[@]}" --profile tools config --images tools | head -1)"
if [ -z "$TOOLS_IMAGE" ]; then
  echo "ERROR: no se pudo resolver la imagen del servicio tools."
  exit 1
fi
if ! docker image inspect "$TOOLS_IMAGE" >/dev/null 2>&1; then
  echo "==> Construyendo la imagen de herramientas ($TOOLS_IMAGE)"
  docker compose -f "$RUNTIME_COMPOSE" "${ENV_FILE_ARGS[@]}" --profile tools build tools
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

SONAR_CONTAINER_ID="$(docker compose -f "$QUALITY_COMPOSE" "${ENV_FILE_ARGS[@]}" ps -q sonarqube | head -1)"
if [ -z "$SONAR_CONTAINER_ID" ]; then
  echo "ERROR: no se encontró el contenedor de SonarQube."
  exit 1
fi
QUALITY_NETWORK="$(docker inspect -f '{{range $name, $_ := .NetworkSettings.Networks}}{{$name}}{{end}}' "$SONAR_CONTAINER_ID")"
if [ -z "$QUALITY_NETWORK" ]; then
  echo "ERROR: no se pudo determinar la red de SonarQube."
  exit 1
fi

mkdir -p "$ROOT_DIR/.docker-cache/nuget"

echo "==> Analizando el proyecto $PROJECT_KEY"
docker run --rm \
  --network "$QUALITY_NETWORK" \
  --user "$LOCAL_UID:$LOCAL_GID" \
  -v "$ROOT_DIR:/app" \
  -v "$ROOT_DIR/.docker-cache/nuget:/nuget" \
  -w /app \
  -e DOTNET_CLI_HOME=/tmp/dh \
  -e NUGET_PACKAGES=/nuget \
  -e DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
  -e DOTNET_NOLOGO=1 \
  -e SONAR_TOKEN="$TOKEN" \
  -e SONAR_HOST_URL="http://sonarqube:9000" \
  -e SONAR_PROJECT_KEY="$PROJECT_KEY" \
  --entrypoint bash \
  "$TOOLS_IMAGE" -lc 'set -e
    dotnet tool restore
    dotnet sonarscanner begin \
      /k:"$SONAR_PROJECT_KEY" \
      /d:sonar.host.url="$SONAR_HOST_URL" \
      /d:sonar.token="$SONAR_TOKEN"
    dotnet build SolarUY.sln
    dotnet sonarscanner end /d:sonar.token="$SONAR_TOKEN"'

echo
echo "==> Resultados en $HOST_URL/projects"
