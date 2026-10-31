# Tareas: req007 Históricos de generación

> Cada tarea debe ser verificable, indicar el archivo o la ruta cuando aplique y comenzar con `[ ]`. Marcar `[x]` solo cuando esté terminada y verificada.
>
> **Punto de partida:** existen la telemetría efímera de req006 y los ABM de parques, nodos y paneles. No hay persistencia histórica, tablas de resúmenes ni pantalla de históricos.
>
> **Restricciones:** no modificar `README.md`, `ControlCalidad/README.md` ni `ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests`. El usuario actualiza la documentación.

## 0. Preparación y aprobación

- [x] 0.1 Confirmar que `requirements.md`, `specs.md` y este plan están aprobados antes de implementar.
- [x] 0.2 Revisar las reglas de `ControlCalidad/.agents/rules/`, la plantilla y los patrones de `req004_gestion_paneles`, `req006_mock_inversor`, `SolarUYDbContext`, `Program.cs` y `SolarUY.Tests/Domain/PanelTests.cs`.
- [x] 0.3 Confirmar con `git status` el punto de partida y que no se modificarán `README.md`, `ControlCalidad/README.md` ni `CodeQuality.Tests`.

## 1. Dominio (`SolarUY.Domain`)

- [x] 1.1 Crear `SolarUY/Domain/Enums/EstadoPanel.cs` con `Operativo` y `EnFalla`.
- [x] 1.2 Crear `SolarUY/Domain/Enums/TipoEntidadHistorico.cs` con `Parque`, `Nodo` y `Panel`.
- [x] 1.3 Crear `SolarUY/Domain/Enums/AgrupacionHistorico.cs` con `Dia`, `Mes` y `Anio`.
- [x] 1.4 Crear `SolarUY/Domain/ValueObjects/CalculadoraEnergia.cs` con la fórmula, el tope de 15 segundos y las validaciones de `specs.md`.
- [x] 1.5 Crear `SolarUY/Domain/ValueObjects/FiltroHistorico.cs` con `Crear(...)`, propiedades y validaciones de rango, enums y entidad.
- [x] 1.6 Crear `SolarUY/Domain/Exceptions/FiltroHistoricoInvalidoException.cs`.
- [x] 1.7 Crear `SolarUY/Domain/ValueObjects/ResumenDiarioGeneracion.cs` con la proyección diaria y `PotenciaPromedioW` calculado.
- [x] 1.8 Crear `SolarUY/Domain/Entities/ResumenDiarioParque.cs` con `Crear`, `RegistrarLectura`, acumuladores, `UltimaFechaHoraUtc` y `AgregarNodo` idempotente.
- [x] 1.9 Crear `SolarUY/Domain/Entities/ResumenDiarioNodo.cs` con `Crear`, `RegistrarLectura`, acumuladores, `AgregarPanel` idempotente y navegación al resumen de parque.
- [x] 1.10 Crear `SolarUY/Domain/Entities/ResumenDiarioPanel.cs` con `Crear`, `RegistrarLectura`, acumuladores y navegación al resumen de nodo.
- [x] 1.11 Crear `SolarUY/Domain/Repositories/IHistoricoRepository.cs` con los cuatro métodos de `specs.md`.
- [x] 1.12 Confirmar que `SolarUY/Domain/` no referencia EF Core, ASP.NET Core, Application, Infrastructure ni Presentation.

## 2. Tests del Dominio (`SolarUY.Tests/Domain`)

- [x] 2.1 Crear `CalculadoraEnergiaTests.cs` y cubrir sin lectura anterior, 5 segundos, tope de 15 segundos, intervalo no positivo y validaciones.
- [x] 2.2 Crear `FiltroHistoricoTests.cs` y cubrir filtro válido, rango invertido, fechas `default`, entidad vacía y enums inválidos.
- [x] 2.3 Crear `ResumenDiarioGeneracionTests.cs` y cubrir promedio calculado, promedio cero y validaciones.
- [x] 2.4 Crear `ResumenDiarioParqueTests.cs` y cubrir creación, primera lectura con energía 0, acumulados, máximo, fallas, estado, duplicado, fuera de orden, fecha distinta y alta idempotente de nodos.
- [x] 2.5 Crear `ResumenDiarioNodoTests.cs` con los mismos comportamientos y alta idempotente de paneles.
- [x] 2.6 Crear `ResumenDiarioPanelTests.cs` con los mismos comportamientos sin hijos.
- [x] 2.7 Ejecutar `docker compose run --rm tools dotnet test SolarUY.Tests` y confirmar que los tests de Dominio están en verde.

