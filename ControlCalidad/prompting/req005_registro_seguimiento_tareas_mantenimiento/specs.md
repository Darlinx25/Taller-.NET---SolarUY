# Especificación Técnica: Registro y seguimiento de tareas de mantenimiento

## Objetivo

Implementar el registro y seguimiento de tareas de mantenimiento de paneles fotovoltaicos (limpieza, revisión, reparación), asociadas a un Panel, con asignación a personal especializado, estados, fechas obligatorias, registro de quién asigna/quién completa, validaciones en Dominio, operaciones CRUD + cambio de estado, autorización exclusiva para Administrador, gestión desde la ficha del Panel mostrando siempre el contexto Parque Solar -> Nodo -> Panel y baja física con confirmación.

## Decisiones y supuestos

- La tarea se registra a nivel de Panel. Una tarea se aplica a un solo Panel (relación 1:N: Panel -> TareasMantenimiento).
- Únicamente rol "Administrador" puede crear/asignar/editar/cancelar/cambiar estado.
- Estados válidos: `Pendiente`, `EnProceso`, `Completada`, `Cancelada`. No hay otros estados.
- Transiciones permitidas: `Pendiente -> EnProceso`, `Pendiente -> Cancelada`, `EnProceso -> Completada`, `EnProceso -> Cancelada`. No se permite `Pendiente -> Completada` ni retroceder.
- No se permite editar una tarea si su estado es `Completada` o `Cancelada`.
- `FechaCreacion` y `FechaAsignacion` se registran al alta. `FechaVencimiento` es obligatoria e ingresada por el Administrador. `FechaInicio` se registra al pasar a `EnProceso`. `FechaFinalizacion` se registra al pasar a `Completada` o `Cancelada`.
- `QuienAsigna` se obtiene del usuario autenticado en la capa de Presentación y se pasa a Application; Application no depende de `HttpContext`.
- `QuienCompleta` es obligatorio al pasar a `Completada`. Si no se informa, el controlador lo completa con el usuario autenticado.
- `AsignadoA` (personal especializado) es obligatorio.
- `TipoTarea` y `Descripcion` son obligatorias. `Observaciones` es opcional, con trim y máximo de 1000 caracteres, validado en Dominio.
- Baja física (eliminación definitiva) con confirmación.
- Siempre mostrar contexto jerárquico: Parque Solar -> Nodo -> Panel.

## 1) Dominio (`SolarUY/Domain`)

### 1.1 Entidad `TareaMantenimiento`

Ubicación: `SolarUY/Domain/Entities/TareaMantenimiento.cs`

| Campo | Tipo | Obligatorio | Regla |
|---|---|---|---|
| `Id` | `Guid` | Sí | Clave primaria, generada en el alta |
| `PanelId` | `Guid` | Sí | FK a `Panel`, no vacía, inmutable |
| `Panel` | `Panel?` | Navegación | Relación con el panel padre |
| `TipoTarea` | `string` | Sí | Obligatorio, trim, longitud máxima razonable (validada en Dominio) |
| `Descripcion` | `string` | Sí | Obligatorio, trim, longitud máxima razonable (validada en Dominio) |
| `AsignadoA` | `string` | Sí | Obligatorio, trim, representa personal especializado |
| `FechaCreacion` | `DateTime` | Sí | UTC, inmutable, registrada automáticamente al alta. |
| `FechaAsignacion` | `DateTime` | Sí | Registrada automáticamente al alta. |
| `FechaVencimiento` | `DateTime` | Sí | Ingresada por el Administrador. |
| `FechaInicio` | `DateTime` | No | Se registra automáticamente al pasar a `EnProceso`. `default` significa "sin fecha registrada". |
| `FechaFinalizacion` | `DateTime` | No | Se registra automáticamente al pasar a `Completada` o `Cancelada`. `default` significa "sin fecha registrada". |
| `Estado` | `string` | Sí | Obligatorio, debe ser uno de: `Pendiente`, `EnProceso`, `Completada`, `Cancelada` |
| `QuienAsigna` | `string` | Sí | Usuario autenticado que crea la tarea. |
| `QuienCompleta` | `string?` | Condicional | Obligatorio al pasar a `Completada`; se toma del usuario si no se informa. |
| `Observaciones` | `string?` | No | Opcional, trim, vacío se normaliza a `null`, máximo 1000 caracteres validado en Dominio. |

### 1.2 Comportamiento

- Constructor privado para Entity Framework Core.
- Constructor público que valida todos los campos obligatorios, normaliza trims, valida Estado contra los valores permitidos, valida fechas obligatorias, valida observaciones y crea `Id`, `FechaCreacion` y `FechaAsignacion` (UTC).
- `ActualizarDatos(...)` para editar campos permitidos. Debe lanzar excepción si el estado actual es `Completada` o `Cancelada`, y debe validar observaciones.
- `CambiarEstado(string nuevoEstado, string? quienCompleta = null)` debe:
  - validar que la transición esté en la matriz permitida;
  - validar `quienCompleta` antes de mutar `Estado` cuando el nuevo estado sea `Completada`;
  - registrar `FechaInicio` al pasar a `EnProceso`;
  - registrar `QuienCompleta` y `FechaFinalizacion` al pasar a `Completada`;
  - registrar `FechaFinalizacion` al pasar a `Cancelada`.
