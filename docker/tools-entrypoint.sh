#!/bin/sh
# Entrypoint de la imagen de herramientas.
# Restaura las herramientas locales (.config/dotnet-tools.json) y ejecuta
# el comando solicitado. Así `docker compose run --rm tools dotnet ef ...`
# funciona sin pasos extra.
set -e

if [ -f ".config/dotnet-tools.json" ]; then
    dotnet tool restore >/dev/null 2>&1 || true
fi

exec "$@"
