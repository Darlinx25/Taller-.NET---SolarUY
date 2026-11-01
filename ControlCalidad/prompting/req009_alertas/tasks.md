# Tareas: req009 Alertas por fallos en paneles

> Cada tarea debe ser verificable, indicar el archivo o la ruta cuando aplique y comenzar con `[ ]`. Marcar `[x]` solo cuando esté terminada y verificada.

## 0. Preparación

- [x] 0.1 Confirmar que `requirements.md`, `specs.md` y este plan están aprobados por una persona antes de implementar.
- [x] 0.2 Revisar las reglas de `ControlCalidad/.agents/rules/` y los patrones de `req002_gestion_parques`, `req003_nodos_inversores`, `req007_historicos_generacion` (entidad persistida, repositorio, migración, ingesta) y `req008_dashboard` (hub y notificador de SignalR).
- [x] 0.3 Confirmar el punto de partida: `docker compose run --rm tools dotnet build SolarUY.sln` y `docker compose run --rm tools dotnet test SolarUY.sln` en verde, y `git grep -il AlertaPanel -- SolarUY SolarUY.Tests` sin resultados.

## 1. Dominio (`SolarUY.Domain`)

- [x] 1.1 Crear `SolarUY/Domain/Entities/AlertaPanel.cs` con las constantes de longitud (`ParqueSolar.NombreMaxLength`, `Nodo.CodigoMaxLength`, `Panel.CodigoSerieMaxLength`), constructor privado para EF Core, propiedades con `private set`, `EstaActiva`, `Abrir`, `Cerrar` (devuelve `bool`), `CalcularDuracion` y `DebeAbrirse`, con las validaciones y mensajes en español de `specs.md`.
- [x] 1.2 Crear `SolarUY/Domain/Repositories/IAlertaPanelRepository.cs` con `ObtenerActivasAsync`, `ObtenerCerradasRecientesAsync`, `ObtenerUltimaPorPanelAsync`, `ContarActivasAsync`, `AgregarAsync` y `SaveChangesAsync`.
- [x] 1.3 Confirmar que `SolarUY/Domain/` no referencia EF Core, ASP.NET Core, SignalR, `Application`, `Infrastructure` ni `Controllers`.

## 2. Tests del Dominio (`SolarUY.Tests/Domain`)

- [x] 2.1 Crear `SolarUY.Tests/Domain/AlertaPanelTests.cs` con los casos de `Abrir`: datos válidos (activa, `Id` no vacío, trim), identificadores vacíos, textos vacíos o de solo espacios, textos que superan la longitud máxima y fecha `default`.
- [x] 2.2 Agregar a `AlertaPanelTests.cs` los casos de `Cerrar`: cierra y devuelve `true`; fin igual o anterior al inicio devuelve `false` sin modificar la alerta; fecha `default` lanza `ArgumentException`; alerta ya cerrada lanza `InvalidOperationException`.
- [x] 2.3 Agregar a `AlertaPanelTests.cs` los casos de `CalcularDuracion` (alerta cerrada, alerta activa hasta `ahoraUtc`, resultado nunca negativo, `ahoraUtc` `default`) y de `DebeAbrirse` (sin previa, previa activa, previa cerrada con fecha posterior, igual y anterior a su fin, fecha `default`).
- [x] 2.4 Ejecutar `docker compose run --rm tools dotnet test SolarUY.Tests` y confirmar que los tests nuevos y los existentes están en verde.

## 3. Aplicación (`SolarUY.Application`)

- [x] 3.1 Crear en `SolarUY/Application/Dtos/` `AlertaPanelDto.cs` y `ListadoAlertasDto.cs` con las propiedades de `specs.md`.
- [x] 3.2 Crear `SolarUY/Application/Services/IAlertasNotifier.cs` con `NotificarAlertasActualizadasAsync`.
- [x] 3.3 Crear `SolarUY/Application/Services/IRegistroAlertasService.cs` y `RegistroAlertasService.cs` con el algoritmo de `specs.md`: ignorar lecturas sin parques, abrir alertas con `AlertaPanel.DebeAbrirse`, cerrar por `Operativo` o por ausencia en la lectura, un único `SaveChangesAsync`, log informativo y notificación solo si hubo cambios (con el fallo del notificador capturado).
- [x] 3.4 Crear `SolarUY/Application/Services/IAlertasService.cs` y `AlertasService.cs` con `ObtenerListadoAsync` (activas y las 50 cerradas más recientes, con `Duracion` calculada con `TimeProvider`) y `ContarActivasAsync`.
- [x] 3.5 Modificar `SolarUY/Application/Services/TelemetriaService.cs`: agregar la dependencia `IRegistroAlertasService` (después de `IRegistroHistoricoService`) y llamarla después del registro histórico y antes de `IAlmacenTelemetria.GuardarAsync`.
- [x] 3.6 Confirmar que los archivos nuevos de `SolarUY/Application/` no referencian EF Core, ASP.NET Core, SignalR, `Infrastructure` ni `Controllers`.

