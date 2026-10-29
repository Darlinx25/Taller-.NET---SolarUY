# Tareas: req006 Mock del inversor (simulación de telemetría y sensores ARM)

> Cada tarea debe ser verificable, indicar el archivo o la ruta cuando aplique y comenzar con `[ ]`. Marcar `[x]` solo cuando esté terminada y verificada.
>
> **Punto de partida:** el repositorio no contiene código de telemetría (ni Dominio, ni DTOs, ni servicios, ni controladores, ni proyecto del simulador, ni tests). Todas las tareas están pendientes.

## 0. Preparación y aprobación

- [ ] 0.1 Confirmar que `requirements.md`, `specs.md` y este plan están aprobados por una persona antes de implementar.
- [ ] 0.2 Revisar las reglas de `ControlCalidad/.agents/rules/` y los patrones de `req003_nodos_inversores`, `req004_gestion_paneles` y `req005_registro_seguimiento_tareas_mantenimiento` (DTOs, servicios, controladores, `Program.cs`).
- [ ] 0.3 Confirmar el punto de partida: `git grep -il telemetria -- SolarUY SolarUY.Tests` no devuelve resultados y la solución compila con `docker compose run --rm tools dotnet build SolarUY.sln`.
- [ ] 0.4 Confirmar que el alcance sigue siendo una `app` y un `simulador`, sin balanceador, réplicas, Redis, Data Protection compartida, persistencia ni migraciones.

## 1. Dominio (`SolarUY.Domain`)

- [ ] 1.1 Crear `SolarUY/Domain/Telemetria/EstadoPanelSimulado.cs` con `Operativo` y `EnFalla`.
- [ ] 1.2 Crear `SolarUY/Domain/Telemetria/GeneradorGeneracionSolar.cs` con la ventana 07:00–19:00, `CalcularFactorSolar` (hora fraccionaria, pico a las 13:00) y `CalcularPotenciaGenerada` (variación, falla, redondeo a 2 decimales y acotado a `PotenciaWp` después de redondear).
- [ ] 1.3 Crear `SolarUY/Domain/Telemetria/PanelSimulado.cs` con el estado de falla (`ActualizarEstado` con ramas excluyentes: recuperación o falla) y `GenerarLectura`.
- [ ] 1.4 Crear `SolarUY/Domain/Telemetria/LecturaPanelSimulado.cs` con validaciones y `EstaOperativo`.
- [ ] 1.5 Crear `SolarUY/Domain/Telemetria/LecturaNodoSimulado.cs` con la suma de paneles, el límite del inversor (potencia máxima `>= 0`) y la energía acumulada sin redondear.
- [ ] 1.6 Crear `SolarUY/Domain/Telemetria/LecturaParqueSolarSimulado.cs` con la consolidación por parque.
- [ ] 1.7 Confirmar que `SolarUY/Domain/` no referencia EF Core, ASP.NET Core, SignalR, `Application`, `Infrastructure` ni `Controllers`.

## 2. Tests del Dominio (`SolarUY.Tests/Domain`)

- [ ] 2.1 Crear `SolarUY.Tests/Domain/GeneradorGeneracionSolarTests.cs`: factor 0 fuera de la ventana (06:59:59, 07:00, 19:00), factor 1 a las 13:00, simetría, hora fraccionaria, factor en [0, 1] todo el día, potencia 0 con `EnFalla`, variación acotada a `PotenciaWp`, redondeo y validaciones.
- [ ] 2.2 Crear `SolarUY.Tests/Domain/PanelSimuladoTests.cs`: creación y validaciones, transición a `EnFalla`, N lecturas en falla y recuperación en la N+1, sin nueva falla en la lectura en que se recupera, parámetros fuera de rango y `GenerarLectura`.
- [ ] 2.3 Crear `SolarUY.Tests/Domain/LecturaPanelSimuladoTests.cs`: creación, trim, validaciones y `EstaOperativo`.
- [ ] 2.4 Crear `SolarUY.Tests/Domain/LecturaNodoSimuladoTests.cs`: suma de paneles, lista vacía, límite del inversor (con valor, `null` y 0 kW), energía acumulada sin redondear (300 W durante 5 s crece en cada tick) y validaciones.
- [ ] 2.5 Crear `SolarUY.Tests/Domain/LecturaParqueSolarSimuladoTests.cs`: consolidación de potencia y energía, lista vacía y validaciones.
- [ ] 2.6 Ejecutar `docker compose run --rm tools dotnet test SolarUY.Tests` y confirmar que los tests nuevos y los existentes están en verde.

