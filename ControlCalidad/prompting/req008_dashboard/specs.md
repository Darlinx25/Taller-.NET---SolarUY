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
- La jerarquía se construye desde la topología registrada: parques, nodos y paneles.
- La última lectura de telemetría se cruza por `ParqueSolarId`, `NodoId` y `PanelId`.
- Si falta telemetría para un parque/nodo/panel, se muestran valores `0` y estado `Sin telemetría`.
- `TieneDatos` indica si existe una lectura de telemetría, pero la topología se renderiza igual.
- La actualización en vivo se resuelve con SignalR + un endpoint MVC que devuelve el partial actualizado. No se hace `window.location.reload()`.
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
| `DashboardService` | Implementación | `Application/Services/` | Orquesta la topología registrada y la combina con `ITelemetriaService.ObtenerUltimaLecturaAsync(...)`. Lógica puramente de Application. Sin acceso directo a `DbContext`. |

`DashboardService` orquesta:
- Obtiene todos los parques con `IParqueSolarRepository.ObtenerTodosAsync`.
- Obtiene los nodos de cada parque con `INodoRepository.ObtenerPorParqueAsync`.
- Obtiene los paneles de cada nodo con `IPanelRepository.ObtenerPorNodoAsync`.
- Obtiene la última lectura con `ITelemetriaService.ObtenerUltimaLecturaAsync`.
- Cruza telemetría por IDs y arma la jerarquía completa.
- Usa `Marca`, `Modelo` y `PotenciaWp` desde la entidad `Panel`.
- Usa generación y estado desde la telemetría cuando existen; si no, generación `0` y estado `Sin telemetría`.
- Calcula la generación total del parque con la telemetría del parque o, si no existe, con la suma de sus nodos.
- No accede a `DbContext`, SignalR ni Infrastructure.

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
| `DashboardController` | `Controllers/` | Controlador MVC. Acciones `Index()` y `Contenido()` (GET). Ambas son "vistas tontas": inyectan `IDashboardService`, obtienen `DashboardTelemetriaViewDto` y lo pasan a la vista o partial. Sin lógica de negocio/cálculos complejos. |

### Hubs (SignalR)
| Elemento | Ubicación | Descripción |
|---|---|---|
| `TelemetriaHub` | `Hubs/` | Hereda `Hub`. Ubicado en capa Presentation. Respetar convenciones de nomenclatura. **Sin restricción por rol**. |

**Mecanismo de broadcast:** Cuando `ITelemetriaService.RegistrarLecturaAsync` recibe y guarda una lectura válida, se notifica a los clientes conectados al `TelemetriaHub` mediante el evento `ActualizarTelemetria`. La notificación se hace respetando las fronteras de arquitectura (inyectando `IHubContext<TelemetriaHub>` únicamente donde corresponda). El evento indica que hay una nueva lectura; la vista solicita el estado consolidado al endpoint de partial.

### Autorización
- **Sin restricción por rol.** No agregar `[Authorize(Roles = "Administrador")]` al controlador ni al Hub.

### Vistas
| Elemento | Ubicación | Descripción |
|---|---|---|
| `Index.cshtml` | `Views/Dashboard/` | Vista MVC principal. Contiene el layout, el aviso de sin telemetría y el script SignalR. |
| `_DashboardContenido.cshtml` | `Views/Dashboard/` | Partial con la jerarquía Parque -> Nodo -> Panel, generación por nivel y estado de salud textual + indicador visual. Se reutiliza en la carga inicial y en la actualización en vivo. |

Al recibir `ActualizarTelemetria`, el cliente hace `fetch('/Dashboard/Contenido')` y reemplaza el HTML de `#dashboardLive`. No se recarga la página completa.

## 5) Pruebas

- No se agregan tests de Dominio porque este requerimiento no modifica entidades ni reglas de Dominio.
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
