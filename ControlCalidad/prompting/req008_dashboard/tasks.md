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

## 9. Adenda: gráficas del dashboard (agregado posterior)

> Estas tareas corresponden al agregado solicitado en la revisión con el profesor.
> Se suman a las tareas anteriores; no reemplazan ni eliminan las ya completadas.

### 9.1 Preparación

- [x] 9.1.1 Revisar `requirements.md` y `specs.md` de esta adenda y confirmar el alcance de las cuatro gráficas.
- [x] 9.1.2 Revisar `SolarUY/Application/Services/DashboardService.cs`, `IDashboardService.cs`, `DashboardTelemetriaViewDto.cs`, `DashboardParqueDto.cs`, `DashboardNodoDto.cs` y `DashboardPanelDto.cs`.
- [x] 9.1.3 Revisar `SolarUY/Application/Services/HistoricoService.cs`, `IHistoricoService.cs`, `HistoricoGeneracionDto.cs` y `PuntoGraficoHistoricoDto.cs` para reutilizar la consulta de 7 días.
- [x] 9.1.4 Verificar que `SolarUY/wwwroot/lib/chart.js/chart.umd.min.js` esté disponible y que no haga falta agregar una librería nueva.

### 9.2 Aplicación (`SolarUY.Application`)

- [x] 9.2.1 Crear `DashboardBarraDto` con `Etiqueta` y `Valor`.
- [x] 9.2.2 Crear `DashboardDistribucionEstadosDto` con `Operativos`, `EnFalla`, `SinTelemetria` y `TotalPaneles`.
- [x] 9.2.3 Crear `DashboardTendenciaEnergiaDto` con `Puntos` y `TieneDatos`, reutilizando `PuntoGraficoHistoricoDto`.
- [x] 9.2.4 Crear `DashboardPaginaDto` con `Dashboard` y `Tendencia`.
- [x] 9.2.5 Extender `DashboardTelemetriaViewDto` con `GeneracionPorParque`, `GeneracionPorNodo` y `DistribucionEstadosPaneles`.
- [x] 9.2.6 En `DashboardService`, calcular `GeneracionPorParque` a partir de `DashboardParqueDto.GeneracionTotal`.
- [x] 9.2.7 En `DashboardService`, calcular `GeneracionPorNodo` con etiqueta `"<Parque> / <Nodo>"` y valor `DashboardNodoDto.Generacion`.
- [x] 9.2.8 En `DashboardService`, calcular `DistribucionEstadosPaneles` contando paneles `EnFalla`, `SinTelemetria` y `Operativos`.
- [x] 9.2.9 Inyectar `IHistoricoService` en `DashboardService` por constructor.
- [x] 9.2.10 Implementar `ObtenerTendenciaEnergiaAsync(...)` con filtro de parque, agrupación diaria y rango de los últimos 7 días.
- [x] 9.2.11 Verificar que `DashboardService` no acceda a `DbContext`, Infrastructure, SignalR ni Hubs.
- [x] 9.2.12 Verificar que la tendencia no se consulte desde `Contenido()` ni desde cada evento SignalR.

### 9.3 Presentación (`SolarUY.Controllers` + `Views` + JavaScript)

- [x] 9.3.1 Actualizar `DashboardController.Index()` para obtener el estado en vivo y la tendencia, y pasar `DashboardPaginaDto`.
- [x] 9.3.2 Mantener `DashboardController.Contenido()` devolviendo solo `DashboardTelemetriaViewDto`.
- [x] 9.3.3 Opcional: agregar `GET /Dashboard/Tendencia` para refrescar la tendencia por JSON con baja frecuencia.
- [x] 9.3.4 Actualizar `Views/Dashboard/Index.cshtml` para recibir `DashboardPaginaDto` y cargar Chart.js local.
- [x] 9.3.5 Agregar la tarjeta y el canvas de tendencia de energía de 7 días en `Index.cshtml`.
- [x] 9.3.6 Agregar las tarjetas y canvas de generación por parque, estado de paneles y generación por nodo en `_DashboardContenido.cshtml`.
- [x] 9.3.7 Serializar los datos de las gráficas con `System.Text.Json` en etiquetas `<script type="application/json">`.
- [x] 9.3.8 Implementar la inicialización de las cuatro gráficas con Chart.js.
- [x] 9.3.9 Destruir las instancias de Chart.js antes de reemplazar `#dashboardLive` y reinicializarlas después del reemplazo.
- [x] 9.3.10 Mostrar un estado vacío claro por gráfica cuando no haya datos.
- [x] 9.3.11 Usar alturas fijas y `maintainAspectRatio: false` para el comportamiento responsive.
- [x] 9.3.12 Verificar que no se agregue lógica de negocio ni cálculos en Razor o JavaScript.