## 3. Aplicación (`SolarUY.Application`)

- [ ] 3.1 Crear en `SolarUY/Application/Dtos/` los DTOs de telemetría `TelemetriaDto.cs`, `TelemetriaParqueSolarDto.cs`, `TelemetriaNodoDto.cs` y `TelemetriaPanelDto.cs` con las propiedades de `specs.md` (sin DataAnnotations).
- [ ] 3.2 Crear en `SolarUY/Application/Dtos/` los DTOs de topología `TopologiaSimulacionDto.cs`, `ParqueSimulacionDto.cs`, `NodoSimulacionDto.cs` y `PanelSimulacionDto.cs`.
- [ ] 3.3 Crear `SolarUY/Application/Seguridad/AutenticacionComponentes.cs` con los nombres del esquema `ApiKey` y del encabezado `X-Api-Key`, para que la Presentación no dependa de Infraestructura.
- [ ] 3.4 Crear `SolarUY/Application/Services/IAlmacenTelemetria.cs` con `Task<TelemetriaDto?> ObtenerAsync(CancellationToken)` y `Task GuardarAsync(TelemetriaDto, CancellationToken)`.
- [ ] 3.5 Crear `SolarUY/Application/Services/ITelemetriaService.cs` (con el record `ResultadoIngestaTelemetria`) y `TelemetriaService.cs`: lee del almacén asíncrono, valida la ingesta, no guarda si es inválida, reemplaza la lectura si es válida y registra en el log la ingesta y los rechazos.
- [ ] 3.6 Crear `SolarUY/Application/Services/ValidadorTelemetria.cs` con las reglas de `specs.md` (IDs, textos, longitudes, potencias, energía, estados exactos y fechas) y mensajes con la ruta del dato.
- [ ] 3.7 Crear `SolarUY/Application/Services/ITopologiaSimulacionService.cs` y `TopologiaSimulacionService.cs`: leen parques, nodos (con inversor) y paneles con los repositorios existentes y mapean a los DTOs de topología, sin filtrar por estado y sin escribir en la base.
- [ ] 3.8 Crear `SolarUY/Application/Services/IValidadorApiKey.cs` con `bool EsValida(string? apiKey)`.
- [ ] 3.9 Confirmar que `SolarUY/Application/` no referencia EF Core, ASP.NET Core, SignalR, `Infrastructure` ni `Controllers`.

## 4. Infraestructura (`SolarUY.Infrastructure`)

