# Entorno SonarQube Local para .NET

Esta carpeta contiene la configuración necesaria para levantar un servidor de SonarQube local y analizar el proyecto SolarUY **sin instalar SDKs de manera global en la máquina host**: el escáner se ejecuta en un contenedor de herramientas (`solaruy-tools`) conectado a la red interna del stack de calidad.

> El servidor de SonarQube es *tooling de calidad*, no aplicación: por eso tiene su propio
> `docker-compose.yml` y no se despliega junto con SolarUY.

## Requisitos
- **Docker** instalado y funcionando (Docker Engine en Linux, o Docker Desktop en Windows/Mac).

---

## 1. Levantar el Servidor SonarQube

1. Desde la **raíz del repositorio**:
   ```bash
   docker compose -f ControlCalidad/SonarQube/docker-compose.yml up -d
   ```
2. Espera un par de minutos a que el servidor inicie (la primera vez es más lento).
   ```bash
   docker compose -f ControlCalidad/SonarQube/docker-compose.yml logs -f sonarqube
   ```
3. Abre en tu navegador: **http://localhost:9000**
4. Inicia sesión con:
   - **Usuario:** `admin`
   - **Contraseña:** `admin`
5. El sistema te pedirá cambiar la contraseña por defecto.

### 1.1 Crear el Token de Seguridad
Para que el analizador se comunique con el servidor, necesita un Token:
1. En SonarQube, ve a **My Account** (arriba a la derecha, click en el icono de usuario) > **Security**.
2. En **Generate Tokens**, ingresa un nombre (ej. `mi-token-local`), selecciona tipo `User Token`, y sin expiración (o a conveniencia).
3. **Guarda muy bien este Token**, lo necesitarás para analizar tu código.

> **NOTA IMPORTANTE:** Al iniciar sesión, la pantalla de inicio de SonarQube te invitará a crear un proyecto ("Create a local project"). **No es necesario que lo hagas.** El script se encarga de crear el proyecto en SonarQube de forma automática la primera vez que analice tu código.

### 1.2 Guardar el token en tu máquina

El token es **personal de cada máquina** y no se sube al repositorio. Se guarda en el `.env` de la raíz:

```bash
echo "SONAR_TOKEN=squ_xxxxxxxxxxxxxxxx" >> .env
```

(`.env` está en `.gitignore`; `.env.example` lo deja vacío a propósito.)

---

## 2. Cómo Analizar tu Código

Los scripts son los mismos para todos: `analizar.sh` (Linux/Mac) y `analizar.ps1` (Windows). No hay que editarlos ni pasarles rutas.

**En Linux/Mac:**
```bash
./ControlCalidad/SonarQube/analizar.sh
```

**En Windows (PowerShell):**
```powershell
.\ControlCalidad\SonarQube\analizar.ps1
```

¿Qué hace el script?

1. Verifica que exista `SONAR_TOKEN` (en el entorno o en el `.env` de la raíz) y avisa si falta.
2. Levanta el stack de calidad (`sonarqube` + `sonarqube-db`) si no está arriba.
3. Espera a que el servidor responda en `/api/system/status` con estado `UP`.
4. Lanza el escáner en un contenedor efímero `solaruy-tools` dentro de la red del stack de calidad:
   ```bash
   docker run --rm --network solaruy-quality_quality \
     -e SONAR_HOST_URL=http://sonarqube:9000 ...
     solaruy-tools
   ```
   Dentro del contenedor ejecuta `dotnet sonarscanner begin`, `dotnet build` y `dotnet sonarscanner end`.
5. Muestra la URL con los resultados.

Variables opcionales (en el `.env` de la raíz):

| Variable | Por defecto | Para qué sirve |
| --- | --- | --- |
| `SONAR_TOKEN` | (vacío) | Token personal, obligatorio |
| `SONAR_PROJECT_KEY` | `SolarUY` | Clave del proyecto en SonarQube |
| `SONAR_HOST_URL` | `http://localhost:9000` | URL del servidor **desde el host** (la usa el script para esperar y para el mensaje final); el escáner usa `http://sonarqube:9000` dentro de la red de calidad |

---

## 3. Cómo Leer tus Resultados en SonarQube

La interfaz de SonarQube tiene mucha información. Para no perderte, enfócate en estas **3 pestañas principales** (ubicadas en el menú superior al entrar a tu proyecto):

### A. Overview (Resumen)
Es la portada de tu proyecto. Lo más importante aquí es el recuadro grande que dice **Passed** o **Failed** (el "Quality Gate"). Te indica rápidamente si tu código cumple con los estándares mínimos de calidad de la industria.

### B. Issues (Problemas)
¡Esta es tu área de trabajo! Aquí verás una lista detallada de todos los errores, bugs y "Code Smells" (malas prácticas).
* **Tip de Aprendizaje:** Si haces clic en un problema, SonarQube te mostrará la línea exacta de código donde te equivocaste. Si haces clic en la pestaña **"Why is this an issue?"** (¿Por qué es un problema?), te dará una explicación teórica de por qué esa práctica es mala y cómo se escribe correctamente en C#.

### C. Measures (Medidas)
Aquí encontrarás gráficas y métricas detalladas del proyecto completo.
* **¿Cómo ver el Código Duplicado?** La forma más fácil de verlo en las versiones nuevas de SonarQube es directamente desde la pestaña **Overview**. En la parte inferior derecha verás un indicador llamado **Duplications** con un porcentaje. Si haces clic en ese número, el sistema te llevará exactamente a los archivos y a las líneas de código que copiaste y pegaste.

---

## 4. Apagar el servidor

```bash
docker compose -f ControlCalidad/SonarQube/docker-compose.yml down      # conserva los datos
docker compose -f ControlCalidad/SonarQube/docker-compose.yml down -v   # borra el historial de análisis
```

SonarQube necesita ~2 GB de RAM: si la maquina va justa, apagalo mientras se trabaja con el stack de runtime.
