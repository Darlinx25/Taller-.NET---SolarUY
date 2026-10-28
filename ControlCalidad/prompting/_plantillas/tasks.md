# Tareas: reqNNN <Nombre del requerimiento>

> Cada tarea debe ser verificable, indicar el archivo o la ruta cuando aplique
> y comenzar con `[ ]`. Marcar `[x]` solo cuando este terminada y verificada.

## 0. Preparacion

- [ ] 0.1 Confirmar que `requirements.md`, `specs.md` y este plan estan aprobados.
- [ ] 0.2 Revisar el codigo existente para respetar nombres y patrones.

## 1. Dominio (`SolarUY.Domain`)

- [ ] 1.1 ...

## 2. Tests del Dominio (`SolarUY.Tests/Domain`)

- [ ] 2.1 ...

## 3. Aplicacion (`SolarUY.Application`)

- [ ] 3.1 ...

## 4. Infraestructura (`SolarUY.Infrastructure`)

- [ ] 4.1 ...
- [ ] 4.x Generar y aplicar la migracion correspondiente (si cambia el esquema).
- [ ] 4.x Registrar repositorios y servicios en `Program.cs`.

## 5. Presentacion (`SolarUY.Controllers` + `Views`)

- [ ] 5.1 ...

## 6. Documentacion

- [ ] 6.1 Actualizar `ControlCalidad/README.md` con el estado del requerimiento.
- [ ] 6.2 Actualizar `README.md` si cambia el estado de una funcionalidad.

## 7. Verificacion

- [ ] 7.1 `docker compose run --rm tools dotnet build SolarUY.sln`
- [ ] 7.2 `docker compose run --rm tools dotnet test SolarUY.sln`
- [ ] 7.3 Ejecutar `./ControlCalidad/SonarQube/analizar.sh` y registrar hallazgos.
- [ ] 7.4 Prueba manual del flujo completo.

## Notas

- ...