- [ ] 4.1 Crear `SolarUY/Infrastructure/Telemetria/AlmacenTelemetriaCache.cs` como singleton sobre `IDistributedCache`, con serialización JSON web y clave/vencimiento configurables.
- [ ] 4.2 Crear `SolarUY/Infrastructure/Telemetria/ConfiguracionAlmacenTelemetria.cs` con `Seccion = "TelemetriaAlmacen"`, `Clave = "simulacion:ultima-telemetria"` y `VigenciaHoras = 24`; validar los valores en el constructor del almacén.
- [ ] 4.3 Crear `SolarUY/Infrastructure/Telemetria/ConfiguracionApiSimulacion.cs` con `Seccion = "SimulacionApi"` y `ApiKey`.
- [ ] 4.4 Crear `SolarUY/Infrastructure/Telemetria/ValidadorApiKey.cs` con comparación de tiempo fijo (`CryptographicOperations.FixedTimeEquals`) y sin registrar la key.
- [ ] 4.5 Crear `SolarUY/Infrastructure/Telemetria/ApiKeyAuthenticationHandler.cs` usando `AutenticacionComponentes`, con fallo sin redirección (`401`).
- [ ] 4.6 En `SolarUY/Program.cs`: agregar `AddDistributedMemoryCache()`, las dos `Configure<...>`, el registro de `AlmacenTelemetriaCache`, `ValidadorApiKey`, `ValidadorTelemetria`, `TelemetriaService` y `TopologiaSimulacionService`, encadenar `AddScheme` del esquema `ApiKey` a la autenticación existente y registrar la advertencia cuando `SimulacionApi:ApiKey` esté vacía.
- [ ] 4.7 En `SolarUY/appsettings.json`: agregar `SimulacionApi:ApiKey` vacía y `TelemetriaAlmacen` con clave y vigencia.
- [ ] 4.8 Confirmar que no se agregó ningún `DbSet`, configuración Fluent API, propiedad de navegación ni archivo en `SolarUY/Migrations/` (no hay migración en este requerimiento).
- [ ] 4.9 Ejecutar `docker compose run --rm tools dotnet build SolarUY.sln` y confirmar que compila.

## 5. Presentación (`SolarUY.Controllers`)

- [ ] 5.1 Crear `SolarUY/Controllers/TelemetriaController.cs` en `api/telemetria` con `GET` asíncrono, cookie y `[Authorize(Roles = "Administrador")]`.
- [ ] 5.2 Crear `SolarUY/Controllers/SimulacionController.cs` en `api/simulacion`, protegido a nivel de clase con el esquema `ApiKey`.
- [ ] 5.3 `GET /api/simulacion/topologia`: `200 OK` con `TopologiaSimulacionDto`, incluso con `{ "parques": [] }`.
- [ ] 5.4 `POST /api/simulacion/telemetria`: `400 Bad Request` con `{ "errores": [...] }` si el payload es inválido y `204 No Content` si se almacena.
- [ ] 5.5 `GET /api/telemetria`: `204 No Content` antes de la primera lectura y `200 OK` con el `TelemetriaDto` después.
- [ ] 5.6 Confirmar que los controladores solo dependen de `ITelemetriaService` y `ITopologiaSimulacionService`, no acceden a repositorios ni a `DbContext` y no conocen al simulador.
- [ ] 5.7 Confirmar que no se crean vistas Razor ni hubs de SignalR.
- [ ] 5.8 Verificar manualmente con la `app` levantada: `GET /api/simulacion/topologia` sin key → `401` y con key → `200`; `POST /api/simulacion/telemetria` sin key → `401`; `POST` con payload inválido → `400`; `POST` válido → `204`; `GET /api/telemetria` sin cookie → `302` a `/dev/login`; con sesión de desarrollo → `204` antes de la primera lectura y `200` después.

## 6. Proyecto simulador (`SolarUY.Simulador`)

