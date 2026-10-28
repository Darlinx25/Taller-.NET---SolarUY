# Tareas: req005 Registro y seguimiento de tareas de mantenimiento

> Cada tarea debe ser verificable, indicar el archivo o la ruta cuando aplique y comenzar con `[ ]`. Marcar `[x]` solo cuando esté terminada y verificada.

## 0. Preparación
- [ ] 0.1 Confirmar que `requirements.md`, `specs.md` y este plan están aprobados.
- [ ] 0.2 Revisar patrones de `req003_nodos_inversores` y `req004_gestion_paneles` (entidades, repos, servicios, controladores, vistas, DI, migraciones).

## 1. Dominio (`SolarUY/Domain`)
- [ ] 1.1 Crear `SolarUY/Domain/Entities/TareaMantenimiento.cs` con constantes, constructor privado, constructor público con validaciones, `ActualizarDatos`, `CambiarEstado`, campos obligatorios/opcionales según `specs.md`, `Id`/`PanelId`/`FechaCreacion` inmutables.
- [ ] 1.2 Crear `SolarUY/Domain/Exceptions/TareaMantenimientoNoEncontradaException.cs` con propiedad `Id`.
- [ ] 1.3 Crear `SolarUY/Domain/Exceptions/TareaMantenimientoEstadoInvalidoException.cs`.
- [ ] 1.4 Crear `SolarUY/Domain/Exceptions/TareaMantenimientoNoEditableException.cs`.
- [ ] 1.5 Crear `SolarUY/Domain/Repositories/ITareaMantenimientoRepository.cs` con `AgregarAsync`, `ObtenerPorIdAsync`, `ObtenerPorPanelAsync`, `Eliminar`, `SaveChangesAsync`.

## 2. Tests del Dominio (`SolarUY.Tests/Domain`)
- [ ] 2.1 Crear `SolarUY.Tests/Domain/TareaMantenimientoTests.cs` con test de creación válida con todos los campos obligatorios.
- [ ] 2.2 Agregar tests de trims/normalización, validación de `Estado` inválido y campos obligatorios (`TipoTarea`, `Descripcion`, `AsignadoA`, fechas, `QuienAsigna`).
- [ ] 2.3 Agregar test: al pasar a `Completada` debe registrarse `QuienCompleta` (y exigirlo).
- [ ] 2.4 Agregar test: no permitir editar si `Estado == Completada` o `Cancelada` (debe lanzar excepción).
- [ ] 2.5 Agregar tests de inmutabilidad de `Id`, `PanelId`, `FechaCreacion`.
- [ ] 2.6 Ejecutar tests del dominio (`dotnet test SolarUY.sln` filtrando o completo) para validar que pasan.

## 3. Aplicación (`SolarUY/Application`)
- [ ] 3.1 Crear `SolarUY/Application/Dtos/TareaMantenimientoDto.cs`, `TareaMantenimientoDetalleDto.cs`, `CrearTareaMantenimientoDto.cs`, `EditarTareaMantenimientoDto.cs`, `CambiarEstadoTareaMantenimientoDto.cs` con DataAnnotations según reglas.
- [ ] 3.2 Crear `SolarUY/Application/Services/ITareaMantenimientoService.cs` con los 6 casos de uso.
- [ ] 3.3 Implementar `SolarUY/Application/Services/TareaMantenimientoService.cs`: validar Panel existente, validar estado, bloquear edición si Completada/Cancelada, registrar `QuienCompleta` al completar, mapear con contexto jerárquico, eliminar físico.

