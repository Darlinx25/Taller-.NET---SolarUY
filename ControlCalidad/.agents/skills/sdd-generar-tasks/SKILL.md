---
name: sdd-generar-tasks
description: Genera tasks.md verificable a partir de requirements.md y specs.md de un requerimiento del Taller .NET. Usar cuando el usuario pida planificar tareas, crear tasks.md o pasar de specs a implementacion.
---

# Instrucciones

**Rol:** planificador tecnico. En este paso no se implementa codigo.

Cuando el usuario invoque esta skill, debes ejecutar el siguiente flujo de
trabajo de manera estricta:

1. **Localizar el requerimiento:** ubica la carpeta
   `ControlCalidad/prompting/reqNNN_*` indicada por el usuario.
2. **Leer los artefactos:** lee `requirements.md`, `specs.md`, las rules de
   `ControlCalidad/.agents/rules/` y la plantilla
   `ControlCalidad/prompting/_plantillas/tasks.md`.
3. **Revisar el codigo existente:** mira los requerimientos ya implementados
   (por ejemplo req002 y req003) para respetar nombres, rutas y patrones.
4. **Detectar ambiguedades:** si falta informacion o hay una decision abierta,
   PREGUNTA al usuario antes de generar el archivo. No inventes decisiones.
5. **Generar `tasks.md`:** usa las secciones de la plantilla. Cada tarea debe:
   - empezar con `[ ]`,
   - indicar el archivo o la ruta cuando aplique,
   - describir un resultado verificable.
6. **Incluir siempre:**
   - tests xUnit del Dominio (obligatorios) para el comportamiento nuevo,
   - migracion y actualizacion del esquema solo si cambia la base de datos,
   - `DbSet`, configuraciones Fluent API y repositorios si aplica,
   - registro de repositorios y servicios en `Program.cs`,
   - autorizacion `[Authorize(Roles = "Administrador")]` cuando corresponda,
   - actualizacion de `ControlCalidad/README.md` y `README.md`,
   - build, tests, ArchUnitNET y SonarQube en la verificacion final.
7. **No implementar:** no escribas codigo de la aplicacion, no modifiques el
   codigo fuente y no marques tareas como completadas.
8. **Guardar y avisar:** escribe `tasks.md` en la carpeta del requerimiento y
   avisa al usuario que el plan esta listo para su revision y aprobacion.
