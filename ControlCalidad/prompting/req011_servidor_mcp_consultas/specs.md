# Especificacion Tecnica: Servidor MCP de consultas en lenguaje natural

## Objetivo

Incorporar al monolito SolarUY un servidor MCP embebido sobre Streamable HTTP que
exponga herramientas de solo lectura. Un agente externo (Antigravity + Gemini) se
conecta y traduce las preguntas en lenguaje natural a llamadas a esas herramientas.
La aplicacion no llama a ningun LLM ni guarda credenciales de Gemini.

## Decisiones y supuestos

- SDK oficial `ModelContextProtocol.AspNetCore` version 2.3.1 (ultima estable al
  2026-11-03, compatible con `net10.0`).
- Servidor embebido en el proyecto `SolarUY` (no se crea un proyecto aparte).
- Transporte Streamable HTTP en modo stateless; endpoint `/mcp`.
- Acceso anonimo, solo lectura, para el entorno local de la POC.
- Herramientas en `SolarUY/Controllers/Mcp/SolarUyMcpTools.cs`, namespace
  `SolarUY.Controllers.Mcp` (Presentacion). Dependen solo de Application.
- El SDK construye una instancia de la clase de herramientas por invocacion con
  `ActivatorUtilities`; no se registra en el contenedor de dependencias.
- Se reutilizan DTOs y servicios existentes; no se duplica logica de negocio.
- `ITopologiaSimulacionService` se reutiliza como servicio de consulta de topologia
  (ya es de solo lectura y vive en Application).
- No hay migracion ni cambios de dominio.
- Las herramientas lanzan `McpException` (namespace `ModelContextProtocol`) con
  mensajes en espanol para errores de validacion o entidad inexistente; el SDK los
  devuelve como `CallToolResult` con `IsError = true`.
- La configuracion `Mcp:Habilitado` (por defecto `true`) permite apagar el endpoint.
- El puerto de la aplicacion se publica solo en `127.0.0.1` para que el endpoint
  anonimo no quede accesible desde la red local. `AllowedHosts` no se restringe
  porque el simulador llama a la aplicacion como `http://app:8080` dentro de la red
  de Docker.

## 1) Dominio (`SolarUY.Domain`)

### Entidades

Sin cambios. No se agregan entidades.

### Validaciones

Sin cambios. Se reutilizan las validaciones de `FiltroHistorico` y las excepciones
de dominio existentes.

### Excepciones

Sin cambios.

### Repositorios (contratos)

Se agrega un metodo de solo lectura a `ITareaMantenimientoRepository`:

- `Task<IReadOnlyList<TareaMantenimiento>> ObtenerTodasAsync(CancellationToken cancellationToken = default);`

No se modifica ningun otro contrato.

## 2) Aplicacion (`SolarUY.Application`)

### DTOs

Sin DTOs nuevos. Se reutilizan:

- `ParqueSolarDto` para `listar_parques`.
- `TopologiaSimulacionDto` para `obtener_topologia`.
- `DashboardTelemetriaViewDto` para `obtener_estado_actual`.
- `HistoricoGeneracionDto` para `obtener_historico`.
- `ListadoAlertasDto` para `obtener_alertas`.
- `TareaMantenimientoDto` para `listar_tareas_mantenimiento`.

### Servicios y casos de uso

- `IParqueSolarService.ObtenerParquesAsync` para listar parques.
- `ITopologiaSimulacionService.ObtenerAsync` para la topologia.
- `IDashboardService.ObtenerDashboardAsync` para el estado actual.
- `IHistoricoService.ConsultarAsync` para los historicos.
- `IAlertasService.ObtenerListadoAsync` para las alertas.
- `ITareaMantenimientoService` incorpora:
  `Task<IReadOnlyList<TareaMantenimientoDto>> ObtenerTareasAsync(string? estado, Guid? panelId, CancellationToken cancellationToken = default);`
  Implementacion: si `panelId` tiene valor usa `ObtenerPorPanelAsync`; si no,
  `ObtenerTodasAsync`. Luego filtra por `Estado` (sin distinguir mayusculas) cuando
  se indica y mapea con el mapper existente. Los estados validos son
  `Pendiente`, `EnProceso`, `Completada` y `Cancelada`.
