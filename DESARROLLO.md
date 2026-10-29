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

Esto levanta cuatro servicios:

| Servicio | Que hace | Como verificarlo |
| --- | --- | --- |
| `mysql` | MySQL 8.4 con el volumen `mysqldata` | `docker compose ps` -> `healthy` |
| `migrate` | Aplica las migraciones de EF Core (`dotnet ef database update`) y termina | `docker compose logs migrate` -> `Done.` |
| `app` | Publica la API web | <http://localhost:5080> |
| `simulador` | Mock del inversor: componente independiente que obtiene la topologia de `app` y le envia la telemetria cada 5 s. No accede a MySQL ni publica puertos | `docker compose logs -f simulador` |

Comprobaciones rapidas:

```bash
curl http://localhost:5080/health          # {"estado":"ok","baseDeDatos":"operativa"}
docker compose ps                           # mysql y app healthy, simulador running
docker compose logs --tail 30 app
```

### 3.1 Acceso a la aplicacion

- Aplicacion: <http://localhost:5080>
- En `Development` hay un **stub de autenticacion**: el enlace *Acceso dev* de la barra superior
  (o <http://localhost:5080/dev/login>) firma un usuario con rol `Administrador`.
  El proveedor real (OpenID Connect) se integra en una iteracion posterior.
- MySQL: `127.0.0.1:3307`, base `SolarUY`, usuario `solaruy`, password `SolarUY.Local2026`.

```bash
docker compose exec mysql mysql -usolaruy -pSolarUY.Local2026 SolarUY
```

### 3.2 Verificar el mock del inversor

El `simulador` genera una lectura cada 5 segundos para cada parque registrado y la envia a la `app`
(`POST /api/simulacion/telemetria`), que conserva solo la ultima en memoria.

```bash
docker compose logs -f simulador      # lectura generada y envio de cada tick
docker compose logs -f app            # ingesta de cada lectura
```

Para consultar la ultima lectura hay que iniciar sesion como Administrador (stub de desarrollo) y
hacer `GET /api/telemetria` (`204` antes de la primera lectura, `200` despues; sin sesion, `401`):

```bash
curl -c cookies.txt http://localhost:5080/dev/login
curl -b cookies.txt http://localhost:5080/api/telemetria
```

La generacion es 0 W fuera de 07:00-19:00 (hora de Uruguay). Para ver potencia fuera de ese horario
se puede levantar un simulador efimero con otro desfase horario, por ejemplo:

```bash
docker run --rm --network solaruy_solaruy   -e SimulacionApi__UrlBase=http://app:8080 -e SimulacionApi__ApiKey=SolarUY.Local2026   -e Simulador__DesfaseHorarioUruguayHoras=-9 solaruy-simulador
```

(con el servicio `simulador` detenido, para que no pise la lectura). Los endpoints internos exigen el
encabezado `X-Api-Key`; sin el responden `401`.

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
| La app no conecta con la base | MySQL no esta listo | `docker compose ps` y esperar a `healthy`; `docker compose logs migrate` |
| `dotnet` no encontrado | Se ejecuto fuera del contenedor | Usar `docker compose run --rm tools dotnet ...` |
| El puerto 5080 esta ocupado | Otra app local | Cambiar `APP_PORT` en `.env` |
| SonarQube no levanta | Falta memoria (necesita ~2 GB) | `docker stats`; apagar el stack de runtime mientras se analiza |

Para empezar de cero (borra la base y las migraciones aplicadas):

```bash
docker compose down -v
docker compose up -d
```
