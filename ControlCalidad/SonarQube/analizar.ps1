param([switch]$SkipWait)

# =========================================================================
# Análisis estático con SonarQube.
#
# El servidor de SonarQube vive en el stack de calidad
# (ControlCalidad/SonarQube/docker-compose.yml) y el scanner corre en el
# stack de runtime (servicio "tools"), que alcanza al server por
# host.docker.internal:9000.
#
# Uso:
#   $env:SONAR_TOKEN = "squ_..."   # o dejalo en el .env de la raíz
#   .\ControlCalidad\SonarQube\analizar.ps1
# =========================================================================

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$RootDir = (Resolve-Path (Join-Path $ScriptDir "..\..\..")).Path
$RuntimeCompose = Join-Path $RootDir "docker-compose.yml"
$QualityCompose = Join-Path $RootDir "ControlCalidad\SonarQube\docker-compose.yml"
$RootEnv = Join-Path $RootDir ".env"

$ProjectKey = if ($env:SONAR_PROJECT_KEY) { $env:SONAR_PROJECT_KEY } else { "SolarUY" }
$HostUrl = if ($env:SONAR_HOST_URL) { $env:SONAR_HOST_URL } else { "http://localhost:9000" }

$Token = $env:SONAR_TOKEN
if (-not $Token -and (Test-Path $RootEnv)) {
    $line = Select-String -Path $RootEnv -Pattern '^SONAR_TOKEN=' | Select-Object -First 1
    if ($line) { $Token = ($line.Line -replace '^SONAR_TOKEN=', '').Trim('"', "'") }
}

if (-not $Token) {
    Write-Host "ERROR: falta SONAR_TOKEN." -ForegroundColor Red
    Write-Host "Generá un token personal (ver ControlCalidad/SonarQube/README.md) y guardalo en el .env de la raíz."
    exit 1
}

$EnvFileArgs = @()
if (Test-Path $RootEnv) { $EnvFileArgs = @("--env-file", $RootEnv) }

Write-Host "==> Levantando el stack de calidad (SonarQube)"
& docker compose -f $QualityCompose @EnvFileArgs up -d
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if (-not $SkipWait) {
    Write-Host "==> Esperando a que SonarQube responda..."
    $ready = $false
    for ($i = 0; $i -lt 60; $i++) {
        try {
            Invoke-WebRequest -Uri "$HostUrl/api/system/status" -UseBasicParsing -TimeoutSec 5 | Out-Null
            $ready = $true
            break
        } catch {
            Start-Sleep -Seconds 5
        }
    }
    if (-not $ready) {
        Write-Host "ERROR: SonarQube no respondió a tiempo en $HostUrl" -ForegroundColor Red
        & docker compose -f $QualityCompose @EnvFileArgs logs --tail 50 sonarqube
        exit 1
    }
}

Write-Host "==> Analizando el proyecto $ProjectKey"
& docker compose -f $RuntimeCompose @EnvFileArgs --profile tools run --rm tools bash -lc @'
set -e
dotnet tool restore
dotnet sonarscanner begin \
  /k:"$SONAR_PROJECT_KEY" \
  /d:sonar.host.url="$SONAR_HOST_URL" \
  /d:sonar.token="$SONAR_TOKEN"
dotnet build
dotnet sonarscanner end /d:sonar.token="$SONAR_TOKEN"
'@
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host ""
Write-Host "==> Resultados en $HostUrl/projects"
