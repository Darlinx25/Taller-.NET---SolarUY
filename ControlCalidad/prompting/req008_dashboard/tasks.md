# Tareas: req008_dashboard - Dashboard de telemetría en vivo

> Cada tarea debe ser verificable, indicar el archivo o la ruta cuando aplique y comenzar con `[ ]`. Marcar `[x]` solo cuando esté terminada y verificada.

## 0. Preparación
- [x] 0.1 Confirmar que `requirements.md`, `specs.md` y este plan están aprobados.
- [x] 0.2 Revisar `req002_gestion_parques`, `req003_nodos_inversores`, `req004_gestion_paneles` para imitar nombres y patrones.
- [x] 0.3 Revisar `SolarUY/Controllers/TelemetriaController.cs`, `SolarUY/Application/Services/ITelemetriaService.cs`, `SolarUY/Application/Services/TelemetriaService.cs`, `SolarUY/Application/Dtos/TelemetriaDto.cs` y `SolarUY/Program.cs` para reutilizar contratos.
- [x] 0.4 Revisar reglas en `ControlCalidad/.agents/rules/` y decisiones en `D:\Proyectos\Iteracion 1 Grupo 2 .NET.pdf`.

## 1. Dominio (`SolarUY.Domain`)
- [x] 1.1 Verificar entidades/repositorios en `SolarUY/Domain/`. No crear cambios de dominio ni migración.

## 2. Tests del Dominio (`SolarUY.Tests/Domain`)
- [x] 2.1 Revisar si aplica agregar tests xUnit en `SolarUY.Tests/Domain/`. Si no hay comportamiento nuevo en Dominio, no agregar tests.

## 3. Aplicación (`SolarUY.Application`)
- [x] 3.1 Crear DTOs en `SolarUY/Application/Dtos/`: `DashboardTelemetriaViewDto.cs`, `DashboardParqueDto.cs`, `DashboardNodoDto.cs`, `DashboardPanelDto.cs`.
- [x] 3.2 Crear interfaz `SolarUY/Application/Services/IDashboardService.cs`.
- [x] 3.3 Crear implementación `SolarUY/Application/Services/DashboardService.cs` (inyección por constructor). Debe combinar topología existente con `ITelemetriaService.ObtenerUltimaLecturaAsync`. Debe manejar estado "Sin telemetría disponible". Debe armar jerarquía Parque -> Nodo -> Panel e incluir `EstadoSalud` textual + indicador por panel.
- [x] 3.4 Verificar que Application no referencia SignalR, Hubs, Infrastructure ni Controllers.
- [x] 3.5 Construir la jerarquía desde la topología registrada usando `IParqueSolarRepository`, `INodoRepository` e `IPanelRepository`.
- [x] 3.6 Cruzar telemetría por `ParqueSolarId`, `NodoId` y `PanelId`.
- [x] 3.7 Completar `Marca`, `Modelo` y `PotenciaWp` desde la entidad `Panel`.
- [x] 3.8 Mostrar `Sin telemetría` y generación `0` cuando falte lectura para un elemento.

## 4. Infraestructura (`SolarUY.Infrastructure`)
- [x] 4.1 Revisar `SolarUY/Infrastructure/` sin crear migración (sin cambios de esquema). Justificar en `specs.md` ya hecho.
- [x] 4.2 Si es necesario para el broadcast, inyectar `IHubContext<TelemetriaHub>` respetando arquitectura. No romper dependencias hacia adentro.

## 5. Presentación (`SolarUY.Controllers` + `Hubs` + `Views`)
- [x] 5.1 Crear carpeta `SolarUY/Hubs/` si no existe.
- [x] 5.2 Crear `SolarUY/Hubs/TelemetriaHub.cs` heredando `Microsoft.AspNetCore.SignalR.Hub`. Sin `[Authorize(Roles = "Administrador")]`. Sin restricción por rol.
- [x] 5.3 Crear `SolarUY/Controllers/DashboardController.cs` con acción `Index()` (GET). Inyectar `IDashboardService`. Pasar `DashboardTelemetriaViewDto` a la vista. Vista tonta.
- [x] 5.4 Crear carpeta `SolarUY/Views/Dashboard/`.
- [x] 5.5 Crear `SolarUY/Views/Dashboard/Index.cshtml`. Debe mostrar jerarquía Parque/Nodo/Panel, generación por nivel, estado de salud textual + indicador visual por panel, mensaje "Sin telemetría disponible" cuando corresponda. Conectar al hub `/hubs/telemetria` y actualizar sin recarga al recibir evento.
- [x] 5.6 Agregar referencia/uso del cliente SignalR en la vista.
- [x] 5.7 Extraer `Views/Dashboard/_DashboardContenido.cshtml`.
- [x] 5.8 Agregar `GET /Dashboard/Contenido` en `DashboardController`.
- [x] 5.9 Reemplazar `window.location.reload()` por `fetch('/Dashboard/Contenido')` y reemplazo de `#dashboardLive`.

