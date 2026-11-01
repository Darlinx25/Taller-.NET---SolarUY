# Tareas: req005 Registro y seguimiento de tareas de mantenimiento

> Cada tarea debe ser verificable, indicar el archivo o la ruta cuando aplique y comenzar con `[ ]`. Marcar `[x]` solo cuando esté terminada y verificada.

## 0. Preparación
- [x] 0.1 Confirmar que `requirements.md`, `specs.md` y este plan están aprobados.
- [x] 0.2 Revisar patrones de `req003_nodos_inversores` y `req004_gestion_paneles` (entidades, repos, servicios, controladores, vistas, DI, migraciones).

## 1. Dominio (`SolarUY/Domain`)
- [x] 1.1 Crear `SolarUY/Domain/Entities/TareaMantenimiento.cs` con constantes, constructor privado, constructor público con validaciones, `ActualizarDatos`, `CambiarEstado`, campos obligatorios/opcionales según `specs.md`, `Id`/`PanelId`/`FechaCreacion` inmutables.
- [x] 1.2 Crear `SolarUY/Domain/Exceptions/TareaMantenimientoNoEncontradaException.cs` con propiedad `Id`.
- [x] 1.3 Crear `SolarUY/Domain/Exceptions/TareaMantenimientoEstadoInvalidoException.cs`.
- [x] 1.4 Crear `SolarUY/Domain/Exceptions/TareaMantenimientoNoEditableException.cs`.
- [x] 1.5 Crear `SolarUY/Domain/Repositories/ITareaMantenimientoRepository.cs` con `AgregarAsync`, `ObtenerPorIdAsync`, `ObtenerPorPanelAsync`, `Eliminar`, `SaveChangesAsync`.
- [x] 1.6 Validar `Observaciones` en el constructor y en `ActualizarDatos` usando `ValidarObservaciones`.
- [x] 1.7 Implementar matriz de transiciones permitidas y rechazar `Pendiente -> Completada`.
- [x] 1.8 Validar `QuienCompleta` antes de mutar `Estado` en `CambiarEstado`.

## 2. Tests del Dominio (`SolarUY.Tests/Domain`)
- [x] 2.1 Crear `SolarUY.Tests/Domain/TareaMantenimientoTests.cs` con test de creación válida con todos los campos obligatorios.
- [x] 2.2 Agregar tests de trims/normalización, validación de `Estado` inválido y campos obligatorios (`TipoTarea`, `Descripcion`, `AsignadoA`, fechas, `QuienAsigna`).
- [x] 2.3 Agregar test: al pasar a `Completada` debe registrarse `QuienCompleta` (y exigirlo).
- [x] 2.4 Agregar test: no permitir editar si `Estado == Completada` o `Cancelada` (debe lanzar excepción).
- [x] 2.5 Agregar tests de inmutabilidad de `Id`, `PanelId`, `FechaCreacion`.
- [x] 2.6 Ejecutar tests del dominio (`dotnet test SolarUY.sln` filtrando o completo) para validar que pasan.
- [x] 2.7 Test: `Pendiente -> Completada` lanza `TareaMantenimientoEstadoInvalidoException`.
- [x] 2.8 Test: `Pendiente -> Cancelada` registra `FechaFinalizacion`.
- [x] 2.9 Test: completar sin `QuienCompleta` lanza excepción y el estado no cambia.
- [x] 2.10 Test: observaciones mayores a 1000 caracteres lanzan excepción.
- [x] 2.11 Ejecutar `dotnet test SolarUY.sln` y verificar que todos los tests pasan.

## 3. Aplicación (`SolarUY/Application`)
- [x] 3.1 Crear `SolarUY/Application/Dtos/TareaMantenimientoDto.cs`, `TareaMantenimientoDetalleDto.cs`, `CrearTareaMantenimientoDto.cs`, `EditarTareaMantenimientoDto.cs`, `CambiarEstadoTareaMantenimientoDto.cs` con DataAnnotations según reglas.
- [x] 3.2 Crear `SolarUY/Application/Services/ITareaMantenimientoService.cs` con los 6 casos de uso.
- [x] 3.3 Implementar `SolarUY/Application/Services/TareaMantenimientoService.cs`: validar Panel existente, validar estado, bloquear edición si Completada/Cancelada, registrar `QuienCompleta` al completar, mapear con contexto jerárquico, eliminar físico.
- [x] 3.4 Eliminar `IHttpContextAccessor` y `ObtenerUsuarioActual` de `TareaMantenimientoService`.
- [x] 3.5 Agregar parámetro `quienAsigna` a `CrearTareaAsync` y actualizar la interfaz.
- [x] 3.6 Actualizar `specs.md` para reflejar que `QuienAsigna` lo aporta Presentación.

## 4. Infraestructura (`SolarUY/Infrastructure`)
- [x] 4.1 Agregar `DbSet<TareaMantenimiento> TareasMantenimiento` a `SolarUY/Infrastructure/Persistence/SolarUYDbContext.cs`.
- [x] 4.2 Crear `SolarUY/Infrastructure/Persistence/Configurations/TareaMantenimientoConfiguration.cs` con tabla, PK, FK a Paneles, longitudes, requeridos e índice por `PanelId`.
- [x] 4.3 Crear `SolarUY/Infrastructure/Repositories/TareaMantenimientoRepository.cs` implementando `ITareaMantenimientoRepository` siguiendo patrón de `PanelRepository`.
- [x] 4.4 Registrar `ITareaMantenimientoRepository` y `ITareaMantenimientoService` en `SolarUY/Program.cs` con `AddScoped`.
- [x] 4.5 Generar migración `AgregarTareasMantenimiento` con `dotnet ef migrations add AgregarTareasMantenimiento --project SolarUY --startup-project SolarUY`.
- [x] 4.6 Aplicar/verificar migración según flujo del proyecto (documentar comando usado: `docker compose run --rm tools dotnet ef database update --project SolarUY --startup-project SolarUY`).

