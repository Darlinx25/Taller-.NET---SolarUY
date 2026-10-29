# Tareas: req006 Mock del inversor (simulación de telemetría y sensores ARM)

> Cada tarea debe ser verificable, indicar el archivo o la ruta cuando aplique y comenzar con `[ ]`. Marcar `[x]` solo cuando esté terminada y verificada.
>
> **Estado de esta revisión:** el mock se extrae a un componente separado, obtiene la topología por un endpoint interno de la aplicación y envía telemetría por otro endpoint interno. La aplicación es la única que accede a MySQL y conserva la última lectura en `IDistributedCache` a través de una interfaz asíncrona. Las tareas de Dominio y sus tests ya están implementadas y se conservan; las tareas de la aplicación, el simulador, Docker, documentación y verificación se rehacen.

## 0. Preparación y aprobación

- [ ] 0.1 Confirmar que `requirements.md`, `specs.md` y este plan corregidos al diseño final están aprobados por una persona antes de implementar.
- [x] 0.2 Eliminar la carpeta `ControlCalidad/prompting/req007_mock_componente_separado/`, rechazada por el usuario: la extracción del mock forma parte de req006.
- [x] 0.3 Relevar la implementación anterior: Dominio y tests de telemetría se reutilizan; el hosted service y el simulador interno se trasladan al proyecto nuevo.
- [x] 0.4 Reemplazar el diseño de "simulador leyendo MySQL" por el diseño final: topología por endpoint, ingesta interna y `IAlmacenTelemetria` asíncrono sobre `IDistributedCache`.
- [ ] 0.5 Revisar las reglas de `ControlCalidad/.agents/rules/` y el flujo SDD antes de tocar código.
- [ ] 0.6 Confirmar que la solución compila antes de empezar: `docker compose run --rm tools dotnet build SolarUY.sln`.
- [ ] 0.7 Confirmar que el alcance sigue siendo una `app` y un `simulador`, sin balanceador, réplicas, Redis ni Data Protection compartida.

## 1. Dominio (`SolarUY.Domain`) — existente, sin cambios

- [x] 1.1 `SolarUY/Domain/Telemetria/EstadoPanelSimulado.cs` con `Operativo` y `EnFalla`.
- [x] 1.2 `SolarUY/Domain/Telemetria/GeneradorGeneracionSolar.cs` con la curva 07:00–19:00, la variación y los límites de potencia.
- [x] 1.3 `SolarUY/Domain/Telemetria/PanelSimulado.cs` con el estado de falla y `GenerarLectura`.
- [x] 1.4 `SolarUY/Domain/Telemetria/LecturaPanelSimulado.cs` con validaciones y `EstaOperativo`.
- [x] 1.5 `SolarUY/Domain/Telemetria/LecturaNodoSimulado.cs` con la suma de paneles, el límite del inversor y la energía acumulada.
- [x] 1.6 `SolarUY/Domain/Telemetria/LecturaParqueSolarSimulado.cs` con la consolidación por parque.
- [x] 1.7 Confirmar que `SolarUY/Domain/` no referencia EF Core, ASP.NET Core, SignalR, `Application`, `Infrastructure` ni `Controllers`.

## 2. Tests del Dominio (`SolarUY.Tests/Domain`) — existentes, se conservan

- [x] 2.1 `GeneradorGeneracionSolarTests.cs` con curva solar, falla, variaciones y validaciones.
- [x] 2.2 `PanelSimuladoTests.cs` con transición a `EnFalla`, recuperación y validaciones.
- [x] 2.3 `LecturaPanelSimuladoTests.cs`.
- [x] 2.4 `LecturaNodoSimuladoTests.cs` con suma, energía, límite del inversor y lista vacía.
- [x] 2.5 `LecturaParqueSolarSimuladoTests.cs` con consolidación y lista vacía.
- [x] 2.6 Tests de Dominio en verde con `docker compose run --rm tools dotnet test SolarUY.Tests`.

## 3. Aplicación (`SolarUY.Application`)