- [ ] 6.1 Crear `SolarUY.Simulador/SolarUY.Simulador.csproj` (`Microsoft.NET.Sdk.Worker`, `net10.0`) con los paquetes `Microsoft.Extensions.Hosting` y `Microsoft.Extensions.Http` y los `Compile Include` por enlace de `Domain`, DTOs de telemetría y topología y `AutenticacionComponentes`, **sin `ProjectReference`**; agregarlo a `SolarUY.sln`.
- [ ] 6.2 Crear `SolarUY.Simulador/Simulacion/ConfiguracionSimulador.cs` con sección `Simulador`, los parámetros de simulación y `IntentosEnvio`/`EsperaEntreIntentosSegundos`.
- [ ] 6.3 Crear `SolarUY.Simulador/Transporte/ConfiguracionSimulacionApi.cs` con `UrlBase`, `ApiKey` y `TimeoutSegundos`.
- [ ] 6.4 Crear `SolarUY.Simulador/Transporte/ClienteTopologia.cs` con `GET api/simulacion/topologia`, `X-Api-Key`, deserialización JSON web y `null` ante cualquier fallo (HTTP no exitoso, transporte, timeout o JSON inválido).
- [ ] 6.5 Crear `SolarUY.Simulador/Transporte/ClienteTelemetria.cs` con `POST api/simulacion/telemetria`, `X-Api-Key`, JSON y `true` solo para `2xx`.
- [ ] 6.6 Crear `SolarUY.Simulador/Transporte/TelemetriaMapper.cs` con `Mapear(IReadOnlyList<LecturaParqueSolarSimulado>)`: lista vacía → `FechaHora = null` y `Parques` vacía; estado como texto; energía redondeada a 4 decimales solo en el DTO.
- [ ] 6.7 Crear `SolarUY.Simulador/Simulacion/SimuladorInversor.cs` (singleton con `TimeProvider`): depende de `ClienteTopologia`, filtra nodos `Activo`, devuelve `null` si la topología no está disponible, conserva el estado de fallas y energía y reemplaza el `PanelSimulado` cuyo `CodigoSerie` o `PotenciaWp` cambió.
- [ ] 6.8 Crear `SolarUY.Simulador/Simulacion/SimuladorTelemetriaHostedService.cs` con tick inmediato, `PeriodicTimer`, omisión del tick sin topología, reintentos y logs de lectura generada, éxito y error sin detener el proceso.
- [ ] 6.9 Crear `SolarUY.Simulador/Program.cs` con `Host.CreateApplicationBuilder`, opciones validadas con `ValidateOnStart`, dos `HttpClient` tipados con `PooledConnectionLifetime`, `TimeProvider.System`, simulador y hosted service; sin `DbContext`, cadena de conexión ni repositorios.
- [ ] 6.10 Crear `SolarUY.Simulador/appsettings.json` con `Simulador` y `SimulacionApi` (`UrlBase`, `ApiKey` vacía, `TimeoutSegundos`); sin `ConnectionStrings`.
- [ ] 6.11 Confirmar que el simulador no levanta Kestrel, no publica puertos, no usa controladores, no accede a MySQL y no escribe en la base de datos.
- [ ] 6.12 Ejecutar `dotnet publish` del simulador y confirmar que la salida tiene solo su propio `appsettings.json` y ninguna DLL de EF Core, MySQL ni ASP.NET Core ni el `appsettings.Development.json` del monolito.

## 7. Docker y configuración

- [ ] 7.1 Crear `docker/Dockerfile.simulador` con build del worker (copiando solo `SolarUY/Domain`, `SolarUY/Application` y `SolarUY.Simulador`) y runtime `runtime:10.0`.
- [ ] 7.2 Agregar el servicio `simulador` a `docker-compose.yml` con `depends_on` solo de `app` healthy; sin puertos publicados y sin variables de MySQL.
- [ ] 7.3 Pasar `SimulacionApi__ApiKey` a `app` y `SimulacionApi__UrlBase`/`SimulacionApi__ApiKey` a `simulador` desde `${SIMULACION_API_KEY}`.
- [ ] 7.4 Agregar `SIMULACION_API_KEY` a `.env.example` y al `.env` local (sin commitear).
- [ ] 7.5 Actualizar el comentario de cabecera de `docker-compose.yml` para incluir al simulador.
- [ ] 7.6 Ejecutar `docker compose up -d --build` y confirmar `mysql`/`app` healthy y `simulador` corriendo.
- [ ] 7.7 Confirmar que `docker compose logs simulador` muestra un tick cada 5 segundos, la lectura generada y envíos exitosos, y que `docker compose logs app` muestra la ingesta.
- [ ] 7.8 Confirmar con `docker compose config` que `simulador` no recibe `ConnectionStrings__SolarUYDb` ni variables de MySQL.

## 8. Documentación