## 4. Infraestructura (`SolarUY.Infrastructure`)

- [x] 4.1 Agregar `DbSet<AlertaPanel> AlertasPanel` a `SolarUY/Infrastructure/Persistence/SolarUYDbContext.cs`.
- [x] 4.2 Crear `SolarUY/Infrastructure/Persistence/Configurations/AlertaPanelConfiguration.cs` con la tabla `AlertasPanel`, clave `Id` sin generación, longitudes, campos requeridos, FK a `ParqueSolar`, `Nodo` y `Panel` con `Restrict`, y los índices `IX_AlertasPanel_PanelId_FechaInicioUtc` e `IX_AlertasPanel_FechaFinUtc`.
- [x] 4.3 Crear `SolarUY/Infrastructure/Repositories/AlertaPanelRepository.cs` implementando `IAlertaPanelRepository` con los criterios de orden y tracking de `specs.md`.
- [x] 4.4 Registrar en `SolarUY/Program.cs` `IAlertaPanelRepository`, `IRegistroAlertasService`, `IAlertasService` e `IAlertasNotifier` (con `SolarUY.Hubs.SignalRAlertasNotifier`) como `AddScoped`.
- [x] 4.5 Generar la migración `AgregarAlertasPanel` con `docker compose run --rm tools bash -lc "dotnet restore SolarUY.sln && dotnet ef migrations add AgregarAlertasPanel --project SolarUY --startup-project SolarUY"` (el `restore` previo es necesario si los `obj` quedaron restaurados desde Windows) y revisar que solo crea la tabla `AlertasPanel`, sus tres claves foráneas y sus índices (los dos definidos más los automáticos de las FK).
- [x] 4.6 Aplicar la migración con `docker compose up -d --build` (servicio `migrate`) y confirmar que la tabla `AlertasPanel` existe en MySQL.
- [x] 4.7 Ejecutar `docker compose run --rm tools dotnet build SolarUY.sln` y confirmar que compila.

## 5. Presentación (`SolarUY.Controllers` + `Views`)

- [x] 5.1 Crear `SolarUY/Hubs/SignalRAlertasNotifier.cs` (namespace `SolarUY.Hubs`) que implementa `IAlertasNotifier` y emite `ActualizarAlertas` con `IHubContext<TelemetriaHub>`.
- [x] 5.2 Crear `SolarUY/Controllers/AlertasController.cs` sin `[Authorize]`, con `Index` (devuelve `ListadoAlertasDto`) y `CantidadActivas` (devuelve `Json(new { cantidadActivas })`), inyectando solo `IAlertasService`.
- [x] 5.3 Crear `SolarUY/Views/Alertas/Index.cshtml` con las secciones de alertas activas (aviso "Sin alertas activas.") e historial (aviso "Todavía no hay alertas cerradas."), fechas en UTC rotuladas y duración como `horas:minutos:segundos`.
- [x] 5.4 Crear `SolarUY/wwwroot/js/alertas-indicador.js` que consulta `/Alertas/CantidadActivas`, actualiza el contador del menú y se actualiza con el evento `ActualizarAlertas` y al reconectar, sin romper la página si SignalR no carga.
- [x] 5.5 Modificar `SolarUY/Views/Shared/_Layout.cshtml`: enlace "Alertas" visible para todos con el contador `alertasContador`, y los scripts del cliente de SignalR (mismo CDN e integridad que el Dashboard) y `alertas-indicador.js` antes de `RenderSectionAsync("Scripts")`.
- [x] 5.6 Confirmar que `AlertasController` y las vistas no acceden a `Infrastructure` ni contienen lógica de negocio, y que no se crearon formularios ni acciones de escritura.

## 6. Documentación

- [x] 6.1 Agregar `req009_alertas` a la tabla de `ControlCalidad/README.md` con el estado correspondiente (Planificado / En progreso / Implementado).
- [x] 6.2 Actualizar `README.md`: pasar "Alertas" a `Listo` y agregar una breve sección sobre las alertas (cuándo se abren y se cierran, la pantalla `/Alertas`, el indicador en vivo y el acceso sin sesión).
- [x] 6.3 Confirmar que `requirements.md`, `specs.md` y `tasks.md` describen exactamente lo implementado.

## 7. Verificación

- [x] 7.1 Build: `docker compose run --rm tools dotnet build SolarUY.sln` sin errores.
- [x] 7.2 Tests: `docker compose run --rm tools dotnet test SolarUY.sln` con los tests de Dominio y ArchUnitNET en verde.
- [ ] 7.3 Ejecutar `./ControlCalidad/SonarQube/analizar.sh` (o `analizar.ps1` en Windows) y registrar hallazgos.
- [x] 7.4 Prueba manual:
  - `GET /Alertas` y `GET /Alertas/CantidadActivas` responden `200` sin sesión;
  - con `Simulador__ProbabilidadFalla=1` en un simulador efímero y el de compose detenido, aparecen alertas activas en `/Alertas` y el indicador del menú sube sin recargar la página;
  - al volver a una probabilidad baja, las alertas pasan al historial con fecha de fin y duración, y el indicador baja sin recargar;
  - reenviar la misma lectura no duplica alertas;
  - un nodo que pasa a `EnMantenimiento` cierra las alertas de sus paneles;
  - sin fallos, la pantalla muestra "Sin alertas activas.";
  - la tabla `AlertasPanel` conserva las alertas cerradas.