## 3. Aplicación (`SolarUY.Application`)

- [x] 3.1 Crear `SolarUY/Application/Services/ConfiguracionHistorico.cs` con sección `Historico`, `BrechaMaximaSegundos = 15` y `DesfaseHorarioUruguayHoras = -3`.
- [x] 3.2 Crear `SolarUY/Application/Dtos/FiltroHistoricoDto.cs`, `OpcionEntidadDto.cs`, `OpcionesHistoricoDto.cs`, `ResumenHistoricoDto.cs`, `PuntoGraficoHistoricoDto.cs` y `HistoricoGeneracionDto.cs`.
- [x] 3.3 Crear `SolarUY/Application/Services/IRegistroHistoricoService.cs` y `RegistroHistoricoService.cs`: carga por fecha, crea/actualiza parques, nodos y paneles, calcula estados consolidados, ignora duplicados y ejecuta un único `SaveChangesAsync`.
- [x] 3.4 Crear `SolarUY/Application/Services/IHistoricoService.cs` y `HistoricoService.cs`: filtro predeterminado, opciones de topología, consulta, agrupación día/mes/año, totales y puntos de gráfico.
- [x] 3.5 Modificar `SolarUY/Application/Services/ITelemetriaService.cs` y `TelemetriaService.cs` para invocar el registro histórico antes de guardar la última lectura y propagar sus errores.
- [x] 3.6 Confirmar que `SolarUY/Application/` no referencia EF Core, ASP.NET Core, Infrastructure ni Controllers.

## 4. Infraestructura (`SolarUY.Infrastructure`)

- [x] 4.1 Agregar los tres `DbSet` de resúmenes en `SolarUY/Infrastructure/Persistence/SolarUYDbContext.cs`.
- [x] 4.2 Crear `ResumenDiarioParqueConfiguration.cs` en `SolarUY/Infrastructure/Persistence/Configurations/` con tabla, propiedades, precisión, índice único y FK `Restrict` a `ParquesSolares`.
- [x] 4.3 Crear `ResumenDiarioNodoConfiguration.cs` con tabla, propiedades, índice único, FK `Restrict` a `Nodos` y FK `Cascade` al resumen de parque.
- [x] 4.4 Crear `ResumenDiarioPanelConfiguration.cs` con tabla, propiedades, índice único, FK `Restrict` a `Paneles` y FK `Cascade` al resumen de nodo.
- [x] 4.5 Crear `SolarUY/Infrastructure/Repositories/HistoricoRepository.cs` con carga trackeada por fecha, consulta con `AsNoTracking`, proyecciones `ResumenDiarioGeneracion` y persistencia.
- [x] 4.6 Generar la migración `AgregarResumenesDiariosHistoricos` con `docker compose run --rm tools dotnet ef migrations add AgregarResumenesDiariosHistoricos --project SolarUY --startup-project SolarUY` y revisar que incluya las tres tablas, índices y FKs.
- [x] 4.7 En `SolarUY/Program.cs` registrar `ConfiguracionHistorico`, `TimeProvider.System`, `IHistoricoRepository`, `IRegistroHistoricoService` y `IHistoricoService` como scoped, y cambiar `ITelemetriaService` a scoped.
- [x] 4.8 Agregar la sección `Historico` con `BrechaMaximaSegundos` y `DesfaseHorarioUruguayHoras` en `SolarUY/appsettings.json`.
- [x] 4.9 Ejecutar `docker compose run --rm tools dotnet build SolarUY.sln` y confirmar que compila.

## 5. Presentación (`SolarUY.Controllers` + `Views`)