- [ ] 8.1 Actualizar `README.md`: el mock es el componente `simulador` (obtiene la topología por `GET /api/simulacion/topologia`, envía telemetría por `POST /api/simulacion/telemetria`, no corre dentro del monolito ni accede a MySQL); pasar "Mock Inversor" a Listo; actualizar la descripción del nivel "Simulación" de la arquitectura y el texto de `docker compose up` (MySQL + migraciones + app + simulador); mencionar la evolución a balanceador sin dibujarlo.
- [ ] 8.2 Actualizar `README.md` con la verificación del mock: `docker compose logs -f simulador`, login de desarrollo y `GET /api/telemetria`.
- [ ] 8.3 Actualizar `DESARROLLO.md`: servicio `simulador` en la lista de servicios, variable `SIMULACION_API_KEY` (quien ya tenga `.env` debe agregarla) y verificación del mock.
- [ ] 8.4 Agregar `req006_mock_inversor` a la tabla de `ControlCalidad/README.md` con el estado correspondiente (Planificado / En progreso / Implementado) y describir el componente separado.
- [ ] 8.5 Registrar en este archivo las evidencias de la verificación final (sección 9).

## 9. Verificación final

- [ ] 9.1 Build completo: `docker compose run --rm tools dotnet build SolarUY.sln` sin errores.
- [ ] 9.2 Tests completos: `docker compose run --rm tools dotnet test SolarUY.sln` con los tests de Dominio y ArchUnitNET en verde.
- [ ] 9.3 Ejecutar SonarQube con `./ControlCalidad/SonarQube/analizar.ps1` (Windows) o `analizar.sh` (Linux/macOS) y registrar hallazgos. **(requiere `SONAR_TOKEN` en `.env`)**
- [ ] 9.4 Prueba manual completa:
  - `docker compose up -d --build`;
  - `docker compose ps` y `docker compose logs simulador`;
  - `GET /api/simulacion/topologia` sin key → `401`; con `X-Api-Key` → `200`;
  - `GET /api/telemetria` sin cookie → `302` a `/dev/login`;
  - login en `http://localhost:5080/dev/login` y `GET /api/telemetria` → `200` con `fechaHora` que cambia cada 5 segundos;
  - potencia del parque = suma de nodos activos y potencia del nodo = suma de paneles;
  - nodo `EnMantenimiento` excluido de la lectura;
  - energía acumulada creciente entre lecturas;
  - límite del inversor respetado;
  - cambio de `PotenciaWp` de un panel reflejado en la siguiente lectura;
  - falla y recuperación de paneles con `Simulador__ProbabilidadFalla=1` y `Simulador__IntervaloSegundos=2` en un contenedor efímero (6 lecturas `EnFalla` y 1 `Operativo`);
  - `POST` sin `X-Api-Key` → `401` y la lectura anterior no cambia;
  - payload inválido → `400` y la lectura anterior no cambia;
  - `docker stop` de `app`: el simulador sigue vivo y registra fallos de topología y envío; `docker start` de `app`: se reanuda;
  - `docker restart simulador`: la energía vuelve a cero;
  - `docker restart app`: `GET` responde `204` hasta el siguiente tick (caché en memoria);
  - `docker compose config` confirma que `simulador` no tiene cadena de conexión.
- [ ] 9.5 Confirmar que no se modificó `ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests`, que no se generó migración y que `git status` solo muestra los archivos previstos.

## Notas

- La telemetría es efímera: no hay persistencia, `DbSet`, configuración Fluent API ni migración.
- El simulador es un worker de consola; no expone endpoints ni puertos, no accede a MySQL y no referencia el proyecto `SolarUY`: comparte por enlace de fuentes solo Dominio, DTOs y `AutenticacionComponentes`.
- La aplicación es la única que accede a MySQL y la única que conoce la topología registrada.
- La última lectura vive detrás de `IAlmacenTelemetria` asíncrono sobre `IDistributedCache`; hoy memoria, mañana Redis sin cambiar controladores ni simulador.
- La API key autentica al simulador como componente interno; no es un mecanismo de login de usuarios.
- No se implementan balanceador de carga ni réplicas en esta iteración, pero el diseño queda preparado para agregarlos.
- No se envían comandos al inversor ni se integra hardware real.
- No se crean tests unitarios en Application, Infrastructure, Presentation ni en el simulador; solo tests xUnit del Dominio.
- No se modifica `CodeQuality.Tests` sin autorización explícita.