- [ ] 3.1 Eliminar `SolarUY/Application/Services/ISimuladorInversor.cs`.
- [ ] 3.2 Eliminar `SolarUY/Application/Services/ISimulacionTelemetriaService.cs` y `SimulacionTelemetriaService.cs`.
- [ ] 3.3 Conservar los DTOs `TelemetriaDto`, `TelemetriaParqueSolarDto`, `TelemetriaNodoDto` y `TelemetriaPanelDto` sin cambios de contrato.
- [ ] 3.4 Crear `SolarUY/Application/Dtos/Simulacion/TopologiaSimulacionDto.cs`, `ParqueSimulacionDto.cs`, `NodoSimulacionDto.cs` y `PanelSimulacionDto.cs` con las propiedades de `specs.md`.
- [ ] 3.5 Crear `SolarUY/Application/Services/TelemetriaMapper.cs` con `Mapear(IReadOnlyList<LecturaParqueSolarSimulado>)`, lista vacía → `FechaHora = null` y `Parques` vacía.
- [ ] 3.6 Crear `SolarUY/Application/Services/IAlmacenTelemetria.cs` con `Task<TelemetriaDto?> ObtenerAsync(CancellationToken)` y `Task GuardarAsync(TelemetriaDto, CancellationToken)`.
- [ ] 3.7 Crear `SolarUY/Application/Services/ITelemetriaService.cs` con `ObtenerUltimaLecturaAsync` y `RegistrarLecturaAsync`, y el record `ResultadoIngestaTelemetria`.
- [ ] 3.8 Crear `SolarUY/Application/Services/TelemetriaService.cs`: lee del almacén asíncrono, valida la ingesta, no guarda si es inválida y reemplaza la lectura si es válida.
- [ ] 3.9 Crear `SolarUY/Application/Services/ValidadorTelemetria.cs` con las reglas de `specs.md` (IDs, textos, longitudes, potencias, energía, estados y fechas).
- [ ] 3.10 Crear `SolarUY/Application/Services/ITopologiaSimulacionService.cs` y `TopologiaSimulacionService.cs`: leen parques, nodos y paneles con los repositorios existentes y mapean a los DTOs de topología, sin filtrar por estado y sin escribir en la base.
- [ ] 3.11 Crear `SolarUY/Application/Services/IValidadorApiKey.cs` con `bool EsValida(string? apiKey)`.
- [ ] 3.12 Crear `SolarUY/Application/Seguridad/AutenticacionComponentes.cs` con los nombres del esquema `ApiKey` y del encabezado `X-Api-Key`, para que la Presentación no dependa de Infraestructura.
- [ ] 3.13 Confirmar que `SolarUY/Application/` no referencia EF Core, ASP.NET Core, SignalR, `Infrastructure` ni `Controllers`.

## 4. Infraestructura de la aplicación (`SolarUY.Infrastructure`)

- [ ] 4.1 Crear `SolarUY/Infrastructure/Telemetria/AlmacenTelemetriaCache.cs` como singleton sobre `IDistributedCache`, con serialización JSON web y clave/vencimiento configurables.
- [ ] 4.2 Crear `SolarUY/Infrastructure/Telemetria/ConfiguracionAlmacenTelemetria.cs` con `Seccion = "TelemetriaAlmacen"`, `Clave = "simulacion:ultima-telemetria"` y `VigenciaHoras = 24`; validar los valores en el constructor del almacén.
- [ ] 4.3 Crear `SolarUY/Infrastructure/Telemetria/ConfiguracionApiSimulacion.cs` con `Seccion = "SimulacionApi"` y `ApiKey`.
- [ ] 4.4 Crear `SolarUY/Infrastructure/Telemetria/ValidadorApiKey.cs` con comparación de tiempo fijo (`CryptographicOperations.FixedTimeEquals`) y sin registrar la key.
- [ ] 4.5 Crear `SolarUY/Infrastructure/Telemetria/ApiKeyAuthenticationHandler.cs` usando `AutenticacionComponentes`, con fallo sin redirección (`401`).
- [ ] 4.6 Eliminar `SolarUY/Infrastructure/Simulacion/ConfiguracionSimulacionInversor.cs`, `SimuladorInversor.cs` y `SimulacionTelemetriaHostedService.cs`, y la carpeta `Simulacion/`.
- [ ] 4.7 En `SolarUY/Program.cs`: quitar la configuración y los registros de simulación; agregar `AddDistributedMemoryCache()`, registrar `AlmacenTelemetriaCache`, `ValidadorApiKey`, `ValidadorTelemetria`, `TelemetriaService`, `TopologiaSimulacionService` y el esquema `ApiKey`.
- [ ] 4.8 En `SolarUY/appsettings.json`: eliminar `SimulacionInversor` y agregar `SimulacionApi:ApiKey` vacía y `TelemetriaAlmacen` con clave y vigencia.
- [ ] 4.9 Confirmar que no se agregó ningún `DbSet`, configuración Fluent API, propiedad de navegación ni archivo en `SolarUY/Migrations/`.
- [ ] 4.10 Ejecutar `docker compose run --rm tools dotnet build SolarUY.sln` y confirmar que compila.