- [x] 7.5 Confirmar que no se modificó `ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests`, el simulador ni el contrato de telemetría, y que `git status` solo muestra los archivos previstos.
- [x] 7.6 Registrar en este archivo las evidencias de la verificación final.

## Notas

- Hay migración: `AgregarAlertasPanel` crea la tabla `AlertasPanel`.
- Acceso anónimo y de solo lectura (rol: Todos), igual que el Dashboard.
- El registro de alertas es parte de la ingesta: si falla, la lectura no se guarda y el simulador reintenta, igual que con los históricos.
- El indicador del menú se actualiza en vivo; el listado de la pantalla se actualiza al recargar.
- Solo hay tests xUnit del Dominio. No se crean tests unitarios en Application, Infrastructure ni Presentation.
- No se modifica `CodeQuality.Tests` sin autorización explícita.

## Evidencias de la verificación (2026-11-01)

- **Build y tests (contenedor `tools`):** `docker compose run --rm tools dotnet build SolarUY.sln` sin errores ni advertencias; `docker compose run --rm tools dotnet test SolarUY.sln`: `SolarUY.Tests` 161 correctos (34 nuevos de `AlertaPanelTests`), `SolarUY.Simulador.Tests` 102 y `CodeQuality.Tests` (ArchUnitNET) 9, todos en verde.
- **Migración:** `AgregarAlertasPanel` crea solo la tabla `AlertasPanel` con sus tres claves foráneas (`Restrict`) e índices (los dos definidos más los automáticos de `NodoId` y `ParqueSolarId`). La aplicó el servicio `migrate` y `SHOW CREATE TABLE AlertasPanel` confirma columnas, claves e índices. El snapshot cambió además `FechaLocal` de `DateOnly` a `DateTime` en los resúmenes diarios (ajuste del snapshot, sin `AlterColumn`).
- **Acceso:** `GET /Alertas` y `GET /Alertas/CantidadActivas` responden `200` sin sesión. El menú de una página cualquiera trae el enlace "Alertas", el contador `alertasContador`, el cliente de SignalR (mismo CDN y hash que el Dashboard) y `alertas-indicador.js`.
- **Apertura:** con el simulador normal se abrió una alerta real. Con un simulador efímero con `Simulador__ProbabilidadFalla=1`, aparecieron 4 alertas activas (una por panel de nodo activo) en `/Alertas` y en `CantidadActivas`; tras varias lecturas más no hubo duplicados (una activa por panel en la base).
- **Cierre por ausencia y reapertura:** al pasar `NODO-MOCK-01` a `EnMantenimiento`, sus 3 alertas se cerraron y quedó 1 activa; al volver a `Activo`, sus paneles reaparecieron en falla y se abrieron 3 alertas nuevas (las anteriores quedaron en el historial).
- **Lecturas repetidas y anteriores:** reenviar 3 veces la misma lectura (`204`) dejó la base igual (18 alertas, 4 activas); una lectura de 1 hora antes con todos los paneles `Operativo` tampoco cerró ni abrió nada.
- **Recuperación:** con un simulador sin fallas (`ProbabilidadFalla=0`) las 4 alertas se cerraron; `/Alertas` muestra "Sin alertas activas." y el historial con inicio, fin y duración, y la tabla conserva las 18 alertas cerradas.
- **SignalR:** un cliente de consola conectado a `/hubs/telemetria` recibió `ActualizarAlertas` solo cuando se abrió o cerró alguna alerta (no en cada lectura) y `CantidadActivas` devolvió el valor esperado en cada evento (4 → 0 → 4 → 1 → 4 → 0, según el ciclo de falla forzado de 6 lecturas, el mantenimiento del nodo y la recuperación). No se inspeccionó visualmente el indicador en un navegador.
- **Registro de la app:** `Alertas de paneles actualizadas con la lectura ...: N abierta(s), M cerrada(s).` solo cuando hay cambios.
- **Sin cambios ajenos:** `git status` muestra solo los archivos previstos de req009; `ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests`, el simulador y el contrato de telemetría no se modificaron. Quedan, ajenos a este requerimiento, la corrección de `DashboardService` y la nota de `req008_dashboard/tasks.md`.
- **SonarQube (pendiente):** no hay `SONAR_TOKEN` en el `.env` local. Falta ejecutar `./ControlCalidad/SonarQube/analizar.ps1` y registrar los hallazgos; con eso `req009_alertas` pasa a `Implementado` en `ControlCalidad/README.md` (hoy figura `En progreso`).
