# Tareas: req011 Servidor MCP de consultas en lenguaje natural

> Cada tarea debe ser verificable, indicar el archivo o la ruta cuando aplique
> y comenzar con `[ ]`. Marcar `[x]` solo cuando este terminada y verificada.

## 0. Preparacion

- [x] 0.1 Confirmar que `requirements.md`, `specs.md` y este plan estan aprobados
      (aprobacion implicita en la orden de implementar).
- [x] 0.2 Revisar `SolarUY/Program.cs`, `SolarUY/SolarUY.csproj`, los servicios y
      DTOs de Application y el patron de `TareaMantenimientoService`.
- [x] 0.3 Confirmar que `docker compose run --rm tools dotnet build SolarUY.sln` y
      `docker compose run --rm tools dotnet test SolarUY.sln` parten en verde.

## 1. Dominio (`SolarUY.Domain`)

- [x] 1.1 Confirmar que no hay cambios de entidades, validaciones ni excepciones.
- [x] 1.2 Agregar `ObtenerTodasAsync(CancellationToken)` a
      `SolarUY/Domain/Repositories/ITareaMantenimientoRepository.cs`.

## 2. Tests del Dominio (`SolarUY.Tests/Domain`)

- [x] 2.1 Confirmar que no hay logica de dominio nueva y que los tests existentes
      siguen pasando; no se agregan tests nuevos.

## 3. Aplicacion (`SolarUY.Application`)

- [x] 3.1 Agregar `ObtenerTareasAsync(string? estado, Guid? panelId, CancellationToken)`
      a `SolarUY/Application/Services/ITareaMantenimientoService.cs`.
- [x] 3.2 Implementar `ObtenerTareasAsync` en
      `SolarUY/Application/Services/TareaMantenimientoService.cs` (elegir
      `ObtenerPorPanelAsync` u `ObtenerTodasAsync`, filtrar por estado sin
      distinguir mayusculas y mapear con el mapper existente).
- [x] 3.3 Confirmar que no se agregan DTOs nuevos y que Application no referencia
      MCP ni Infrastructure.

## 4. Infraestructura (`SolarUY.Infrastructure`)

- [x] 4.1 Implementar `ObtenerTodasAsync` en
      `SolarUY/Infrastructure/Repositories/TareaMantenimientoRepository.cs`
      (solo lectura, orden por `FechaCreacion` descendente).
- [x] 4.2 Agregar el paquete `ModelContextProtocol.AspNetCore` (2.3.1) a
      `SolarUY/SolarUY.csproj`.
- [x] 4.3 En `SolarUY/Program.cs`, registrar
      `AddMcpServer().WithHttpTransport(options => options.Stateless = true).WithTools<SolarUyMcpTools>()`.
- [x] 4.4 En `SolarUY/Program.cs`, mapear `app.MapMcp("/mcp")` condicionado a
      `Mcp:Habilitado`.
- [x] 4.5 Agregar la seccion `Mcp` a `SolarUY/appsettings.json`.
- [x] 4.6 Publicar el puerto de `app` solo en `127.0.0.1` en `docker-compose.yml`.
- [x] 4.7 Confirmar que no se genera migracion.

## 5. Presentacion (`SolarUY.Controllers`)

- [x] 5.1 Crear `SolarUY/Controllers/Mcp/SolarUyMcpTools.cs` con
      `[McpServerToolType]` e inyeccion por constructor.
- [x] 5.2 Implementar `listar_parques` con `IParqueSolarService`.
- [x] 5.3 Implementar `obtener_topologia` con `ITopologiaSimulacionService`.
- [x] 5.4 Implementar `obtener_estado_actual` con `IDashboardService`.
- [x] 5.5 Implementar `obtener_historico` (parseo de enums y fechas, armado de
      `FiltroHistoricoDto`, `IHistoricoService.ConsultarAsync`, errores como
      `McpException`).
- [x] 5.6 Implementar `obtener_alertas` con `IAlertasService`.
- [x] 5.7 Implementar `listar_tareas_mantenimiento` con `ITareaMantenimientoService`.
- [x] 5.8 Confirmar que las tools no acceden a Infrastructure ni contienen logica
      de negocio, y que el endpoint es anonimo y de solo lectura.

## 6. Configuracion de Antigravity y documentacion

- [x] 6.1 Crear `.agents/mcp_config.json` en la raiz con
      `serverUrl: "http://127.0.0.1:5080/mcp"`.
- [x] 6.2 Actualizar `README.md`: pasar "Consulta IA" a `Listo`, describir el
      servidor MCP y como conectar Antigravity.
- [x] 6.3 Actualizar `DESARROLLO.md` con la seccion de Antigravity/MCP y la
      verificacion alternativa con MCP Inspector.
- [x] 6.4 Agregar `req011_servidor_mcp_consultas` a la tabla de
      `ControlCalidad/README.md`.

## 7. Verificacion

- [x] 7.1 `docker compose run --rm tools dotnet build SolarUY.sln` sin errores.
- [x] 7.2 `docker compose run --rm tools dotnet test SolarUY.sln` con Dominio y
      ArchUnitNET en verde (161 + 102 + 9 pruebas).
- [x] 7.3 Ejecutar `./ControlCalidad/SonarQube/analizar.sh` y registrar hallazgos.
      Analisis ejecutado (`ANALYSIS SUCCESSFUL`). Gate en **ERROR** por deuda
      preexistente: cobertura nueva 0%, duplicacion nueva 5,1% y 10 hallazgos
      nuevos en `SolarUY.Simulador`, `RegistroAlertasService` y
      `alertas-indicador.js`; **ninguno** en el codigo de req011.
- [x] 7.4 Prueba manual con MCP Inspector o `curl`: `tools/list` muestra las 6
      herramientas y `tools/call` responde para cada una. Verificado con `curl`
      contra `http://127.0.0.1:5080/mcp` (initialize, tools/list y las 6 tools).
      Los comandos quedaron documentados en `pruebas-curl.md`.
- [ ] 7.5 Prueba manual con Antigravity conectado: preguntar por el estado actual,
      un historico y las alertas/tareas. **Pendiente:** requiere abrir Antigravity
      y cargar `.agents/mcp_config.json`.
- [x] 7.6 Confirmar que no se modifico `CodeQuality.Tests`, que no hay migracion y
      que `git status` solo muestra los archivos previstos.
- [ ] 7.7 Marcar todas las tareas como `[x]` solo cuando esten verificadas.
      **Pendiente:** 7.5.

## Notas

- Sin migracion de base de datos.
- Acceso anonimo y de solo lectura.
- No hay chat dentro de SolarUY: la interaccion ocurre en Antigravity.
- La aplicacion no usa key de Gemini.
- No se crean tests en Application, Infrastructure ni Presentation.
- No se modifica `CodeQuality.Tests` sin autorizacion explicita.