- No se agrega logica de negocio nueva a la capa de Presentacion.

## 3) Infraestructura (`SolarUY.Infrastructure`)

### DbSet y configuraciones

Sin cambios.

### Repositorios

`TareaMantenimientoRepository` implementa `ObtenerTodasAsync` con una consulta de
solo lectura, ordenada por `FechaCreacion` descendente.

### Migracion

No hay cambios de esquema, por lo que **no se genera migracion**.

### Registro de dependencias (`Program.cs`)

- `builder.Services.AddMcpServer().WithHttpTransport(options => options.Stateless = true).WithTools<SolarUY.Controllers.Mcp.SolarUyMcpTools>();`
- `app.MapMcp("/mcp");` solo si `Mcp:Habilitado` es `true`.
- Seccion `Mcp` en `appsettings.json`:
  `{ "Mcp": { "Habilitado": true, "Ruta": "/mcp" } }`
- El puerto de la aplicacion en `docker-compose.yml` se publica en
  `127.0.0.1:${APP_PORT:-5080}:8080`.

## 4) Presentacion (`SolarUY.Controllers` + `Views`)

### Controlador y acciones

No se crea un controlador MVC. Se crea `SolarUY/Controllers/Mcp/SolarUyMcpTools.cs`
con `[McpServerToolType]`, inyeccion por constructor de los servicios de Application
y las siguientes herramientas:

- `listar_parques` -> `IParqueSolarService.ObtenerParquesAsync`.
- `obtener_topologia` -> `ITopologiaSimulacionService.ObtenerAsync`.
- `obtener_estado_actual` -> `IDashboardService.ObtenerDashboardAsync`.
- `obtener_historico(string tipoEntidad, string desde, string hasta, string agrupacion, Guid? entidadId = null)`
  -> parsea enums (`TipoEntidadHistorico`, `AgrupacionHistorico`) y fechas
  (`yyyy-MM-dd`), arma `FiltroHistoricoDto` y llama a `IHistoricoService.ConsultarAsync`.
  Captura `FiltroHistoricoInvalidoException` y relanza `McpException` con su mensaje.
- `obtener_alertas` -> `IAlertasService.ObtenerListadoAsync`.
- `listar_tareas_mantenimiento(string? estado, Guid? panelId)` -> usa
  `ITareaMantenimientoService.ObtenerTareasAsync`.

Cada metodo lleva
`[McpServerTool(Name = "...", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false), Description("...")]`
y descripciones de parametros claras. Los metodos convierten parametros de entrada
y delegan; no contienen reglas de negocio.

### Autorizacion

Anonima y de solo lectura. No se aplica `[Authorize]`. El endpoint se habilita o
deshabilita con `Mcp:Habilitado`.

### Vistas

Sin vistas nuevas.

## 5) Pruebas

- Tests xUnit del Dominio (obligatorios): no hay comportamiento nuevo de dominio,
  por lo que no se agregan tests; los existentes deben seguir en verde.
- No se agregan tests en Application, Infrastructure ni Presentation.
- Verificacion manual con MCP Inspector o `curl` (`tools/list` y `tools/call`) y
  con Antigravity.

## 6) Calidad y verificacion

- [ ] Build: `docker compose run --rm tools dotnet build SolarUY.sln`
- [ ] Tests: `docker compose run --rm tools dotnet test SolarUY.sln`
- [ ] ArchUnitNET en verde
- [ ] SonarQube ejecutado y hallazgos revisados
- [ ] Prueba manual del flujo completo (MCP Inspector + Antigravity)

## 7) Fuera de alcance

- Chat dentro de SolarUY.
- Autenticacion del endpoint MCP.
- Escrituras o comandos al inversor.
- Persistencia de consultas/respuestas.
- Cambios en telemetria, simulador o contrato interno.
- Limites de uso, cuotas y alta disponibilidad.
