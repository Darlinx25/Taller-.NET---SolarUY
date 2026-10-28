param([switch]$SkipWait)

# =========================================================================
# Análisis estático con SonarQube.
#
# El servidor de SonarQube vive en el stack de calidad
# (ControlCalidad/SonarQube/docker-compose.yml) y el scanner corre en un
# contenedor de herramientas conectado a la red de ese stack. Así el server
# sigue publicado solo en 127.0.0.1 y no hace falta exponerlo en la LAN.
#
# Uso:
#   $env:SONAR_TOKEN = "squ_..."   # o dejalo en el .env de la raíz
#   .\ControlCalidad\SonarQube\analizar.ps1
# =========================================================================

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$RootDir = (Resolve-Path (Join-Path $ScriptDir "..\..")).Path
$RuntimeCompose = Join-Path $RootDir "docker-compose.yml"
$QualityCompose = Join-Path $RootDir "ControlCalidad\SonarQube\docker-compose.yml"
$RootEnv = Join-Path $RootDir ".env"

function Read-EnvValue([string]$Key) {
    if (-not (Test-Path $RootEnv)) { return $null }
    $line = Select-String -Path $RootEnv -Pattern "^$Key=" | Select-Object -First 1
    if (-not $line) { return $null }
    return ($line.Line -replace "^$Key=", '').Trim('"', "'")
}

$ProjectKey = if ($env:SONAR_PROJECT_KEY) { $env:SONAR_PROJECT_KEY } elseif (Read-EnvValue "SONAR_PROJECT_KEY") { Read-EnvValue "SONAR_PROJECT_KEY" } else { "SolarUY" }
$HostUrl = if ($env:SONAR_HOST_URL) { $env:SONAR_HOST_URL } elseif (Read-EnvValue "SONAR_HOST_URL") { Read-EnvValue "SONAR_HOST_URL" } else { "http://localhost:9000" }
$LocalUid = if ($env:LOCAL_UID) { $env:LOCAL_UID } elseif (Read-EnvValue "LOCAL_UID") { Read-EnvValue "LOCAL_UID" } else { "1000" }
$LocalGid = if ($env:LOCAL_GID) { $env:LOCAL_GID } elseif (Read-EnvValue "LOCAL_GID") { Read-EnvValue "LOCAL_GID" } else { "1000" }

$Token = $env:SONAR_TOKEN
if (-not $Token) { $Token = Read-EnvValue "SONAR_TOKEN" }

if (-not $Token) {
    Write-Host "ERROR: falta SONAR_TOKEN." -ForegroundColor Red
    Write-Host "Generá un token personal (ver ControlCalidad/SonarQube/README.md) y guardalo en el .env de la raíz."
    exit 1
}

$EnvFileArgs = @()
if (Test-Path $RootEnv) { $EnvFileArgs = @("--env-file", $RootEnv) }

$ToolsImage = (& docker compose -f $RuntimeCompose @EnvFileArgs --profile tools config --images tools | Select-Object -First 1)
if (-not $ToolsImage) {
    Write-Host "ERROR: no se pudo resolver la imagen del servicio tools." -ForegroundColor Red
    exit 1
}
& docker image inspect $ToolsImage *> $null
if ($LASTEXITCODE -ne 0) {
    Write-Host "==> Construyendo la imagen de herramientas ($ToolsImage)"
    & docker compose -f $RuntimeCompose @EnvFileArgs --profile tools build tools
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Write-Host "==> Levantando el stack de calidad (SonarQube)"
& docker compose -f $QualityCompose @EnvFileArgs up -d
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if (-not $SkipWait) {
    Write-Host "==> Esperando a que SonarQube responda..."
    $ready = $false
    for ($i = 0; $i -lt 60; $i++) {
        try {
            $status = Invoke-RestMethod -Uri "$HostUrl/api/system/status" -TimeoutSec 5
            if ($status.status -eq "UP") {
                $ready = $true
                break
            }
            Start-Sleep -Seconds 5
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

$SonarCid = (& docker compose -f $QualityCompose @EnvFileArgs ps -q sonarqube | Select-Object -First 1)
if (-not $SonarCid) {
    Write-Host "ERROR: no se encontró el contenedor de SonarQube." -ForegroundColor Red
    exit 1
}
$QualityNetwork = (& docker inspect -f '{{range $name, $_ := .NetworkSettings.Networks}}{{$name}}{{end}}' $SonarCid)
$QualityNetwork = "$QualityNetwork".Trim()
if (-not $QualityNetwork) {
    Write-Host "ERROR: no se pudo determinar la red de SonarQube." -ForegroundColor Red
    exit 1
}

$NugetCache = Join-Path $RootDir ".docker-cache\nuget"
New-Item -ItemType Directory -Force -Path $NugetCache | Out-Null

Write-Host "==> Analizando el proyecto $ProjectKey"
$ScannerScript = @'
set -e
dotnet tool restore
dotnet sonarscanner begin \
  /k:"$SONAR_PROJECT_KEY" \
  /d:sonar.host.url="$SONAR_HOST_URL" \
  /d:sonar.token="$SONAR_TOKEN" \
  /d:sonar.exclusions=SolarUY/Migrations/**
dotnet build SolarUY.sln
dotnet sonarscanner end /d:sonar.token="$SONAR_TOKEN"
'@
$DockerArgs = @(
    "run", "--rm",
    "--network", $QualityNetwork,
    "--user", "$($LocalUid):$($LocalGid)",
    "-v", "$($RootDir):/app",
    "-v", "$($NugetCache):/nuget",
    "-w", "/app",
    "-e", "DOTNET_CLI_HOME=/tmp/dh",
    "-e", "NUGET_PACKAGES=/nuget",
    "-e", "DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1",
    "-e", "DOTNET_NOLOGO=1",
    "-e", "SONAR_TOKEN=$Token",
    "-e", "SONAR_HOST_URL=http://sonarqube:9000",
    "-e", "SONAR_PROJECT_KEY=$ProjectKey",
    "--entrypoint", "bash",
    $ToolsImage, "-lc", $ScannerScript
)
& docker @DockerArgs
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host ""
Write-Host "==> Resultados en $HostUrl/projects"
