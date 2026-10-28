# Flujo SDD de SolarUY

Este documento define como se crea, revisa y ejecuta un requerimiento en
SolarUY. La regla obligatoria es
[`../.agents/rules/6_flujo_trabajo_ia.md`](../.agents/rules/6_flujo_trabajo_ia.md)
y las plantillas viven en [`_plantillas/`](_plantillas/).

## 1. Convencion de nombres

```text
ControlCalidad/prompting/
├── README.md                  # este documento
├── _plantillas/               # moldes; NO es un requerimiento
│   ├── requirements.md
│   ├── specs.md
│   └── tasks.md
├── req001_alta_parque_solar/
│   ├── requirements.md
│   ├── specs.md
│   └── tasks.md
├── req002_gestion_parques/
├── req003_nodos_inversores/
└── req004_gestion_paneles/
```

- `NNN`: numero de tres cifras (`001`, `002`, ...).
- `slug`: nombre corto en minusculas y con guiones bajos
  (`gestion_paneles`, `nodos_inversores`).
- Los tres archivos siempre se llaman `requirements.md`, `specs.md` y
  `tasks.md`.
- No se crean subcarpetas dentro de la carpeta de un requerimiento.
- Los documentos existentes de req001 a req004 no se reescriben con las
  plantillas. Las plantillas son solo para requerimientos nuevos.

## 2. Pipeline

Cada paso tiene una salida concreta y un gate de aprobacion humana. No se
avanza al paso siguiente sin aprobar el anterior.

| Paso | Entrada | Salida | Responsable | Gate |
| --- | --- | --- | --- | --- |
| 1. Requerimiento | Puede ser la letra del taller o una necesidad nueva | `requirements.md` | Humano + IA | **Gate R** |
| 2. Specs | `requirements.md` + rules + codigo | `specs.md` | Skill `sdd-generar-specs` | **Gate S** |
| 3. Plan | `specs.md` + plantilla + codigo | `tasks.md` | Skill `sdd-generar-tasks` | **Gate T** |
| 4. Implementacion | `tasks.md` aprobado | Codigo + tests + `tasks.md` con `[x]` | IA | **Gate C** |
| 5. Cierre | Implementacion completa | Estado `Implementado` y READMEs | IA + humano | **Gate D** |

### Paso 1: crear el requerimiento

1. Crear la carpeta `ControlCalidad/prompting/reqNNN_slug/`.
2. Copiar `_plantillas/requirements.md` y completarlo en lenguaje de negocio.
3. No incluir detalles tecnicos de implementacion.

**Gate R:** una persona revisa que la historia, los criterios de aceptacion y
las reglas de negocio esten completos. Si falta algo, se pregunta antes de
continuar.

### Paso 2: generar la spec

Pedirle a la IA:

```text
Usando la skill sdd-generar-specs, genera specs.md para
ControlCalidad/prompting/reqNNN_slug siguiendo
ControlCalidad/prompting/README.md.
```

La skill debe leer el requerimiento, las rules, la plantilla
`_plantillas/specs.md` y el codigo existente para imitar patrones.

**Gate S:** revisar que:

- [ ] Esten las cuatro capas: Domain, Application, Infrastructure y Presentation.
- [ ] Los nombres sean exactos (`ParqueSolar`, `IPanelRepository`, `PanelService`, etc.).
- [ ] No queden decisiones abiertas ("a definir", "evaluar", "segun convencion").
- [ ] Este claro si hay migracion o no.
- [ ] Esten el registro en `Program.cs`, la autorizacion y las validaciones.
- [ ] Esten definidos los tests de Dominio obligatorios.

### Paso 3: generar el plan

Pedirle a la IA:

```text
Usando la skill sdd-generar-tasks, genera tasks.md para
ControlCalidad/prompting/reqNNN_slug siguiendo
ControlCalidad/prompting/README.md.
```

**Gate T:** revisar que cada tarea:

- [ ] Empiece con `[ ]`.
- [ ] Indique el archivo o la ruta cuando aplique.
- [ ] Describa un resultado verificable (no "analizar" o "ver" solo).
- [ ] Incluya tests de Dominio, migracion (si aplica), DI, autorizacion y vistas.
- [ ] Incluya la verificacion final y la actualizacion de READMEs.

### Paso 4: implementar

Ejecutar `tasks.md` de arriba a abajo. Al terminar cada tarea, marcarla `[x]`.
No se marcan tareas por intencion: solo cuando estan implementadas y verificadas.

**Gate C:** build y tests en verde.

```bash
docker compose run --rm tools dotnet build SolarUY.sln
docker compose run --rm tools dotnet test SolarUY.sln
```

### Paso 5: cerrar

- [ ] Ejecutar `./ControlCalidad/SonarQube/analizar.sh` y registrar hallazgos.
- [ ] Actualizar la tabla de `ControlCalidad/README.md` con el estado final.
- [ ] Actualizar `README.md` si cambio el estado de una funcionalidad.
- [ ] Confirmar que todas las tareas de `tasks.md` esten en `[x]`.

**Gate D:** el requerimiento pasa a `Implementado`.

## 3. Estados

| Estado | Significado |
| --- | --- |
| **Planificado** | Existen `requirements.md` y `specs.md`; el plan puede estar en revision. |
| **En progreso** | La implementacion comenzo. |
| **Implementado** | Cierre completo: build, tests, ArchUnitNET, SonarQube y READMEs. |

## 4. Definicion de terminado (DoD)

Un requerimiento esta terminado cuando:

1. `requirements.md`, `specs.md` y `tasks.md` estan aprobados y completos.
2. Todas las tareas de `tasks.md` estan en `[x]`.
3. `dotnet build SolarUY.sln` no tiene errores.
4. `dotnet test SolarUY.sln` pasa completo (dominio + ArchUnitNET).
5. SonarQube fue ejecutado y los hallazgos relevantes estan resueltos o justificados.
6. `ControlCalidad/README.md` y `README.md` reflejan el estado real.
7. No se modifico `CodeQuality.Tests` sin autorizacion explicita.

## 5. Reglas de calidad

- Tests xUnit obligatorios en `SolarUY.Tests/Domain` para cada comportamiento
  nuevo o modificado.
- No se crean tests unitarios en Application, Infrastructure ni Presentation.
- La Presentacion solo depende de Application.
- El Dominio no depende de EF Core ni de ASP.NET Core.
- Si un guardarrail de ArchUnitNET falla, se corrige el codigo; nunca el test.

## 6. Comandos utiles

```bash
# Compilar
docker compose run --rm tools dotnet build SolarUY.sln

# Tests + ArchUnitNET
docker compose run --rm tools dotnet test SolarUY.sln

# Migracion
docker compose run --rm tools dotnet ef migrations add NombreDeLaMigracion \
    --project SolarUY --startup-project SolarUY

# Analisis estatico
./ControlCalidad/SonarQube/analizar.sh
```