- `Id`, `PanelId` y `FechaCreacion` son inmutables.

### 1.3 Excepciones

Ubicación: `SolarUY/Domain/Exceptions/`

- `TareaMantenimientoNoEncontradaException(Guid id)` con propiedad `Id`.
- `TareaMantenimientoEstadoInvalidoException(string estado)` o similar siguiendo patrón del proyecto.
- `TareaMantenimientoNoEditableException(Guid id)` cuando se intenta editar tarea en estado `Completada` o `Cancelada`.

### 1.4 Repositorios (contratos)

Ubicación: `SolarUY/Domain/Repositories/ITareaMantenimientoRepository.cs`

Métodos:
- `Task AgregarAsync(TareaMantenimiento tarea, CancellationToken cancellationToken = default)`.
- `Task<TareaMantenimiento?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)`.
- `Task<IReadOnlyList<TareaMantenimiento>> ObtenerPorPanelAsync(Guid panelId, CancellationToken cancellationToken = default)`.
- `void Eliminar(TareaMantenimiento tarea)`.
- `Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)`.

## 2) Aplicación (`SolarUY/Application`)

### 2.1 DTOs

Ubicación: `SolarUY/Application/Dtos/`

- `TareaMantenimientoDto`: `Id`, `PanelId`, `TipoTarea`, `Descripcion`, `AsignadoA`, `FechaCreacion`, `FechaAsignacion`, `FechaVencimiento`, `FechaInicio`, `FechaFinalizacion`, `Estado`, `QuienAsigna`, `QuienCompleta`, `Observaciones`.
- `TareaMantenimientoDetalleDto`: agrega `PanelCodigoSerie`, `NodoId`, `NodoCodigo`, `ParqueSolarId`, `ParqueSolarNombre` para mostrar siempre el contexto jerárquico.
- `CrearTareaMantenimientoDto`: `PanelId`, `TipoTarea`, `Descripcion`, `AsignadoA`, `FechaVencimiento`, `Observaciones`. No incluye `QuienAsigna`: lo aporta el controlador desde el usuario autenticado.
- `EditarTareaMantenimientoDto`: incluye `Id`; permite editar solo si no está Completada/Cancelada (validado en servicio). No permite cambiar PanelId.
- `CambiarEstadoTareaMantenimientoDto`: `Id`, `NuevoEstado`, `QuienCompleta`. El controlador completa `QuienCompleta` con el usuario autenticado si el estado es `Completada` y el campo viene vacío.

### 2.2 Servicios y casos de uso

Ubicación: `SolarUY/Application/Services/ITareaMantenimientoService.cs` y `TareaMantenimientoService.cs`

Casos de uso:
- `Task<IReadOnlyList<TareaMantenimientoDto>> ObtenerTareasPorPanelAsync(Guid panelId, CancellationToken cancellationToken = default)`.
- `Task<TareaMantenimientoDetalleDto> ObtenerTareaAsync(Guid tareaId, CancellationToken cancellationToken = default)`.
- `Task<TareaMantenimientoDto> CrearTareaAsync(CrearTareaMantenimientoDto dto, string quienAsigna, CancellationToken cancellationToken = default)`.
- `Task<TareaMantenimientoDto> EditarTareaAsync(EditarTareaMantenimientoDto dto, CancellationToken cancellationToken = default)`.
- `Task CambiarEstadoAsync(CambiarEstadoTareaMantenimientoDto dto, CancellationToken cancellationToken = default)`.
- `Task EliminarTareaAsync(Guid tareaId, CancellationToken cancellationToken = default)`.

Reglas en servicio:
- Validar existencia de Panel al crear.
- No usar `IHttpContextAccessor` ni dependencias de ASP.NET Core en Application.
- No permitir editar si tarea está en estado `Completada` o `Cancelada` (lanzar excepción de Dominio/servicio).
- Al cambiar a `Completada` exigir `QuienCompleta` a nivel de Dominio.
- Mapear entidades a DTOs incluyendo contexto jerárquico cuando corresponda.

## 3) Infraestructura (`SolarUY/Infrastructure`)

### 3.1 DbSet y configuraciones

- Agregar `DbSet<TareaMantenimiento> TareasMantenimiento` a `SolarUYDbContext`.
- Crear `TareaMantenimientoConfiguration` en `SolarUY/Infrastructure/Persistence/Configurations/`:
  - Tabla `TareasMantenimiento`
  - Clave primaria `Id` sin autogeneración
  - FK a `Paneles` con `OnDelete(DeleteBehavior.Cascade)` o `Restrict` según patrón existente; mantener integridad con jerarquía
  - Propiedades requeridas, longitudes razonables para strings
  - Índice opcional por `PanelId` para listados

### 3.2 Repositorios

