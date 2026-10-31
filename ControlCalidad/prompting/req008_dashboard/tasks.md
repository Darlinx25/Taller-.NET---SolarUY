# Tareas: req008_dashboard - Dashboard de telemetría en vivo

> Cada tarea debe ser verificable, indicar el archivo o la ruta cuando aplique y comenzar con `[ ]`. Marcar `[x]` solo cuando esté terminada y verificada.

## 0. Preparación
- [ ] 0.1 Confirmar que `requirements.md`, `specs.md` y este plan están aprobados.
- [ ] 0.2 Revisar `req002_gestion_parques`, `req003_nodos_inversores`, `req004_gestion_paneles` para imitar nombres y patrones.
- [ ] 0.3 Revisar `SolarUY/Controllers/TelemetriaController.cs`, `SolarUY/Application/Services/ITelemetriaService.cs`, `SolarUY/Application/Services/TelemetriaService.cs`, `SolarUY/Application/Dtos/TelemetriaDto.cs` y `SolarUY/Program.cs` para reutilizar contratos.
- [ ] 0.4 Revisar reglas en `ControlCalidad/.agents/rules/` y decisiones en `D:\Proyectos\Iteracion 1 Grupo 2 .NET.pdf`.

## 1. Dominio (`SolarUY.Domain`)
- [ ] 1.1 Verificar entidades/repositorios en `SolarUY/Domain/`. No crear cambios de dominio ni migración.

## 2. Tests del Dominio (`SolarUY.Tests/Domain`)
- [ ] 2.1 Revisar si aplica agregar tests xUnit en `SolarUY.Tests/Domain/`. Si no hay comportamiento nuevo en Dominio, no agregar tests.

## 3. Aplicación (`SolarUY.Application`)
- [ ] 3.1 Crear DTOs en `SolarUY/Application/Dtos/`: `DashboardTelemetriaViewDto.cs`, `DashboardParqueDto.cs`, `DashboardNodoDto.cs`, `DashboardPanelDto.cs`.
- [ ] 3.2 Crear interfaz `SolarUY/Application/Services/IDashboardService.cs`.
- [ ] 3.3 Crear implementación `SolarUY/Application/Services/DashboardService.cs` (inyección por constructor). Debe combinar topología existente con `ITelemetriaService.ObtenerUltimaLecturaAsync`. Debe manejar estado "Sin telemetría disponible". Debe armar jerarquía Parque -> Nodo -> Panel e incluir `EstadoSalud` textual + indicador por panel.
- [ ] 3.4 Verificar que Application no referencia SignalR, Hubs, Infrastructure ni Controllers.

## 4. Infraestructura (`SolarUY.Infrastructure`)
- [ ] 4.1 Revisar `SolarUY/Infrastructure/` sin crear migración (sin cambios de esquema). Justificar en `specs.md` ya hecho.
- [ ] 4.2 Si es necesario para el broadcast, inyectar `IHubContext<TelemetriaHub>` respetando arquitectura. No romper dependencias hacia adentro.

## 5. Presentación (`SolarUY.Controllers` + `Hubs` + `Views`)
- [ ] 5.1 Crear carpeta `SolarUY/Hubs/` si no existe.
- [ ] 5.2 Crear `SolarUY/Hubs/TelemetriaHub.cs` heredando `Microsoft.AspNetCore.SignalR.Hub`. Sin `[Authorize(Roles = "Administrador")]`. Sin restricción por rol.
- [ ] 5.3 Crear `SolarUY/Controllers/DashboardController.cs` con acción `Index()` (GET). Inyectar `IDashboardService`. Pasar `DashboardTelemetriaViewDto` a la vista. Vista tonta.
- [ ] 5.4 Crear carpeta `SolarUY/Views/Dashboard/`.
- [ ] 5.5 Crear `SolarUY/Views/Dashboard/Index.cshtml`. Debe mostrar jerarquía Parque/Nodo/Panel, generación por nivel, estado de salud textual + indicador visual por panel, mensaje "Sin telemetría disponible" cuando corresponda. Conectar al hub `/hubs/telemetria` y actualizar sin recarga al recibir evento.
- [ ] 5.6 Agregar referencia/uso del cliente SignalR en la vista.

## 6. DI y Configuración (`Program.cs`)
- [ ] 6.1 En `SolarUY/Program.cs`: agregar `builder.Services.AddSignalR();`
- [ ] 6.2 En `SolarUY/Program.cs`: registrar `builder.Services.AddScoped<IDashboardService, DashboardService>();`
- [ ] 6.3 En `SolarUY/Program.cs`: agregar `app.MapHub<TelemetriaHub>("/hubs/telemetria");` en el pipeline.
- [ ] 6.4 Verificar compilación tras cambios en `Program.cs`.

## 7. Documentación
- [ ] 7.1 Actualizar `ControlCalidad/README.md` con el estado de `req008_dashboard`.
- [ ] 7.2 Actualizar `README.md` si corresponde.

## 8. Verificación
- [ ] 8.1 Ejecutar `docker compose run --rm tools dotnet build SolarUY.sln`
- [ ] 8.2 Ejecutar `docker compose run --rm tools dotnet test SolarUY.sln` (ArchUnitNET en verde)
- [ ] 8.3 Ejecutar `./ControlCalidad/SonarQube/analizar.sh` y registrar hallazgos.
- [ ] 8.4 Prueba manual: acceder por ruta `/Dashboard/Index` sin restricción por rol. Ver carga inicial. Sin telemetría -> mostrar mensaje. Enviar telemetría -> actualizar vía SignalR. Verificar generación por parque/nodo/panel y estado de salud con indicador + texto por panel.
- [ ] 8.5 Verificar que no se modificó `ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests`.
- [ ] 8.6 Marcar todas las tareas como `[x]` solo cuando estén completadas y verificadas.

## Notas
- Acceso por ruta, **sin restricción por rol**.
- **MVC + SignalR** (carga inicial + tiempo real).
- Reutilizar `TelemetriaDto` / `ITelemetriaService`. **Sin migración**.
- Hub en `SolarUY/Hubs/` (`TelemetriaHub`), respetando convenciones.
- Presentación solo llama a Application. Domain sin dependencias de SignalR/ASP.NET Core.
- Respetar decisiones de `Iteracion 1 Grupo 2 .NET.pdf`.