### 9.4 Documentación

- [x] 9.4.1 Actualizar `ControlCalidad/README.md` con el estado de la adenda de gráficas de `req008_dashboard`.
- [x] 9.4.2 Actualizar `README.md` si la descripción del dashboard cambia.
- [x] 9.4.3 Verificar que `requirements.md`, `specs.md` y este `tasks.md` describan exactamente las gráficas implementadas.

### 9.5 Verificación

- [x] 9.5.1 Ejecutar `docker compose run --rm tools dotnet build SolarUY.sln` (verificado con Docker el 2026-11-11; build OK con warnings preexistentes NU1902/NU1903).
- [x] 9.5.2 Ejecutar `docker compose run --rm tools dotnet test SolarUY.sln` (verificado con Docker el 2026-11-11: 161 + 121 + 9 pruebas en verde).
- [ ] 9.5.3 Ejecutar `./ControlCalidad/SonarQube/analizar.sh` (o `bash ControlCalidad/SonarQube/analizar.sh` si no tiene permiso de ejecución) y registrar hallazgos.
- [x] 9.5.4 Prueba manual sin telemetría: las cuatro gráficas muestran estado vacío y la topología sigue visible. Verificado por HTML/JSON de `GET /Dashboard/Index`, `Contenido` y `Tendencia` con la base vacía.
- [x] 9.5.5 Prueba manual con telemetría: se ven las barras por parque, la dona de estados y las barras por nodo. Verificado con dos parques, cuatro nodos y ocho paneles temporales; luego se limpiaron los datos.
- [x] 9.5.6 Prueba manual con “Simular falla”: la dona cambia a `EnFalla` en la siguiente lectura. Verificado en la distribución devuelta por `Contenido` y en los endpoints de control con sesión de administrador.
- [x] 9.5.7 Prueba manual con históricos: la tendencia de 7 días muestra datos; sin históricos muestra un mensaje claro. Verificado el JSON con un punto diario y el mensaje vacío al limpiar la base.
- [x] 9.5.8 Verificar que el evento SignalR actualiza las gráficas en vivo sin recargar la página y sin errores de Chart.js. Verificado por el usuario en navegador el 2026-11-11.
- [x] 9.5.9 Verificar que `GET /Dashboard/Index` sigue funcionando sin restricción por rol. Verificado con `200` sin cookie de sesión.
- [x] 9.5.10 Verificar que no se modificó `ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests`.
- [x] 9.5.11 Marcar las tareas de esta adenda como `[x]` solo cuando estén completadas y verificadas.

### Notas de la adenda

- Se mantienen las tarjetas KPI y las tablas existentes.
- Las gráficas en vivo se actualizan con SignalR; la tendencia histórica se mantiene separada y con baja frecuencia de refresco.
- No se agrega persistencia, migración ni una librería nueva.
- El dashboard sigue siendo de solo lectura y sin restricción por rol.
- No se usa `EnergiaAcumuladaKWh` del simulador como energía diaria.
- `DashboardDistribucionEstadosDto` expone además `TotalPaneles` para que la vista no realice sumas.
- El 2026-11-11 se ejecutaron build, tests y pruebas manuales con Docker. Las gráficas y la actualización en vivo fueron verificadas por el usuario en navegador. Queda pendiente SonarQube (falta `SONAR_TOKEN`).