Crear `TareaMantenimientoRepository` en `SolarUY/Infrastructure/Repositories/` implementando `ITareaMantenimientoRepository` siguiendo el patrón de `PanelRepository`/`NodoRepository`.

### 3.3 Migración

Migración: **Sí**. Nombre de migración: `AgregarTareasMantenimiento`.

### 3.4 Registro de dependencias (`Program.cs`)

Registrar en `Program.cs`:
- `services.AddScoped<ITareaMantenimientoRepository, TareaMantenimientoRepository>()`
- `services.AddScoped<ITareaMantenimientoService, TareaMantenimientoService>()`

## 4) Presentación (`SolarUY/Controllers` + `Views`)

### 4.1 Controlador y acciones

Ubicación: `SolarUY/Controllers/TareasMantenimientoController.cs`

Decorado con `[Authorize(Roles = "Administrador")]`.

Acciones:
- `Index(Guid panelId)`: listado de tareas por Panel, mostrando siempre contexto Parque Solar -> Nodo -> Panel.
- `Create(Guid panelId)` GET/POST: alta de tarea asociada al Panel.
- `Details(Guid id)`: detalle con todas las fechas, quién asigna/quién completa, observaciones y contexto jerárquico.
- `Edit(Guid id)` GET/POST: edición. Bloquear si estado es `Completada` o `Cancelada` (redirigir con mensaje de error).
- `CambiarEstado(Guid id)` GET/POST: cambio de estado con formulario para `QuienCompleta` cuando corresponda.
- `Delete(Guid id)` GET y `DeleteConfirmed(Guid id)` POST con anti-forgery: eliminación física con confirmación.
- `Create` GET/POST: obtener `quienAsigna` desde `User.Identity?.Name` y pasarlo al servicio. No confiar en un campo editable del formulario.
- `CambiarEstado` POST: si `NuevoEstado == "Completada"` y `QuienCompleta` está vacío, completarlo con `User.Identity?.Name`.
- No usar `catch { }` vacíos: capturar excepciones esperadas y reportarlas por `TempData["Error"]`.

Manejo de errores con `TempData["Mensaje"]` / `TempData["Error"]` siguiendo patrón de req002/req003/req004.

### 4.2 Vistas

Ubicación: `Views/TareasMantenimiento/`

Crear:
- `Index.cshtml`: listado con TipoTarea, Estado, AsignadoA, Fechas, acciones. Mostrar jerarquía.
- `Create.cshtml`: formulario con todos los campos obligatorios, PanelId oculto, mostrar contexto.
- `Details.cshtml`: ficha completa con fechas, QuienAsigna, QuienCompleta, Observaciones, contexto.
- `Edit.cshtml`: formulario; deshabilitar edición si estado final (mostrar aviso). Mantener `Id` oculto.
- `CambiarEstado.cshtml`: formulario para cambiar estado, campo `QuienCompleta` visible solo cuando corresponda.
- `Delete.cshtml`: confirmación explícita de eliminación física.

Modificar: `Views/Paneles/Details.cshtml` para agregar enlace "Administrar tareas de mantenimiento" hacia `TareasMantenimiento/Index` con `panelId`.

### 4.3 Autorización

Todas las acciones requieren `[Authorize(Roles = "Administrador")]`. Solo rol Administrador.

## 5) Pruebas

- Crear `SolarUY.Tests/Domain/TareaMantenimientoTests.cs` con tests xUnit obligatorios:
  - Creación válida con todos los campos obligatorios
  - Trims y normalización de strings
  - Validación de Estado inválido
  - `QuienCompleta` requerido al pasar a Completada
  - No permitir editar si estado es `Completada` o `Cancelada`
  - Inmutabilidad de `Id`, `PanelId`, `FechaCreacion`
  - Validación de campos obligatorios (TipoTarea, Descripcion, AsignadoA, fechas, QuienAsigna)
  - `CambiarEstado` desde `Pendiente` a `Completada` lanza excepción
  - `CambiarEstado` desde `Pendiente` a `Cancelada` registra `FechaFinalizacion`
  - `CambiarEstado` a `Completada` sin `QuienCompleta` lanza excepción y no deja el estado mutado
  - Observaciones mayores a 1000 caracteres lanzan excepción en el constructor y en `ActualizarDatos`
- No crear tests unitarios en Application, Infrastructure ni Presentation.

## 6) Calidad y verificación

- [ ] Build: `docker compose run --rm tools dotnet build SolarUY.sln`
- [ ] Tests: `docker compose run --rm tools dotnet test SolarUY.sln`
- [ ] ArchUnitNET en verde
- [ ] SonarQube ejecutado y hallazgos revisados
- [ ] Prueba manual del flujo completo (alta, asignación, cambio de estado a EnProceso/Completada con QuienCompleta, intento de edición en estado final bloqueado, eliminación con confirmación)

## 7) Fuera de alcance

- Monitoreo en tiempo real, telemetría, alertas, historial agregado de generación y estado de falla del panel.
- Asignación masiva a múltiples paneles.
- Otros estados distintos a `Pendiente`, `EnProceso`, `Completada`, `Cancelada`.
