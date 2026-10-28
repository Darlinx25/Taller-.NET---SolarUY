# Herramientas de Control de Calidad - Taller .NET

Este directorio contiene las herramientas y configuraciones necesarias para garantizar la calidad, el diseño y la arquitectura del código del Taller .NET.

El enfoque utilizado combina el análisis de métricas estáticas con validaciones arquitectónicas, la asistencia guiada por Inteligencia Artificial y la metodología **Spec-Driven Development (SDD)** con prompting trazable.

---

## 1. Metodología SDD + prompting (`/prompting`)

El flujo completo, los gates de aprobación y la convención de nombres están en
[`prompting/README.md`](prompting/README.md). Por cada requerimiento hay una
carpeta con los tres artefactos de la metodología, en este orden:

```
prompting/<requerimiento>/
├── requirements.md   # qué se necesita: historia de usuario, criterios de aceptación y reglas de negocio
├── specs.md          # cómo se resuelve: capas Domain / Application / Infrastructure / Presentation
└── tasks.md          # lista de tareas verificables para implementar la spec
```

Las plantillas para requerimientos nuevos están en
[`prompting/_plantillas/`](prompting/_plantillas/). Los documentos de req001 a
req004 no se reescriben con las plantillas.

Requerimientos:

| Requerimiento | Descripción | Estado |
| --- | --- | --- |
| `req001_alta_parque_solar` | Alta de parque solar (Administrador) | Implementado |
| `req002_gestion_parques` | Detalle, edicion y baja de parques solares (Administrador) | Implementado |
| `req003_nodos_inversores` | Nodos e inversores por parque solar (Administrador) | Implementado |
| `req004_gestion_paneles` | Paneles fotovoltaicos por nodo (Administrador) | Implementado |

Estados: **Planificado** (solo artefactos), **En progreso** (implementación
iniciada) e **Implementado** (cerrado con tests y análisis estático).

Nota: req004 se cierra con deuda aceptada de SonarQube (gate en ERROR por
cobertura no reportada 0% y duplicación nueva 9,4%; 0 defectos).

Reglas de uso (definidas en `/agents/rules/6_flujo_trabajo_ia.md`):

1. No se empieza a codear sin `requirements.md`, `specs.md` y `tasks.md` revisados y aprobados.
2. `specs.md` se genera con la skill `sdd-generar-specs` y `tasks.md` con `sdd-generar-tasks`.
3. Cada cambio en el código se valida ejecutando los tests (sección 2).
4. Si un test arquitectónico falla, se corrige el **código**, nunca el test.

---

## 2. Validación de Código (`/ValidacionDeCodigo`)

### A. Reglas para el Agente IA (`/agents/rules`)

Directrices explícitas sobre cómo debe estructurarse el código: Clean Architecture, SOLID, nomenclatura (Spanglish técnico), modelado de dominio rico, capa de presentación "tonta" y flujo de trabajo obligatorio del agente.

### B. Pruebas Deterministas / Guardarraíles (`/ValidacionDeCodigo/CodeQuality.Tests`)

Proyecto de pruebas unitarias implementado con **ArchUnitNET**. Mientras que el agente de IA es una ayuda proactiva, estos tests actúan como **guardarraíles** deterministas:

- El Dominio no depende de Entity Framework Core.
- El Dominio no depende de Application, Infrastructure ni Presentación.
- Application no depende de Infrastructure ni Presentación.
- La Presentación no accede directamente a Infrastructure.
- Las implementaciones de repositorio viven en Infrastructure.
- Convenciones de nomenclatura: interfaces con prefijo `I`, excepciones con sufijo `Exception`.

**Configuración en este repositorio:** el proyecto ya está agregado a `SolarUY.sln` y el `ArchitectureSetup` apunta al ensamblado de `SolarUY` (monolito en capas, un único `.csproj`).

Ejecución (dentro del contenedor de herramientas, sin instalar el SDK en la máquina):

```bash
docker compose run --rm tools dotnet test SolarUY.sln
```

O solo los guardarraíles:

```bash
docker compose run --rm tools dotnet test ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests
```

> ⚠️ **Regla de integridad:** los archivos de `CodeQuality.Tests` son inmutables. Si una regla
> entra en conflicto con el diseño, hay que corregir el código o pedir autorización explícita
> para ajustar el test (ver `/agents/rules/6_flujo_trabajo_ia.md`, sección 6).

### C. Pruebas unitarias del dominio (`/SolarUY.Tests`)

Proyecto xUnit propio con las pruebas del comportamiento del dominio (obligatorias por la regla 5
del flujo de trabajo). Para las capas Application, Infrastructure y Presentación no se generan
pruebas unitarias.

```bash
docker compose run --rm tools dotnet test SolarUY.Tests
```

---

## 3. 📊 SonarQube (`/SonarQube`)

Configuración (compose + scripts) para levantar y utilizar **SonarQube**:

- Detecta **Código Duplicado** (copy/paste).
- Encuentra **Bugs** y vulnerabilidades de seguridad.
- Detecta **Code Smells** (complejidad ciclomática, clases gigantes, etc.).

Funciona como un **stack separado** del runtime (`docker-compose.yml` propio con `sonarqube` y
`sonarqube-db`): es tooling de calidad, no aplicación. El escáner corre en un contenedor efímero
`solaruy-tools` dentro de la red interna del stack de calidad y alcanza al servidor como
`http://sonarqube:9000`.

Detalle paso a paso (token personal, script para Linux/macOS y para Windows):
[SonarQube/README.md](SonarQube/README.md).

```bash
# Levantar el servidor de calidad
docker compose -f ControlCalidad/SonarQube/docker-compose.yml up -d

# Analizar (Linux/macOS)
./ControlCalidad/SonarQube/analizar.sh
```

El token se configura en el `.env` de la raíz (`SONAR_TOKEN=...`) y **nunca** se sube al repositorio.

---

## 4. Resumen: qué corre y cuándo

| Herramienta | Dónde vive | Cuándo se ejecuta | Comando |
| --- | --- | --- | --- |
| Guardarraíles ArchUnitNET | `ValidacionDeCodigo/CodeQuality.Tests` | En cada cambio de código | `docker compose run --rm tools dotnet test SolarUY.sln` |
| Tests xUnit del dominio | `SolarUY.Tests` | En cada cambio de código | `docker compose run --rm tools dotnet test SolarUY.Tests` |
| SonarQube | `SonarQube/` | Antes de cada entrega / cierre de iteración | `./ControlCalidad/SonarQube/analizar.sh` |
