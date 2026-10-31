# Especificación Técnica: Dashboard de telemetría en vivo

## Objetivo

Implementar un dashboard de monitoreo en tiempo real para visualizar la generación de energía y el estado de salud de Parques Solares, Nodos y Paneles, consumiendo la telemetría ya existente vía **SignalR**, con carga inicial por MVC y actualizaciones en vivo. Acceso por ruta, sin restricción por rol, siguiendo Clean Architecture y las convenciones del proyecto.

## Decisiones y supuestos

- Carpeta del requerimiento: `ControlCalidad/prompting/req008_dashboard/`
- Acceso: **por ruta**. Acción propuesta: `GET /Dashboard/Index`.
- Presentación: **MVC** (`Views/Dashboard/Index.cshtml`) + **SignalR** para actualizaciones en tiempo real.
- Hub: `TelemetriaHub` en `SolarUY/Hubs/` respetando convenciones (clase que hereda de `Microsoft.AspNetCore.SignalR.Hub`).
- Reutilizar el contrato existente: `SolarUY.Application.Dtos.TelemetriaDto`, `SolarUY.Application.Services.ITelemetriaService`, `SolarUY.Application.Services.IAlmacenTelemetria`.
- **Sin restricción por rol**. No se aplica `[Authorize(Roles = "Administrador")]`. Debe ser accesible sin restricción por rol.
- **Ambas formas**: carga inicial desde controlador vía servicio de Application (MVC "vista tonta") + suscripción al Hub `TelemetriaHub` para recibir actualizaciones en tiempo real cuando llegue una nueva lectura.
- Dominio **no** debe tener referencias a SignalR ni a ASP.NET Core.
- Infraestructura **no** debe ser accedida directamente desde Controllers/Views. Solo vía Application.
- Migración: **NO**. No se agregan entidades, ni DbSets, ni cambios de esquema. Se reutiliza telemetría + topología existente (`ParqueSolar`, `Nodo`, `Panel`).
- Respetar decisiones de arquitectura tomadas en `Iteracion 1 Grupo 2 .NET.pdf`.

## 1) Dominio (`SolarUY.Domain`)

### Entidades
- Sin cambios. Se reutilizan `ParqueSolar`, `Nodo`, `Panel`, `Inversor` tal como están modeladas.

### Validaciones
- Sin cambios. Las validaciones siguen correspondiendo al modelo existente.

### Excepciones
- Sin nuevas excepciones de dominio requeridas para este requerimiento.

### Repositorios (contratos)
- Sin nuevos contratos obligatorios. Si se necesitara consultar topología para armar la vista, se deberá usar los repositorios/servicios ya existentes. No crear contratos nuevos sin necesidad estrictamente comprobada contra el código actual.

## 2) Aplicación (`SolarUY.Application`)

### DTOs
Crear DTOs de vista para el dashboard (en `SolarUY.Application.Dtos/`):

| DTO | Ubicación | Descripción |
|---|---|---|
| `DashboardTelemetriaViewDto` | `Application/Dtos/` | Agregado para la vista. Debe incluir: `FechaHora`, `Parques` (lista), indicador `TieneDatos`, `MensajeSinDatos`. |
| `DashboardParqueDto` | `Application/Dtos/` | Parque con `Id`, `Nombre`, `Ubicacion`, `GeneracionTotal`, `Nodos`. |
| `DashboardNodoDto` | `Application/Dtos/` | Nodo con `Id`, `Codigo`, `Generacion`, `Paneles`. |
| `DashboardPanelDto` | `Application/Dtos/` | Panel con `Id`, `CodigoSerie`, `Marca`, `Modelo`, `PotenciaWp`, `Generacion`, `EstadoSalud` (texto), `EsFallo` (bool). Debe mostrar estado de salud textual + indicador visual. |

### Servicios y casos de uso