## 6. DI y Configuración (`Program.cs`)
- [x] 6.1 En `SolarUY/Program.cs`: agregar `builder.Services.AddSignalR();`
- [x] 6.2 En `SolarUY/Program.cs`: registrar `builder.Services.AddScoped<IDashboardService, DashboardService>();`
- [x] 6.3 En `SolarUY/Program.cs`: agregar `app.MapHub<TelemetriaHub>("/hubs/telemetria");` en el pipeline.
- [x] 6.4 Verificar compilación tras cambios en `Program.cs`.

## 7. Documentación
- [x] 7.1 Actualizar `ControlCalidad/README.md` con el estado de `req008_dashboard`.
- [x] 7.2 Actualizar `README.md` si corresponde.
- [x] 7.3 Verificar que `requirements.md`, `specs.md` y `tasks.md` describan exactamente el dashboard implementado.

## 8. Verificación
- [x] 8.1 Ejecutar `docker compose run --rm tools dotnet build SolarUY.sln`
- [x] 8.2 Ejecutar `docker compose run --rm tools dotnet test SolarUY.sln` (ArchUnitNET en verde)
- [x] 8.3 Ejecutar `./ControlCalidad/SonarQube/analizar.sh` y registrar hallazgos.
- [x] 8.4 Prueba manual: acceder por ruta `/Dashboard/Index` sin restricción por rol. Ver carga inicial. Sin telemetría -> mostrar mensaje. Enviar telemetría -> actualizar vía SignalR. Verificar generación por parque/nodo/panel y estado de salud con indicador + texto por panel.
- [x] 8.5 Verificar que no se modificó `ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests`.
- [x] 8.6 Marcar todas las tareas como `[x]` solo cuando estén completadas y verificadas.
- [x] 8.7 Prueba manual: topología sin telemetría visible con aviso y paneles "Sin telemetría".
- [x] 8.8 Prueba manual: llegada de telemetría y actualización sin recarga completa.

## Notas
- Acceso por ruta, **sin restricción por rol**.
- **MVC + SignalR** (carga inicial + tiempo real).
- Reutilizar `TelemetriaDto` / `ITelemetriaService`. **Sin migración**.
- Hub en `SolarUY/Hubs/` (`TelemetriaHub`), respetando convenciones.
- Presentación solo llama a Application. Domain sin dependencias de SignalR/ASP.NET Core.
- Respetar decisiones de `Iteracion 1 Grupo 2 .NET.pdf`.

## Verificación manual (2026-11-01)

Contra la aplicación real en Docker (con el simulador en funcionamiento):

- **8.4:** `GET /Dashboard/Index` responde `200` sin sesión ni rol. Carga inicial con la topología (parques, nodos y paneles), generación por parque, nodo y panel, y estado de salud por panel con indicador (badge) y texto (`Operativo`, `EnFalla`, `Sin telemetría`). *(La primera verificación no comprobó el color del indicador y pasó por alto el defecto descrito en "Correcciones posteriores".)*
- **8.7:** con el simulador detenido y la aplicación reiniciada (sin lectura), el dashboard muestra el aviso "Sin telemetría disponible", conserva toda la topología, genera `0.00` y marca todos los paneles "Sin telemetría".
- **8.8:** un cliente SignalR (negociación, WebSocket y handshake del protocolo JSON) conectado a `/hubs/telemetria` recibió el evento `ActualizarTelemetria` cada ~5 s al llegar cada lectura, y `GET /Dashboard/Contenido` devolvió el parcial con la hora de última lectura y las generaciones nuevas. El script de la vista reemplaza solo el contenedor `#dashboardLive` con ese parcial y no recarga la página. No se inspeccionó visualmente en un navegador.
- **0.4:** se revisaron las reglas y las decisiones del PDF de la Iteración 1 (monolito, ASP.NET Core MVC, SignalR para el monitoreo en vivo); el hub vive en `Hubs/` y ArchUnitNET está en verde.

Correcciones posteriores:

- **Indicador de falla (criterio 3):** `DashboardService.EsEstadoDeFalla` buscaba los textos `Fallo`, `Error` y `FALLA`, pero el estado del contrato de telemetría es `EnFalla`; por eso `EsFallo` nunca era verdadero y los paneles en falla se mostraban con el badge verde. Ahora detecta `EnFalla`. Verificado en la aplicación real forzando fallas en el simulador: `EnFalla` → badge rojo (`bg-danger`), `Operativo` → verde y `Sin telemetría` → gris.
- `SignalRTelemetriaNotifier` (implementación de `ITelemetriaNotifier`, de Application) estaba en `SolarUY/Services/`, una carpeta fuera de las capas; se movió a `SolarUY/Hubs/` (Presentación) con el namespace `SolarUY.Hubs`.

Observaciones, sin cambios: el cliente de SignalR se carga desde `cdnjs.cloudflare.com` (requiere conexión a internet) y el dashboard y el hub no exigen sesión (la spec dice "sin restricción por rol").