## 5. Presentación (`SolarUY.Controllers`)

- [ ] 5.1 Reescribir `SolarUY/Controllers/TelemetriaController.cs` como solo `GET` asíncrono con cookie y rol `Administrador`.
- [ ] 5.2 Crear `SolarUY/Controllers/SimulacionController.cs` en `api/simulacion`, protegido a nivel de clase con el esquema `ApiKey`.
- [ ] 5.3 `GET /api/simulacion/topologia`: `200 OK` con `TopologiaSimulacionDto`, incluso con `{ "parques": [] }`.
- [ ] 5.4 `POST /api/simulacion/telemetria`: `400 Bad Request` con `{ "errores": [...] }` si el payload es inválido y `204 No Content` si se almacena.
- [ ] 5.5 `GET /api/telemetria`: `204 No Content` antes de la primera lectura y `200 OK` con el `TelemetriaDto` después.
- [ ] 5.6 Confirmar que los controladores solo dependen de `ITelemetriaService` y `ITopologiaSimulacionService`, no acceden a repositorios ni a `DbContext` y no conocen al simulador.
- [ ] 5.7 Confirmar que no se crean vistas Razor ni hubs de SignalR.
- [ ] 5.8 Verificar manualmente: `GET /api/simulacion/topologia` sin key → `401`; `POST /api/simulacion/telemetria` sin key → `401`; `GET /api/telemetria` sin cookie → `401`; `POST` con payload inválido → `400`; `POST` válido → `204`; `GET` con sesión → `200`.

## 6. Proyecto simulador (`SolarUY.Simulador`)

- [ ] 6.1 Crear `SolarUY.Simulador/SolarUY.Simulador.csproj` (`Microsoft.NET.Sdk.Worker`, `net10.0`) con referencia a `SolarUY.csproj` y agregarlo a `SolarUY.sln`.
- [ ] 6.2 Crear `SolarUY.Simulador/Simulacion/ConfiguracionSimulador.cs` con sección `Simulador`, los parámetros de simulación y `IntentosEnvio`/`EsperaEntreIntentosSegundos`.
- [ ] 6.3 Crear `SolarUY.Simulador/Simulacion/IRelojSimulador.cs` y `RelojSistema.cs`; inyectar el reloj en `SimuladorInversor` para tests deterministas.
- [ ] 6.4 Crear `SolarUY.Simulador/Transporte/ConfiguracionSimulacionApi.cs` con `UrlBase`, `ApiKey` y `TimeoutSegundos`, y validarla con `ValidateOnStart`.
- [ ] 6.5 Crear `SolarUY.Simulador/Transporte/ClienteTopologia.cs` con `GET /api/simulacion/topologia`, `X-Api-Key`, deserialización JSON web y `null` ante cualquier fallo.
- [ ] 6.6 Crear `SolarUY.Simulador/Transporte/ClienteTelemetria.cs` con `POST /api/simulacion/telemetria`, `X-Api-Key`, JSON, validación de configuración y `true` solo para `2xx`.
- [ ] 6.7 Mover `SimuladorInversor` a `SolarUY.Simulador/Simulacion/SimuladorInversor.cs`, quitarle el `IServiceScopeFactory` y los repositorios, hacerlo depender de `ClienteTopologia`, filtrar nodos `Activo`, devolver `null` si la topología no está disponible y conservar el estado de fallas y energía.
- [ ] 6.8 Crear `SolarUY.Simulador/Simulacion/SimuladorTelemetriaHostedService.cs` con tick inmediato, `PeriodicTimer`, omisión del tick sin topología, reintentos y logs de éxito/error sin detener el proceso.
- [ ] 6.9 Crear `SolarUY.Simulador/Program.cs` con `Host.CreateApplicationBuilder`, configuración, dos `HttpClient` tipados, reloj, simulador y hosted service; sin `DbContext`, cadena de conexión ni repositorios.
- [ ] 6.10 Crear `SolarUY.Simulador/appsettings.json` con `Simulador` y `SimulacionApi` (`UrlBase`, `ApiKey`, `TimeoutSegundos`); sin `ConnectionStrings`.
- [ ] 6.11 Confirmar que el simulador no levanta Kestrel, no publica puertos, no usa controladores, no accede a MySQL y no escribe en la base de datos.