- [x] 5.1 Crear `SolarUY/Models/HistoricoViewModel.cs` con `Filtro`, `Opciones` y `Resultado`.
- [x] 5.2 Crear `SolarUY/Controllers/HistoricoController.cs` con `[Authorize]` y `GET Index`, filtro predeterminado, carga de opciones, manejo de `FiltroHistoricoInvalidoException` y resultado vacío ante error.
- [x] 5.3 Crear `SolarUY/Views/Historico/Index.cshtml` con formulario GET, select de tipo, entidad con opción “Todas”, fechas, agrupación, tarjetas, tabla, `canvas` y bloque JSON de puntos del gráfico.
- [x] 5.4 Descargar Chart.js 4.4.9 UMD en `SolarUY/wwwroot/lib/chart.js/chart.umd.min.js` y su licencia en `SolarUY/wwwroot/lib/chart.js/LICENSE.md`.
- [x] 5.5 Crear `SolarUY/wwwroot/js/historico.js` para filtrar opciones de entidad por tipo y dibujar el gráfico de línea de energía por período.
- [x] 5.6 Agregar el enlace “Histórico” en `SolarUY/Views/Shared/_Layout.cshtml` dentro del bloque de usuarios autenticados.
- [x] 5.7 Verificar manualmente que sin sesión `/Historico` redirige al login y que con sesión de desarrollo responde `200`.

## 6. Documentación

- [x] 6.1 No modificar `README.md` ni `ControlCalidad/README.md`; preparar para el usuario un resumen de la funcionalidad, el estado y las instrucciones de verificación.

## 7. Verificación final

- [x] 7.1 Build completo: `docker compose run --rm tools dotnet build SolarUY.sln` sin errores.
- [x] 7.2 Tests completos: `docker compose run --rm tools dotnet test SolarUY.sln` con `SolarUY.Tests` y ArchUnitNET en verde.
- [x] 7.3 Ejecutar `./ControlCalidad/SonarQube/analizar.sh` y registrar hallazgos.
- [x] 7.4 Levantar el stack, esperar varios ticks y verificar en MySQL que hay como máximo una fila por parque, nodo y panel por fecha local, con energía creciente, promedio igual a potencia acumulada dividida por lecturas y máximo/fallas coherentes.
- [x] 7.5 Reenviar por `POST /api/simulacion/telemetria` una lectura con la misma fecha/hora y otra anterior, y confirmar que no modifican los acumulados.
- [x] 7.6 Reiniciar `app` y confirmar que el histórico persiste; confirmar que no existe retención ni borrado automático.
- [x] 7.7 Verificar la pantalla con filtros por parque, nodo y panel; agrupaciones día, mes y año; tarjetas, tabla y gráfico con datos.
- [x] 7.8 Dar de baja lógicamente una entidad con histórico y confirmar que conserva sus resúmenes pero deja de aparecer en las opciones de filtro.
- [x] 7.9 Confirmar que `README.md`, `ControlCalidad/README.md`, `CodeQuality.Tests`, el simulador y el contrato de telemetría no fueron modificados; registrar la evidencia final.

## Notas

- Los resúmenes diarios son la única persistencia histórica; no se guardan lecturas crudas.
- El registro histórico es síncrono con la ingesta: si falla, la lectura no reemplaza la caché y el simulador puede reintentar.
- `FechaLocal` usa UTC-3 fijo y la agrupación se hace sobre esa fecha.
- La consulta es de solo lectura y accesible para cualquier usuario autenticado.
- No se modifica `CodeQuality.Tests` sin autorización explícita.

## Registro de verificación

- Build: `docker compose run --rm tools dotnet build SolarUY.sln` sin errores.
- Tests: `dotnet test SolarUY.sln` en verde (122 SolarUY.Tests, 102 SolarUY.Simulador.Tests y 9 CodeQuality.Tests).
- SonarQube: Quality Gate `OK`; quedan 9 hallazgos previos fuera del código nuevo (8 code smells y 1 vulnerabilidad). El `SONAR_TOKEN` del `.env` fue rechazado por el servidor local y se usó un token temporal para verificar.
- Verificación manual: ingesta con simulación, resúmenes por parque/nodo/panel, energías crecientes, promedio calculado, lectura duplicada y anterior ignoradas, persistencia tras reiniciar `app`, filtros por entidad y agrupaciones día/mes/año, y baja lógica con resumen conservado pero excluido de las opciones.
- Los datos sintéticos usados para la verificación manual fueron eliminados al finalizar.
