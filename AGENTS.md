# SolarUY - Instrucciones para agentes de IA

## Contexto del proyecto

SolarUY es una aplicacion ASP.NET Core MVC (.NET 10) para la gestion de parques
solares. Es un monolito organizado internamente en capas (`Domain`, `Application`,
`Infrastructure` y Presentacion MVC) dentro de un unico proyecto `SolarUY`.

- Persistencia: Entity Framework Core + MySQL 8.4.
- Calidad: ArchUnitNET (`CodeQuality.Tests`), xUnit (`SolarUY.Tests`) y SonarQube.
- Metodologia: Spec-Driven Development (SDD) con artefactos en
  `ControlCalidad/prompting/`.

## Lectura obligatoria antes de actuar

1. `ControlCalidad/.agents/rules/1_clean_architecture.md`
2. `ControlCalidad/.agents/rules/2_principios_solid.md`
3. `ControlCalidad/.agents/rules/3_convenciones_nomenclatura.md`
4. `ControlCalidad/.agents/rules/4_modelado_dominio.md`
5. `ControlCalidad/.agents/rules/5_capa_presentacion.md`
6. `ControlCalidad/.agents/rules/6_flujo_trabajo_ia.md`
7. `ControlCalidad/prompting/README.md`

## Flujo SDD obligatorio

No se escribe codigo sin los tres artefactos del requerimiento revisados y
aprobados por una persona:

1. `requirements.md`: que se necesita, en lenguaje de negocio. Se crea desde
   `ControlCalidad/prompting/_plantillas/requirements.md`.
2. `specs.md`: como se resuelve, por capas. Se genera con la skill
   `sdd-generar-specs` a partir del requerimiento y la plantilla.
3. `tasks.md`: plan de tareas verificables. Se genera con la skill
   `sdd-generar-tasks` a partir de la spec, la plantilla y el codigo existente.
4. Ejecutar `tasks.md` marcando cada tarea con `[x]` al completarla.
5. Cerrar el requerimiento: build, tests, ArchUnitNET, SonarQube y READMEs
   actualizados.

Cada artefacto requiere aprobacion humana antes de pasar al siguiente. Si algo
no esta especificado o hay una decision abierta, PREGUNTA antes de inventarlo.

## Reglas duras

- No modificar `ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests` sin
  autorizacion explicita.
- Escribir tests xUnit del Dominio para cada comportamiento nuevo o modificado.
- No crear tests unitarios en Application, Infrastructure ni Presentation.
- La Presentacion no accede a Infrastructure: solo a servicios de Application.
- No commitear ni hacer push sin pedido explicito.

## Comandos

```bash
docker compose run --rm tools dotnet build SolarUY.sln
docker compose run --rm tools dotnet test SolarUY.sln
./ControlCalidad/SonarQube/analizar.sh
```