| Elemento | Tipo | Ubicación | Descripción |
|---|---|---|---|
| `IDashboardService` | Interfaz | `Application/Services/` | Caso de uso para obtener el estado inicial del dashboard (topología + última lectura). |
| `DashboardService` | Implementación | `Application/Services/` | Orquesta: obtiene topología registrada (vía servicios/repositorios existentes) y combina con `ITelemetriaService.ObtenerUltimaLecturaAsync(...)`. Calcula agregados por parque/nodo si aplica. Lógica puramente de Application. Sin acceso directo a `DbContext`. |

**Reglas:** Inyección por constructor. Solo depende de `Domain`/contratos de Application. No referencia `Infrastructure` ni `Hubs`/SignalR.

## 3) Infraestructura (`SolarUY.Infrastructure`)

### DbSet y configuraciones
- Sin cambios.

### Repositorios
- Sin cambios obligatorios. Se reutilizan los existentes.

### Migración
**NO**. No hay cambios de esquema. Justificar: se reutiliza `TelemetriaDto` y entidades/topología ya existentes.

### Registro de dependencias (`Program.cs`)
Agregar registros en `Program.cs`:

| Registro | Tipo | Notas |
|---|---|---|
| `AddSignalR()` | Servicio | Habilitar SignalR. |
| `IDashboardService` -> `DashboardService` | Scoped | Registrar con `AddScoped` siguiendo el patrón existente. |
| `MapHub<TelemetriaHub>("/hubs/telemetria")` | Endpoint | Mapear Hub en el pipeline. |

No modificar registros existentes de telemetría.

## 4) Presentación (`SolarUY.Controllers` + `Views` + `Hubs`)

### Controlador y acciones
| Elemento | Ubicación | Descripción |
|---|---|---|
| `DashboardController` | `Controllers/` | Controlador MVC. Acción `Index()` (GET). Debe ser "vista tonta": inyecta `IDashboardService`, obtiene `DashboardTelemetriaViewDto`, pasa a ViewModel/Vista. Sin lógica de negocio/cálculos complejos. |

### Hubs (SignalR)
| Elemento | Ubicación | Descripción |
|---|---|---|
| `TelemetriaHub` | `Hubs/` | Hereda `Hub`. Ubicado en capa Presentation. Respetar convenciones de nomenclatura. **Sin restricción por rol**. |

**Mecanismo de broadcast:** Cuando `ITelemetriaService.RegistrarLecturaAsync` recibe y guarda una lectura válida, se debe notificar a los clientes conectados al `TelemetriaHub` con la información necesaria para actualizar el dashboard. La notificación debe hacerse respetando las fronteras de arquitectura (inyectando `IHubContext<TelemetriaHub>` únicamente donde corresponda).

### Autorización
- **Sin restricción por rol.** No agregar `[Authorize(Roles = "Administrador")]` al controlador ni al Hub.

### Vistas
| Elemento | Ubicación | Descripción |
|---|---|---|
| `Index.cshtml` | `Views/Dashboard/` | Vista MVC principal. Debe mostrar jerarquía Parque -> Nodo -> Panel. Mostrar generación por parque/nodo/panel. Mostrar **estado de salud textual + indicador visual** por panel. Incluir estado "Sin telemetría disponible" cuando no hay datos. Conectar cliente SignalR al hub (`/hubs/telemetria`), suscribirse a evento de actualización y refrescar la vista sin recarga. |

## 5) Pruebas

- Tests xUnit del Dominio (obligatorios): solo si se agrega comportamiento nuevo en `Domain`. Si no hay cambios en Domain, no agregar tests.
- No se agregan tests en Application, Infrastructure ni Presentation.

## 6) Calidad y verificación

- [ ] Build: `docker compose run --rm tools dotnet build SolarUY.sln`
- [ ] Tests: `docker compose run --rm tools dotnet test SolarUY.sln`
- [ ] ArchUnitNET en verde
- [ ] SonarQube ejecutado y hallazgos revisados
- [ ] Prueba manual del flujo completo

## 7) Fuera de alcance

- Modificar `/api/simulacion/*`
- Cambiar ingesta/validación de telemetría
- Notificaciones por correo/SMS
- Histórico/gráficas históricas
- Nuevos sensores/inversores
- Restricción por rol