## 7. Docker y configuración

- [ ] 7.1 Crear `docker/Dockerfile.simulador` con build del worker y runtime `aspnet:10.0`.
- [ ] 7.2 Agregar el servicio `simulador` a `docker-compose.yml` con `depends_on` solo de `app` healthy; sin puertos publicados y sin variables de MySQL.
- [ ] 7.3 Pasar `SimulacionApi__ApiKey` a `app` y `SimulacionApi__UrlBase`/`SimulacionApi__ApiKey` a `simulador` desde `${SIMULACION_API_KEY}`.
- [ ] 7.4 Agregar `SIMULACION_API_KEY` a `.env.example` y al `.env` local (sin commitear).
- [ ] 7.5 Actualizar el comentario de cabecera de `docker-compose.yml` para incluir al simulador.
- [ ] 7.6 Ejecutar `docker compose up -d --build` y confirmar `mysql`/`app` healthy y `simulador` corriendo.
- [ ] 7.7 Confirmar que `docker compose logs simulador` muestra un tick cada 5 segundos, la topología obtenida y envíos exitosos.
- [ ] 7.8 Confirmar con `docker compose config` que `simulador` no recibe `ConnectionStrings__SolarUYDb` ni variables de MySQL.

## 8. Tests nuevos

- [ ] 8.1 Crear `SolarUY.Tests/Application/ValidadorTelemetriaTests.cs` con los casos de `specs.md`.
- [ ] 8.2 Crear `SolarUY.Tests/Application/TelemetriaMapperTests.cs` con lista vacía, lista completa y `null`.
- [ ] 8.3 Crear `SolarUY.Tests/Application/TopologiaSimulacionServiceTests.cs` con fakes de repositorios, topología completa, parque sin nodos y nodo sin paneles.
- [ ] 8.4 Crear `SolarUY.Tests/Infrastructure/AlmacenTelemetriaCacheTests.cs` con `MemoryDistributedCache`: inicio en `null`, guardado, reemplazo, clave configurada y `GuardarAsync(null)`.
- [ ] 8.5 Crear `SolarUY.Tests/Infrastructure/ValidadorApiKeyTests.cs` con key correcta, incorrecta, vacía y longitudes distintas.
- [ ] 8.6 Crear `SolarUY.Tests/Simulador/ClienteTopologiaTests.cs` con un `HttpMessageHandler` falso: ruta, encabezado, deserialización, `401`/`500` y excepción de transporte.
- [ ] 8.7 Crear `SolarUY.Tests/Simulador/ClienteTelemetriaTests.cs` con un `HttpMessageHandler` falso: ruta, encabezado, `204`, `401` y excepción de transporte.
- [ ] 8.8 Crear `SolarUY.Tests/Simulador/SimuladorInversorTests.cs` con reloj fijo: exclusión de nodos no activos, energía creciente, parque sin nodos activos, topología vacía y fallo de topología.
- [ ] 8.9 Agregar referencia de `SolarUY.Tests` a `SolarUY.Simulador`.
- [ ] 8.10 Ejecutar `docker compose run --rm tools dotnet test SolarUY.sln` y confirmar todos los tests y ArchUnitNET en verde.

## 9. Documentación

- [ ] 9.1 Actualizar `README.md`: el mock es el componente `simulador`, obtiene la topología por `GET /api/simulacion/topologia`, envía telemetría por `POST /api/simulacion/telemetria` y no corre dentro del monolito ni accede a MySQL.
- [ ] 9.2 Actualizar la sección "Verificación del mock del inversor" de `README.md` con `docker compose logs -f simulador`, el login de desarrollo y `GET /api/telemetria`.
- [ ] 9.3 Actualizar `ControlCalidad/README.md` con el estado de `req006_mock_inversor` y la arquitectura de componentes.
- [ ] 9.4 Actualizar el diagrama o el texto de arquitectura del `README.md` para mostrar el nivel "Simulación" como contenedor separado; no dibujar balanceador ni réplicas porque están fuera de alcance, pero dejar mencionada la evolución.
- [ ] 9.5 Registrar en este archivo las evidencias de la verificación final.