## 4. Infraestructura (`SolarUY/Infrastructure`)
- [ ] 4.1 Agregar `DbSet<TareaMantenimiento> TareasMantenimiento` a `SolarUY/Infrastructure/Persistence/SolarUYDbContext.cs`.
- [ ] 4.2 Crear `SolarUY/Infrastructure/Persistence/Configurations/TareaMantenimientoConfiguration.cs` con tabla, PK, FK a Paneles, longitudes, requeridos e índice por `PanelId`.
- [ ] 4.3 Crear `SolarUY/Infrastructure/Repositories/TareaMantenimientoRepository.cs` implementando `ITareaMantenimientoRepository` siguiendo patrón de `PanelRepository`.
- [ ] 4.4 Registrar `ITareaMantenimientoRepository` y `ITareaMantenimientoService` en `SolarUY/Program.cs` con `AddScoped`.
- [ ] 4.5 Generar migración `AgregarTareasMantenimiento` con `dotnet ef migrations add AgregarTareasMantenimiento --project SolarUY --startup-project SolarUY`.
- [ ] 4.6 Aplicar/verificar migración según flujo del proyecto (documentar comando usado).

## 5. Presentación (`SolarUY/Controllers` + `Views`)
- [ ] 5.1 Crear `SolarUY/Controllers/TareasMantenimientoController.cs` con `[Authorize(Roles = "Administrador")]`, acciones Index/Create/Details/Edit/CambiarEstado/Delete/DeleteConfirmed, `TempData` y validaciones.
- [ ] 5.2 Crear `SolarUY/Views/TareasMantenimiento/Index.cshtml` mostrando contexto Parque Solar -> Nodo -> Panel y listado con TipoTarea/Estado/AsignadoA/Fechas.
- [ ] 5.3 Crear `SolarUY/Views/TareasMantenimiento/Create.cshtml` con formulario, campos obligatorios, `PanelId` oculto, mostrar contexto.
- [ ] 5.4 Crear `SolarUY/Views/TareasMantenimiento/Details.cshtml` con todas las fechas, QuienAsigna/QuienCompleta, Observaciones y contexto jerárquico.
- [ ] 5.5 Crear `SolarUY/Views/TareasMantenimiento/Edit.cshtml` con bloqueo visual si estado final, `Id` oculto.
- [ ] 5.6 Crear `SolarUY/Views/TareasMantenimiento/CambiarEstado.cshtml` con campo `QuienCompleta` visible cuando corresponda.
- [ ] 5.7 Crear `SolarUY/Views/TareasMantenimiento/Delete.cshtml` con confirmación explícita de eliminación física.
- [ ] 5.8 Modificar `SolarUY/Views/Paneles/Details.cshtml` agregando enlace "Administrar tareas de mantenimiento" hacia `TareasMantenimiento/Index?panelId=...`.
- [ ] 5.9 Verificar navegación Parque Solar -> Nodo -> Panel -> TareasMantenimiento.

## 6. Documentación
- [ ] 6.1 Actualizar `ControlCalidad/README.md` pasando `req005` a estado correspondiente (Planificado/En progreso según avance) manteniendo tabla existente.
- [ ] 6.2 Actualizar `README.md` si corresponde reflejar funcionalidad de tareas de mantenimiento.
- [ ] 6.3 Confirmar que `requirements.md`, `specs.md`, `tasks.md` quedan completos.

## 7. Verificación
- [ ] 7.1 Build: `docker compose run --rm tools dotnet build SolarUY.sln` sin errores.
- [ ] 7.2 Tests: `docker compose run --rm tools dotnet test SolarUY.sln` completo en verde (dominio + ArchUnitNET).
- [ ] 7.3 Ejecutar `./ControlCalidad/SonarQube/analizar.sh` y registrar hallazgos.
- [ ] 7.4 Prueba manual: crear tarea, asignar, pasar a EnProceso, pasar a Completada registrando QuienCompleta, intentar editar tarea Completada (bloqueado), eliminar con confirmación.

## Notas
- Seguir convenciones de nomenclatura y estructura del proyecto.
- No crear tests unitarios en Application/Infrastructure/Presentation.
- No modificar `ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests`.
- Respetar reglas de Clean Architecture y SRP.
