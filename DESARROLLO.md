# Guia de desarrollo - SolarUY

Esta guia describe como preparar **cualquiera de las 4 maquinas del equipo** para trabajar con
SolarUY. El principio de diseño es simple: **todo corre dentro de Docker**. En la maquina solo
se instala Docker (Engine en Linux, Docker Desktop en Windows y macOS). No hace falta instalar
el SDK de .NET ni MySQL.

---

## 1. Requisitos

| Herramienta | Version | Nota |
| --- | --- | --- |
| Docker Engine | 24 o superior | Linux |
| Docker Desktop | 4.30 o superior | Windows y macOS (incluye Compose v2) |
| Docker Compose | v2 (`docker compose`) | Viene incluido en Docker Desktop |

Verificacion:

```bash
docker --version
docker compose version
```

### 1.1 Instalacion

**Windows (10/11)**: instalar [Docker Desktop for Windows](https://docs.docker.com/desktop/install/windows-install/).
Durante la instalacion aceptar WSL 2. Reiniciar la sesion de Windows si lo pide.

**macOS**: instalar [Docker Desktop for macOS](https://docs.docker.com/desktop/install/mac-install/)
(funciona con Intel y Apple Silicon). Otorgar los permisos de disco a Docker.

**Linux (Ubuntu/Debian)**:

```bash
sudo apt-get update
sudo apt-get install -y docker.io docker-compose-v2
sudo usermod -aG docker $USER      # para no usar sudo en cada comando
newgrp docker                       # aplica el cambio de grupo en la terminal actual
```

**Linux (Fedora/RHEL)**:

```bash
sudo dnf install -y docker docker-compose-plugin
sudo systemctl enable --now docker
sudo usermod -aG docker $USER && newgrp docker
```

---

## 2. Preparacion del repositorio

```bash
git clone https://github.com/Darlinx25/Taller-.NET---SolarUY.git
cd Taller-.NET---SolarUY

# Credenciales locales: base de datos MySQL, puerto y token de SonarQube
cp .env.example .env
```

Editar `.env` con los valores por defecto. Lo unico que **cada maquina debe cambiar** es el
`SONAR_TOKEN` (se explica en la seccion 5) y, en Linux, los permisos de archivos:

```ini
MYSQL_ROOT_PASSWORD=SolarUY.Local2026
MYSQL_APP_USER=solaruy
MYSQL_APP_PASSWORD=SolarUY.Local2026
MYSQL_DATABASE=SolarUY
MYSQL_PORT=3307

SONAR_TOKEN=
SONAR_PROJECT_KEY=SolarUY
SONAR_HOST_URL=http://localhost:9000
SONAR_PORT=9000

LOCAL_UID=1000
LOCAL_GID=1000
```

> `LOCAL_UID` y `LOCAL_GID` solo aplican en Linux. Sirven para que los archivos que escriben los
> contenedores (`bin/`, `obj/`, cache de NuGet) queden con el usuario de la maquina y no como
> `root`. En Windows y macOS Docker Desktop los ignora. Para obtenerlos:
> `id -u` y `id -g`.

> El mock del inversor (`simulador`) **no necesita configuracion**: la API key que comparte con la `app`
> (`SIMULACION_API_KEY`) tiene un valor por defecto de desarrollo en `docker-compose.yml`. Solo se define
> en `.env` si se quiere cambiarla.

> El puerto **3307** se usa porque en varias maquinas ya hay un MySQL nativo escuchando en el 3306.
> El puerto queda publicado solo en `127.0.0.1`, por lo que la base no es accesible desde la red.

---

## 3. Levantar la aplicacion

```bash
docker compose up -d
```

Esto levanta el stack completo:

| Servicio | Que hace | Como verificarlo |
| --- | --- | --- |
| `mysql` | MySQL 8.4 con el volumen `mysqldata` | `docker compose ps` -> `healthy` |
| `migrate` | Aplica las migraciones de EF Core (`dotnet ef database update`) y termina | `docker compose logs migrate` -> `Done.` |
| `redis` | Estado compartido: DataProtection, cache y backplane de SignalR | `docker compose ps` -> `healthy` |
| `keycloak` | IdP OIDC local con el realm `solaruy` importado | <http://localhost:8081> |
| `app` | Aplicacion ASP.NET Core (por defecto 2 replicas); no publica puertos | `docker compose logs -f app` |
| `nginx` | Balanceador que publica la aplicacion y los callbacks OIDC | <http://localhost:5080> |
| `simulador` | Mock de la placa ARM (inversor y sensores) compilado a WebAssembly y ejecutado con Node.js; obtiene la topologia y envia la telemetria cada 5 s al balanceador (`nginx`). No accede a MySQL ni publica puertos. Su codigo esta en [`Simulador/`](Simulador/README.md) | `docker compose logs -f simulador` |

Comprobaciones rapidas:

```bash
curl http://localhost:5080/health          # {"estado":"ok","baseDeDatos":"operativa"}
docker compose ps                           # mysql/redis healthy, app y nginx running
docker compose logs --tail 30 app
docker compose logs --tail 30 keycloak
```

La primera vez, Keycloak puede tardar unos segundos en importar el realm. Esperá a ver
`Running the server` en `docker compose logs keycloak` antes de iniciar sesión.

### 3.1 Acceso a la aplicacion

- Aplicacion (via nginx): <http://localhost:5080>
- Keycloak: <http://localhost:8081> (consola de administracion en `/admin`, usuario `KEYCLOAK_ADMIN` y password `KEYCLOAK_ADMIN_PASSWORD` del `.env`)
- MCP (solo lectura): <http://127.0.0.1:5080/mcp>
- Usuarios de desarrollo importados por el realm:
  - `admin@solaruy.uy` / `SolarUY.Local2026` -> rol `Administrador`
  - `operador@solaruy.uy` / `SolarUY.Local2026` -> rol `Operador`
- MySQL: `127.0.0.1:3307`, base `SolarUY`, usuario `solaruy`, password `SolarUY.Local2026`.

Si cambiás `APP_PORT` en el `.env`, actualizá tambien en `docker/keycloak/realm-solaruy.json` la
redirect URI (`http://localhost:<puerto>/signin-oidc`), el post logout (`http://localhost:<puerto>/*`)
y `Authentication__Oidc__PostLogoutRedirectUri` en `docker-compose.yml`. Keycloak valida que el
`redirect_uri` coincida exactamente.

El enlace *Iniciar sesión* de la barra superior redirige a Keycloak. Si no existe configuración
`Authentication:Oidc` (por ejemplo al ejecutar la app fuera de Docker), el mismo enlace cae en el
stub de desarrollo `/dev/login`.

```bash
docker compose exec mysql mysql -usolaruy -pSolarUY.Local2026 SolarUY
```

### 3.2 Verificar el mock del inversor

El `simulador` (placa ARM simulada en WebAssembly, ver [`Simulador/README.md`](Simulador/README.md)) genera una
lectura cada 5 segundos para cada parque registrado y la envia al balanceador (`POST /api/simulacion/telemetria`),
que la entrega a una replica de la `app`. La `app` conserva la ultima en la cache compartida (los resumenes diarios
para los historicos se persisten aparte). Se levanta solo con `docker compose up`; no hace falta abrir ninguna pagina.

```bash
docker compose logs -f simulador      # lectura generada y envio de cada tick
docker compose logs -f app            # ingesta de cada lectura
```

Para consultar la ultima lectura hay que iniciar sesion con rol `Administrador` y hacer
`GET /api/telemetria` (`204` antes de la primera lectura, `200` despues; sin sesion, `401`):

```bash
# En el navegador: http://localhost:5080 -> Iniciar sesión -> admin@solaruy.uy
# Alternativa de desarrollo (sin pasar por Keycloak):
curl -c cookies.txt http://localhost:5080/dev/login
curl -b cookies.txt http://localhost:5080/api/telemetria
```

La generacion es 0 W fuera de 07:00-19:00 (hora de Uruguay). Para ver potencia en cualquier momento, el
administrador puede usar los botones del dashboard (<http://localhost:5080/Dashboard>): **Forzar dia**
(genera como a las 13:00 sin importar la hora, con fallas aleatorias igual) y **Simular falla** (todos los paneles fallan y se
recuperan solos al azar, con 40 % por lectura). El efecto se ve en la lectura siguiente (hasta 5 s) y la pagina se actualiza sola. Mas detalle en
[`Simulador/README.md`](Simulador/README.md).

Alternativamente, se puede levantar un simulador efimero con otro desfase horario, por ejemplo:

```bash
docker run --rm --network solaruy_solaruy   -e SimulacionApi__UrlBase=http://nginx:80 -e SimulacionApi__ApiKey=SolarUY.Local2026   -e Simulador__DesfaseHorarioUruguayHoras=-9 solaruy-simulador
```

(con el servicio `simulador` detenido, para que no pise la lectura). Los endpoints internos exigen el
encabezado `X-Api-Key`; sin el responden `401`.

### 3.3 Consultas con Antigravity (MCP)

La `app` expone un servidor **MCP** (Model Context Protocol) de solo lectura en
`http://127.0.0.1:5080/mcp` (req011). Antigravity traduce las preguntas en lenguaje natural a
llamadas a las herramientas del servidor; la aplicacion no llama a ningun LLM ni necesita una key
de Gemini.

La configuracion del workspace esta en `.agents/mcp_config.json`:

```json
{
  "mcpServers": {
    "solaruy": {
      "serverUrl": "http://127.0.0.1:5080/mcp"
    }
  }
}
```

Para usarla, agregala a la configuracion MCP del workspace en Antigravity o copiala a la global
(`~/.gemini/config/mcp_config.json`). Usar `127.0.0.1` evita problemas de resolucion IPv6 de
`localhost`.

Herramientas: `listar_parques`, `obtener_topologia`, `obtener_estado_actual`,
`obtener_historico`, `obtener_alertas` y `listar_tareas_mantenimiento`. Son anonimas y de solo
lectura; el endpoint se puede apagar con `Mcp:Habilitado=false`.

Para verificar el servidor sin Antigravity se puede usar
[MCP Inspector](https://github.com/modelcontextprotocol/inspector) apuntando a
`http://127.0.0.1:5080/mcp`, o `curl` (el transporte es stateless, asi que
`tools/list` se puede llamar directo). Los ejemplos completos de las 6
herramientas estan en
[pruebas-curl.md](ControlCalidad/prompting/req011_servidor_mcp_consultas/pruebas-curl.md):

```bash
curl -s http://127.0.0.1:5080/mcp \
  -H 'Content-Type: application/json' \
  -H 'Accept: application/json, text/event-stream' \
  -d '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-06-18","capabilities":{},"clientInfo":{"name":"curl","version":"1.0"}}}'

curl -s http://127.0.0.1:5080/mcp \
  -H 'Content-Type: application/json' \
  -H 'Accept: application/json, text/event-stream' \
  -d '{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}'
```

---

## 4. Flujo de trabajo diario

Todos los comandos de .NET se ejecutan **dentro del contenedor de herramientas**, de modo que las
4 maquinas usan exactamente la misma version del SDK:

```bash
# Compilar
docker compose run --rm tools dotnet build SolarUY.sln

# Pruebas unitarias + guardarrailes de arquitectura (ArchUnitNET)
docker compose run --rm tools dotnet test SolarUY.sln

# Generar una migracion
docker compose run --rm tools dotnet ef migrations add NombreDeLaMigracion \
    --project SolarUY --startup-project SolarUY

# Aplicar migraciones (tambien lo hace el servicio migrate en cada `up`)
docker compose run --rm tools dotnet ef database update --project SolarUY --startup-project SolarUY

# Consola de Entity Framework
docker compose run --rm tools dotnet ef database update --project SolarUY --startup-project SolarUY --verbose
```

Comandos de Docker utiles:

```bash
docker compose down          # apaga todo (conserva los datos)
docker compose down -v       # apaga todo y borra el volumen de MySQL
docker compose up -d --build # reconstruye la imagen de la app
docker compose logs -f app
docker compose exec app bash
```

### 4.1 Trabajar con el IDE (opcional)

La imagen `tools` publica el codigo fuente en `/app`, asi que se puede abrir la carpeta del
repositorio en Rider o Visual Studio y conectar el depurador al contenedor, o simplemente
editar el codigo en el IDE y compilar con los comandos de la seccion anterior.

Si se prefiere trabajar con el SDK instalado en la maquina (no recomendado, pero posible):

1. Instalar el SDK de .NET 10 y MySQL 8.4 en la maquina.
2. La cadena de conexion de `appsettings.Development.json` ya apunta a `127.0.0.1:3307`.
3. `dotnet tool restore` para instalar `dotnet-ef` y `dotnet-sonarscanner` del manifiesto local.

---

## 5. Analisis estatico con SonarQube (opcional)

SonarQube corre en un **stack separado** (`ControlCalidad/SonarQube/docker-compose.yml`) porque
es tooling de calidad y no forma parte de la aplicacion.

Primer uso (una vez por maquina):

1. Levantar el servidor:

   ```bash
   docker compose -f ControlCalidad/SonarQube/docker-compose.yml up -d
   ```

2. Esperar a que este listo (la primera vez tarda 1-2 minutos) y entrar a <http://localhost:9000>
   con `admin` / `admin` (cambiar la clave cuando lo pida).
3. Generar un token personal: **My Account > Security > Generate Tokens** (tipo `User Token`).
4. Guardarlo en el `.env` de la raiz:

   ```ini
   SONAR_TOKEN=squ_xxxxxxxxxxxxxxxx
   ```

5. Analizar el proyecto:

   ```bash
   # Linux / macOS
   ./ControlCalidad/SonarQube/analizar.sh

   # Windows (PowerShell)
   .\ControlCalidad\SonarQube\analizar.ps1
   ```

El script levanta el stack de calidad, espera a que el servidor responda con estado `UP` y lanza
el escaner (`dotnet-sonarscanner`) en un contenedor efimero `solaruy-tools` conectado a la red
interna del stack de calidad. Los resultados quedan en <http://localhost:9000/projects>.

> El token **nunca** se sube al repositorio: `.env` esta en `.gitignore` y `.env.example` lo
> deja vacio a proposito.

---

## 6. Problemas frecuentes

| Sintoma | Causa probable | Solucion |
| --- | --- | --- |
| `bind: address already in use` en el puerto 3307 | MySQL nativo o otro contenedor en ese puerto | Cambiar `MYSQL_PORT` en `.env` (por ejemplo 3308) |
| `permission denied` al escribir `bin/` u `obj/` | Los archivos quedaron como `root` | `sudo chown -R $USER:$USER .` y revisar `LOCAL_UID`/`LOCAL_GID` |
| `Access to the path '/nuget/...' is denied` en `migrate`/`tools` | El cache `.docker-cache` quedó como `root` | `sudo chown -R $USER:$USER .docker-cache` |
| `Duplicate column name ...` en `migrate` | El volumen MySQL conserva un esquema viejo | `docker compose down -v` (borra datos locales) y volver a levantar |
| La app no conecta con la base | MySQL no esta listo | `docker compose ps` y esperar a `healthy`; `docker compose logs migrate` |
| `dotnet` no encontrado | Se ejecuto fuera del contenedor | Usar `docker compose run --rm tools dotnet ...` |
| El puerto 5080 esta ocupado | Otra app local o un contenedor viejo | Cambiar `APP_PORT` en `.env`; si el stack actual no muestra el proceso, revisar `docker compose ps -a` |
| El puerto 5080 sigue ocupado tras `docker compose down` | Quedo un Docker Engine local (systemd) separado de Docker Desktop con contenedores viejos | `sudo systemctl disable --now docker.socket docker` y volver a levantar el stack de Docker Desktop |
| `502 Bad Gateway` en `/signin-oidc` | nginx no bufferiza las cookies/headers OIDC o conserva la IP vieja de una replica | Reiniciar nginx (`docker compose restart nginx`); la config actual usa `resolver` dinamico y buffers de 32k |
| `Correlation failed` o `message.State is null` en `/signin-oidc` | La cookie de correlacion no volvio al callback (dominio/puerto distinto o callback reenviado) | Usar siempre el mismo host y puerto (`http://localhost:5080`) y repetir el login desde cero |
| SonarQube no levanta | Falta memoria (necesita ~2 GB) | `docker stats`; apagar el stack de runtime mientras se analiza |

Para empezar de cero (borra la base y las migraciones aplicadas):

```bash
docker compose down -v
docker compose up -d
```