## 10. Verificación final

- [ ] 10.1 Build completo: `docker compose run --rm tools dotnet build SolarUY.sln` sin errores.
- [ ] 10.2 Tests completos: `docker compose run --rm tools dotnet test SolarUY.sln` con Dominio, Aplicación, Infraestructura, Simulador y ArchUnitNET en verde.
- [ ] 10.3 Ejecutar SonarQube con `./ControlCalidad/SonarQube/analizar.ps1` (Windows) o `analizar.sh` (Linux/macOS) y registrar hallazgos. **(pendiente: requiere `SONAR_TOKEN` en `.env`)**
- [ ] 10.4 Prueba manual completa:
  - `docker compose up -d --build`;
  - `docker compose ps` y `docker compose logs simulador`;
  - `GET /api/simulacion/topologia` sin key → `401`; con `X-Api-Key` → `200`;
  - `GET /api/telemetria` sin cookie → `401`;
  - login en `http://localhost:5080/dev/login` y `GET` → `200` con `fechaHora` que cambia cada 5 segundos;
  - potencia del parque = suma de nodos activos y potencia del nodo = suma de paneles;
  - nodo `EnMantenimiento` excluido de la lectura;
  - energía acumulada creciente entre lecturas;
  - límite del inversor respetado;
  - falla y recuperación de paneles con `Simulador__ProbabilidadFalla=1` y `Simulador__IntervaloSegundos=2` en un contenedor efímero;
  - `POST` sin `X-Api-Key` → `401` y la lectura anterior no cambia;
  - payload inválido → `400` y la lectura anterior no cambia;
  - `docker stop app`: el simulador sigue vivo y registra fallos de topología y envío; `docker start app`: se reanuda;
  - `docker restart simulador`: la energía vuelve a cero;
  - `docker restart app`: `GET` responde `204` hasta el siguiente tick (caché en memoria);
  - `docker compose config` confirma que `simulador` no tiene cadena de conexión.
- [ ] 10.5 Confirmar que no se modificó `ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests`, que no se generó migración y que `git status` solo muestra los archivos previstos.

## Notas

- La telemetría es efímera: no hay persistencia, `DbSet`, configuración Fluent API ni migración.
- El simulador es un worker de consola; no expone endpoints ni puertos y no accede a MySQL.
- La aplicación es la única que accede a MySQL y la única que conoce la topología registrada.
- La última lectura vive detrás de `IAlmacenTelemetria` asíncrono sobre `IDistributedCache`; hoy memoria, mañana Redis sin cambiar controladores ni simulador.
- La API key autentica al simulador como componente interno; no es un mecanismo de login de usuarios.
- No se implementan balanceador de carga ni réplicas en esta iteración, pero el diseño queda preparado para agregarlos.
- No se envían comandos al inversor ni se integra hardware real.
- No se modifica `CodeQuality.Tests` sin autorización explícita.

## Historial de la versión anterior (referencia)

La primera implementación de req006 resolvió el mock como un `BackgroundService` dentro del monolito. Esa versión quedó funcional y verificada:

- Build y tests: 163 tests de `SolarUY.Tests` y 9 de ArchUnitNET en verde.
- `GET /api/telemetria`: `401` sin cookie y `200` con login de desarrollo.
- Tick cada 5 segundos, sumas de potencia coherentes y energía acumulada creciente.
- Límite del inversor, falla/recuperación, exclusión de nodos no activos y generación diurna verificados.
- Sin migraciones ni cambios en `CodeQuality.Tests`.

Esa implementación no cumplía la arquitectura de referencia: el mock no era un componente separado y no enviaba telemetría. Esta revisión reemplaza el hosted service interno por el proyecto `SolarUY.Simulador`, que obtiene la topología por endpoint y envía telemetría por endpoint, conservando el Dominio, los tests y el contrato de consulta.