## 5. Presentación (`SolarUY/Controllers` + `Views`)
- [x] 5.1 Crear `SolarUY/Controllers/TareasMantenimientoController.cs` con `[Authorize(Roles = "Administrador")]`, acciones Index/Create/Details/Edit/CambiarEstado/Delete/DeleteConfirmed, `TempData` y validaciones.
- [x] 5.2 Crear `SolarUY/Views/TareasMantenimiento/Index.cshtml` mostrando contexto Parque Solar -> Nodo -> Panel y listado con TipoTarea/Estado/AsignadoA/Fechas.
- [x] 5.3 Crear `SolarUY/Views/TareasMantenimiento/Create.cshtml` con formulario, campos obligatorios, `PanelId` oculto, mostrar contexto.
- [x] 5.4 Crear `SolarUY/Views/TareasMantenimiento/Details.cshtml` con todas las fechas, QuienAsigna/QuienCompleta, Observaciones y contexto jerárquico.
- [x] 5.5 Crear `SolarUY/Views/TareasMantenimiento/Edit.cshtml` con bloqueo visual si estado final, `Id` oculto.
- [x] 5.6 Crear `SolarUY/Views/TareasMantenimiento/CambiarEstado.cshtml` con campo `QuienCompleta` visible cuando corresponda.
- [x] 5.7 Crear `SolarUY/Views/TareasMantenimiento/Delete.cshtml` con confirmación explícita de eliminación física.
- [x] 5.8 Modificar `SolarUY/Views/Paneles/Details.cshtml` agregando enlace "Administrar tareas de mantenimiento" hacia `TareasMantenimiento/Index?panelId=...`.
- [x] 5.9 Verificar navegación Parque Solar -> Nodo -> Panel -> TareasMantenimiento.
- [x] 5.10 En `Create` y `CambiarEstado`, obtener el usuario desde `User.Identity?.Name`.
- [x] 5.11 Reemplazar `catch { }` vacíos por manejo de excepciones esperadas.

## 6. Documentación
- [x] 6.1 Actualizar `ControlCalidad/README.md` pasando `req005` a estado correspondiente (Planificado/En progreso según avance) manteniendo tabla existente.
- [x] 6.2 Actualizar `README.md` si corresponde reflejar funcionalidad de tareas de mantenimiento.
- [x] 6.3 Confirmar que `requirements.md`, `specs.md`, `tasks.md` quedan completos.
- [x] 6.4 Verificar que `requirements.md`, `specs.md` y `tasks.md` describan exactamente el comportamiento implementado.

## 7. Verificación
- [x] 7.1 Build: `docker compose run --rm tools dotnet build SolarUY.sln` sin errores.
- [x] 7.2 Tests: `docker compose run --rm tools dotnet test SolarUY.sln` completo en verde (dominio + ArchUnitNET).
- [x] 7.3 Ejecutar `./ControlCalidad/SonarQube/analizar.sh` y registrar hallazgos.
- [x] 7.4 Prueba manual: crear tarea, asignar, pasar a EnProceso, pasar a Completada registrando QuienCompleta, intentar editar tarea Completada (bloqueado), eliminar con confirmación.

## Notas
- Seguir convenciones de nomenclatura y estructura del proyecto.
- No crear tests unitarios en Application/Infrastructure/Presentation.
- No modificar `ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests`.
- Respetar reglas de Clean Architecture y SRP.

## Verificación manual y correcciones posteriores (2026-11-01)

Se recorrió la aplicación real (Docker, sesión de desarrollo y formularios con token antiforgery):

- Navegación verificada por los enlaces de cada pantalla: Parques Solares → Detalle del parque → Nodos → Detalle del nodo → Paneles → Detalle del panel → Tareas de mantenimiento.
- Flujo de 7.4 verificado: alta de tarea (queda `Pendiente`), `Pendiente` → `EnProceso`, `EnProceso` → `Completada` registrando `QuienCompleta` (si el campo se deja vacío se registra al usuario de la sesión), edición de una tarea `Completada` bloqueada con mensaje de error, y eliminación con pantalla de confirmación. Los datos de prueba se eliminaron.
- Al verificar se encontraron dos incumplimientos de `requirements.md` y se corrigieron en las vistas de `Views/TareasMantenimiento/`:
  1. **Contexto jerárquico (criterios 2 y 4, tareas 5.2 y 5.4):** ninguna vista mostraba Parque Solar → Nodo → Panel (solo el código del panel). Ahora Index, Create, Details, Edit, CambiarEstado y Delete lo muestran con los datos que ya traían los DTOs.
  2. **Resultado de cada operación (criterio 9):** el controlador guardaba `TempData["Mensaje"]`/`TempData["Error"]` pero ninguna vista de tareas los mostraba. Ahora Index, Details y Delete (destinos de las redirecciones) los muestran, igual que Paneles, Nodos y Parques.
- Pendiente menor: cuando el panel o la tarea no existen, el controlador redirige a `Home/Index`, que no muestra mensajes de `TempData`.
