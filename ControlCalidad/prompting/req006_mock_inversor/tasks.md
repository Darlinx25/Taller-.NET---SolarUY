# Tareas: req006 Mock del inversor (simulación de telemetría y sensores ARM)

> Cada tarea debe ser verificable, indicar el archivo o la ruta cuando aplique y comenzar con `[ ]`. Marcar `[x]` solo cuando esté terminada y verificada.
>
> **Punto de partida:** el repositorio no contiene código de telemetría (ni DTOs, ni servicios, ni controladores, ni proyecto del simulador, ni tests). Todas las tareas están pendientes.
>
> **Diseño:** el simulador es un componente independiente (`SolarUY.Simulador`): no referencia ni comparte código con el monolito; solo comparte el contrato JSON. El Dominio del monolito no cambia.

## 0. Preparación y aprobación

- [x] 0.1 Confirmar que `requirements.md`, `specs.md` y este plan están aprobados por una persona antes de implementar.
- [x] 0.2 Confirmar que las dos excepciones a las reglas (segundo proyecto de aplicación `SolarUY.Simulador` y proyecto de tests `SolarUY.Simulador.Tests`) están aprobadas, tal como figura en "Justificación de excepciones" de `specs.md`.
- [x] 0.3 Revisar las reglas de `ControlCalidad/.agents/rules/` y los patrones de `req003_nodos_inversores`, `req004_gestion_paneles` y `req005_registro_seguimiento_tareas_mantenimiento` (DTOs, servicios, controladores, `Program.cs`, estilo de `SolarUY.Tests/Domain/PanelTests.cs`).
- [x] 0.4 Confirmar el punto de partida: `git grep -il telemetria -- SolarUY SolarUY.Tests` no devuelve resultados y la solución compila con `docker compose run --rm tools dotnet build SolarUY.sln`.
- [x] 0.5 Confirmar que el alcance sigue siendo una `app` y un `simulador`, sin balanceador, réplicas, Redis, Data Protection compartida, persistencia ni migraciones.

## 1. Dominio del monolito (`SolarUY.Domain`) — sin cambios

- [x] 1.1 Confirmar al final del requerimiento que `SolarUY/Domain/` no tiene cambios (`git status` no muestra archivos de esa carpeta).

## 2. Tests del Dominio del monolito (`SolarUY.Tests/Domain`) — sin cambios

- [x] 2.1 Confirmar al final del requerimiento que `SolarUY.Tests/` no tiene cambios. Los tests de la lógica de simulación viven en `SolarUY.Simulador.Tests` (sección 6).

## 3. Aplicación (`SolarUY.Application`)

- [x] 3.1 Crear en `SolarUY/Application/Dtos/` los DTOs de telemetría `TelemetriaDto.cs`, `TelemetriaParqueSolarDto.cs`, `TelemetriaNodoDto.cs` y `TelemetriaPanelDto.cs` con las propiedades de `specs.md` (sin DataAnnotations).
- [x] 3.2 Crear en `SolarUY/Application/Dtos/` los DTOs de topología `TopologiaSimulacionDto.cs`, `ParqueSimulacionDto.cs`, `NodoSimulacionDto.cs` y `PanelSimulacionDto.cs`.
- [x] 3.3 Crear `SolarUY/Application/Seguridad/AutenticacionComponentes.cs` con los nombres del esquema `ApiKey` y del encabezado `X-Api-Key`, para que la Presentación no dependa de Infraestructura.
- [x] 3.4 Crear `SolarUY/Application/Services/IAlmacenTelemetria.cs` con `Task<TelemetriaDto?> ObtenerAsync(CancellationToken)` y `Task GuardarAsync(TelemetriaDto, CancellationToken)`.
- [x] 3.5 Crear `SolarUY/Application/Services/ITelemetriaService.cs` (con el record `ResultadoIngestaTelemetria`) y `TelemetriaService.cs`: lee del almacén asíncrono, valida la ingesta, no guarda si es inválida, reemplaza la lectura si es válida y registra en el log la ingesta y los rechazos.
- [x] 3.6 Crear `SolarUY/Application/Services/ValidadorTelemetria.cs` con las reglas de `specs.md` (IDs, textos, longitudes con las constantes de las entidades, potencias, energía, estados exactos `Operativo`/`EnFalla` y fechas) y mensajes con la ruta del dato.
- [x] 3.7 Crear `SolarUY/Application/Services/ITopologiaSimulacionService.cs` y `TopologiaSimulacionService.cs`: leen parques, nodos (con inversor) y paneles con los repositorios existentes y mapean a los DTOs de topología, sin filtrar por estado y sin escribir en la base.
- [x] 3.8 Crear `SolarUY/Application/Services/IValidadorApiKey.cs` con `bool EsValida(string? apiKey)`.
- [x] 3.9 Confirmar que `SolarUY/Application/` no referencia EF Core, ASP.NET Core, SignalR, `Infrastructure` ni `Controllers`.

## 4. Infraestructura (`SolarUY.Infrastructure`)

- [x] 4.1 Crear `SolarUY/Infrastructure/Telemetria/AlmacenTelemetriaCache.cs` como singleton sobre `IDistributedCache`, con serialización JSON web y clave/vencimiento configurables.
- [x] 4.2 Crear `SolarUY/Infrastructure/Telemetria/ConfiguracionAlmacenTelemetria.cs` con `Seccion = "TelemetriaAlmacen"`, `Clave = "simulacion:ultima-telemetria"` y `VigenciaHoras = 24`; validar los valores en el constructor del almacén.
- [x] 4.3 Crear `SolarUY/Infrastructure/Telemetria/ConfiguracionApiSimulacion.cs` con `Seccion = "SimulacionApi"` y `ApiKey`.
- [x] 4.4 Crear `SolarUY/Infrastructure/Telemetria/ValidadorApiKey.cs` con comparación de tiempo fijo (`CryptographicOperations.FixedTimeEquals`) y sin registrar la key.
- [x] 4.5 Crear `SolarUY/Infrastructure/Telemetria/ApiKeyAuthenticationHandler.cs` usando `AutenticacionComponentes`, con fallo sin redirección (`401`).
- [x] 4.6 En `SolarUY/Program.cs`: agregar `AddDistributedMemoryCache()`, las dos `Configure<...>`, el registro de `AlmacenTelemetriaCache`, `ValidadorApiKey`, `ValidadorTelemetria`, `TelemetriaService` y `TopologiaSimulacionService`, encadenar `AddScheme` del esquema `ApiKey` a la autenticación existente y registrar la advertencia cuando `SimulacionApi:ApiKey` esté vacía.
- [x] 4.7 En `SolarUY/appsettings.json`: agregar `SimulacionApi:ApiKey` vacía y `TelemetriaAlmacen` con clave y vigencia.
- [x] 4.8 Confirmar que no se agregó ningún `DbSet`, configuración Fluent API, propiedad de navegación ni archivo en `SolarUY/Migrations/` (no hay migración en este requerimiento).
- [x] 4.9 Ejecutar `docker compose run --rm tools dotnet build SolarUY.sln` y confirmar que compila.

## 5. Presentación (`SolarUY.Controllers`)

- [x] 5.1 Crear `SolarUY/Controllers/TelemetriaController.cs` en `api/telemetria` con `GET` asíncrono, cookie y `[Authorize(Roles = "Administrador")]`.
- [x] 5.2 Crear `SolarUY/Controllers/SimulacionController.cs` en `api/simulacion`, protegido a nivel de clase con el esquema `ApiKey`.
- [x] 5.3 `GET /api/simulacion/topologia`: `200 OK` con `TopologiaSimulacionDto`, incluso con `{ "parques": [] }`.
- [x] 5.4 `POST /api/simulacion/telemetria`: `400 Bad Request` con `{ "errores": [...] }` si el payload es inválido y `204 No Content` si se almacena.
- [x] 5.5 `GET /api/telemetria`: `204 No Content` antes de la primera lectura y `200 OK` con el `TelemetriaDto` después.
- [x] 5.6 Confirmar que los controladores solo dependen de `ITelemetriaService` y `ITopologiaSimulacionService`, no acceden a repositorios ni a `DbContext` y no conocen al simulador.
- [x] 5.7 Confirmar que no se crean vistas Razor ni hubs de SignalR.
- [x] 5.8 Verificar manualmente con la `app` levantada: `GET /api/simulacion/topologia` sin key → `401` y con key → `200`; `POST /api/simulacion/telemetria` sin key → `401`; `POST` con payload inválido → `400`; `POST` válido → `204`; `GET /api/telemetria` sin cookie → `401`; con sesión de desarrollo → `204` antes de la primera lectura y `200` después.

## 6. Proyecto simulador (`SolarUY.Simulador`) y sus tests (`SolarUY.Simulador.Tests`)

### 6.1 Proyecto y Dominio del simulador

- [x] 6.1.1 Crear `SolarUY.Simulador/SolarUY.Simulador.csproj` (`Microsoft.NET.Sdk.Worker`, `net10.0`) con los paquetes `Microsoft.Extensions.Hosting` y `Microsoft.Extensions.Http`, **sin `ProjectReference` ni `Compile Include` hacia `SolarUY`**; agregarlo a `SolarUY.sln`.
- [x] 6.1.2 Crear `SolarUY.Simulador/Domain/EstadoPanelSimulado.cs` con `Operativo` y `EnFalla`.
- [x] 6.1.3 Crear `SolarUY.Simulador/Domain/GeneradorGeneracionSolar.cs` con la ventana 07:00–19:00, `CalcularFactorSolar` (hora fraccionaria, pico a las 13:00) y `CalcularPotenciaGenerada` (variación, falla, redondeo a 2 decimales y acotado a `PotenciaWp` después de redondear).
- [x] 6.1.4 Crear `SolarUY.Simulador/Domain/PanelSimulado.cs` con el estado de falla (`ActualizarEstado` con ramas excluyentes: recuperación o falla) y `GenerarLectura`.
- [x] 6.1.5 Crear `SolarUY.Simulador/Domain/LecturaPanelSimulado.cs` con validaciones y `EstaOperativo`.
- [x] 6.1.6 Crear `SolarUY.Simulador/Domain/LecturaNodoSimulado.cs` con la suma de paneles, el límite del inversor (potencia máxima `>= 0`) y la energía acumulada sin redondear.
- [x] 6.1.7 Crear `SolarUY.Simulador/Domain/LecturaParqueSolarSimulado.cs` con la consolidación por parque.
- [x] 6.1.8 Crear en `SolarUY.Simulador/Contracts/` los ocho DTOs (`TelemetriaDto`, `TelemetriaParqueSolarDto`, `TelemetriaNodoDto`, `TelemetriaPanelDto`, `TopologiaSimulacionDto`, `ParqueSimulacionDto`, `NodoSimulacionDto`, `PanelSimulacionDto`) con los mismos nombres y campos que los del monolito.

### 6.2 Tests del Dominio del simulador

- [x] 6.2.1 Crear `SolarUY.Simulador.Tests/SolarUY.Simulador.Tests.csproj` (xUnit, mismas versiones de paquetes que `SolarUY.Tests`, `ProjectReference` a `SolarUY.Simulador`) y agregarlo a `SolarUY.sln`.
- [x] 6.2.2 Crear `SolarUY.Simulador.Tests/Domain/GeneradorGeneracionSolarTests.cs`: factor 0 fuera de la ventana (06:59:59, 07:00, 19:00), factor 1 a las 13:00, simetría, hora fraccionaria, factor en [0, 1] todo el día, potencia 0 con `EnFalla`, variación acotada a `PotenciaWp`, redondeo y validaciones.
- [x] 6.2.3 Crear `SolarUY.Simulador.Tests/Domain/PanelSimuladoTests.cs`: creación y validaciones, transición a `EnFalla`, N lecturas en falla y recuperación en la N+1, sin nueva falla en la lectura en que se recupera, parámetros fuera de rango y `GenerarLectura`.
- [x] 6.2.4 Crear `SolarUY.Simulador.Tests/Domain/LecturaPanelSimuladoTests.cs`: creación, trim, validaciones y `EstaOperativo`.
- [x] 6.2.5 Crear `SolarUY.Simulador.Tests/Domain/LecturaNodoSimuladoTests.cs`: suma de paneles, lista vacía, límite del inversor (con valor, `null` y 0 kW), energía acumulada sin redondear (300 W durante 5 s crece en cada tick) y validaciones.
- [x] 6.2.6 Crear `SolarUY.Simulador.Tests/Domain/LecturaParqueSolarSimuladoTests.cs`: consolidación de potencia y energía, lista vacía y validaciones.
- [x] 6.2.7 Ejecutar `docker compose run --rm tools dotnet test SolarUY.Simulador.Tests` y confirmar que están en verde (si la referencia a un proyecto Worker desde el proyecto de tests diera problemas, detenerse y consultar antes de cambiar el diseño).

### 6.3 Orquestación, transporte y arranque

- [x] 6.3.1 Crear `SolarUY.Simulador/Simulation/ConfiguracionSimulador.cs` con sección `Simulador`, los parámetros de simulación y `IntentosEnvio`/`EsperaEntreIntentosSegundos`.
- [x] 6.3.2 Crear `SolarUY.Simulador/Simulation/IProveedorTopologia.cs` con `Task<TopologiaSimulacionDto?> ObtenerAsync(CancellationToken)`.
- [x] 6.3.3 Crear `SolarUY.Simulador/Simulation/SimuladorInversor.cs` (singleton con `TimeProvider`): depende de `IProveedorTopologia`, filtra nodos `Activo` (sin distinguir mayúsculas), devuelve `null` si la topología no está disponible, conserva el estado de fallas y energía y reemplaza el `PanelSimulado` cuyo `CodigoSerie` o `PotenciaWp` cambió.
- [x] 6.3.4 Crear `SolarUY.Simulador.Tests/Simulation/SimuladorInversorTests.cs` con proveedor falso, `ProbabilidadFalla = 0`, `VariacionMaxima = 0` y `TimeProvider` fijo a las 16:00 UTC: exclusión de nodos no activos, suma y límite del inversor, energía creciente, parque sin nodos activos, topología vacía, fallo de topología (estado conservado), panel con `PotenciaWp` modificada, falla con `ProbabilidadFalla = 1`, energía de nodo inactivo/desaparecido e inversor de 0 kW.
- [x] 6.3.5 Crear `SolarUY.Simulador/Transport/TelemetriaMapper.cs` con `Mapear(IReadOnlyList<LecturaParqueSolarSimulado>)`: lista vacía → `FechaHora = null` y `Parques` vacía; estado como texto; energía redondeada a 4 decimales solo en el DTO.
- [x] 6.3.6 Crear `SolarUY.Simulador.Tests/Transport/TelemetriaMapperTests.cs`: lista vacía, orden y valores, estado como texto, redondeo a 4 decimales y `null`.
- [x] 6.3.7 Ejecutar `docker compose run --rm tools dotnet test SolarUY.Simulador.Tests` y confirmar que todos los tests del simulador están en verde.
- [x] 6.3.8 Crear `SolarUY.Simulador/Transport/ConfiguracionSimulacionApi.cs` con `UrlBase`, `ApiKey`, `TimeoutSegundos` y la constante del encabezado `X-Api-Key`.
- [x] 6.3.9 Crear `SolarUY.Simulador/Transport/ClienteTopologia.cs` (implementa `IProveedorTopologia`) con `GET api/simulacion/topologia`, `X-Api-Key`, deserialización JSON web y `null` ante cualquier fallo (HTTP no exitoso, transporte, timeout o JSON inválido).
- [x] 6.3.10 Crear `SolarUY.Simulador/Transport/ClienteTelemetria.cs` con `POST api/simulacion/telemetria`, `X-Api-Key`, JSON y `true` solo para `2xx`.
- [x] 6.3.11 Crear `SolarUY.Simulador/Simulation/SimuladorTelemetriaHostedService.cs` con tick inmediato, `PeriodicTimer`, omisión del tick sin topología, reintentos y logs de lectura generada, éxito y error sin detener el proceso.
- [x] 6.3.12 Crear `SolarUY.Simulador/Program.cs` con `Host.CreateApplicationBuilder`, opciones validadas con `ValidateOnStart`, dos `HttpClient` tipados con `PooledConnectionLifetime`, `TimeProvider.System`, simulador y hosted service; sin `DbContext`, cadena de conexión ni repositorios.
- [x] 6.3.13 Crear `SolarUY.Simulador/appsettings.json` con `Simulador` y `SimulacionApi` (`UrlBase`, `ApiKey` vacía, `TimeoutSegundos`); sin `ConnectionStrings`.

### 6.4 Confirmaciones del simulador

- [x] 6.4.1 Confirmar que `Domain` y `Contracts` no dependen de otras carpetas ni de paquetes, que `SimuladorInversor` solo accede a la topología por `IProveedorTopologia` y no conoce HTTP (el hosted service es la única pieza de `Simulation` que usa `Transport`), y que solo `Program.cs` conoce todas las carpetas (por ejemplo, `grep` de `using` en cada carpeta).
- [x] 6.4.2 Confirmar que el simulador no levanta Kestrel, no publica puertos, no usa controladores, no accede a MySQL y no escribe en la base de datos.
- [x] 6.4.3 Ejecutar `dotnet publish` del simulador y confirmar que la salida tiene solo su propio `appsettings.json` y ninguna DLL de EF Core, MySQL ni ASP.NET Core ni el `appsettings.Development.json` del monolito.

## 7. Docker y configuración

- [x] 7.1 Crear `docker/Dockerfile.simulador` con build del worker (copiando solo `SolarUY.Simulador`) y runtime `runtime:10.0`.
- [x] 7.2 Agregar el servicio `simulador` a `docker-compose.yml` con `depends_on` solo de `app` healthy; sin puertos publicados y sin variables de MySQL.
- [x] 7.3 Pasar `SimulacionApi__ApiKey` a `app` y `SimulacionApi__UrlBase`/`SimulacionApi__ApiKey` a `simulador` desde `${SIMULACION_API_KEY:-SolarUY.Local2026}` (valor por defecto de desarrollo: no requiere configuración).
- [x] 7.4 Documentar `SIMULACION_API_KEY` como variable opcional (comentada) en `.env.example`; confirmar que el stack levanta sin definirla en el `.env`.
- [x] 7.5 Actualizar el comentario de cabecera de `docker-compose.yml` para incluir al simulador.
- [x] 7.6 Ejecutar `docker compose up -d --build` y confirmar `mysql`/`app` healthy y `simulador` corriendo.
- [x] 7.7 Confirmar que `docker compose logs simulador` muestra un tick cada 5 segundos, la lectura generada y envíos exitosos, y que `docker compose logs app` muestra la ingesta.
- [x] 7.8 Confirmar con `docker compose config` que `simulador` no recibe `ConnectionStrings__SolarUYDb` ni variables de MySQL.

## 8. Documentación

- [x] 8.1 Actualizar `README.md`: el mock es el componente independiente `simulador` (obtiene la topología por `GET /api/simulacion/topologia`, envía telemetría por `POST /api/simulacion/telemetria`, no corre dentro del monolito, no comparte código con él ni accede a MySQL); pasar "Mock Inversor" a Listo; actualizar la descripción del nivel "Simulación" de la arquitectura y el texto de `docker compose up` (MySQL + migraciones + app + simulador); mencionar la evolución a balanceador sin dibujarlo.
- [x] 8.2 Actualizar `README.md` con la verificación del mock: `docker compose logs -f simulador`, login de desarrollo y `GET /api/telemetria`.
- [x] 8.3 Actualizar `DESARROLLO.md`: servicio `simulador` en la lista de servicios, variable `SIMULACION_API_KEY` (quien ya tenga `.env` debe agregarla) y verificación del mock.
- [x] 8.4 Agregar `req006_mock_inversor` a la tabla de `ControlCalidad/README.md` con el estado correspondiente (Planificado / En progreso / Implementado) y describir el componente independiente.
- [x] 8.5 Actualizar la sección C y el resumen de `ControlCalidad/README.md`: agregar `SolarUY.Simulador.Tests` (tests de la lógica del simulador) y su comando de ejecución, manteniendo que para Application, Infrastructure y Presentación del monolito no se generan tests.
- [x] 8.6 Registrar en este archivo las evidencias de la verificación final (sección 9).

## 9. Verificación final

- [x] 9.1 Build completo: `docker compose run --rm tools dotnet build SolarUY.sln` sin errores.
- [x] 9.2 Tests completos: `docker compose run --rm tools dotnet test SolarUY.sln` con `SolarUY.Tests`, `SolarUY.Simulador.Tests` y ArchUnitNET en verde.
- [ ] 9.3 Ejecutar SonarQube con `./ControlCalidad/SonarQube/analizar.ps1` (Windows) o `analizar.sh` (Linux/macOS) y registrar hallazgos (el análisis compila `SolarUY.sln`, por lo que incluye al simulador). **(pendiente: no hay `SONAR_TOKEN` en el `.env` local; ver evidencias)**
- [x] 9.4 Prueba manual completa:
  - `docker compose up -d --build`;
  - `docker compose ps` y `docker compose logs simulador`;
  - `GET /api/simulacion/topologia` sin key → `401`; con `X-Api-Key` → `200`;
  - `GET /api/telemetria` sin cookie → `401`;
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
- [x] 9.5 Confirmar que no se modificó `ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests`, `SolarUY/Domain/` ni `SolarUY.Tests/`, que no se generó migración y que `git status` solo muestra los archivos previstos.

## Notas

- La telemetría es efímera: no hay persistencia, `DbSet`, configuración Fluent API ni migración.
- El simulador es un worker de consola independiente: no expone endpoints ni puertos, no accede a MySQL, no referencia el proyecto `SolarUY` ni comparte código con él. El único acoplamiento es el contrato JSON (DTOs duplicados a propósito en ambos lados).
- La aplicación es la única que accede a MySQL y la única que conoce la topología registrada.
- La última lectura vive detrás de `IAlmacenTelemetria` asíncrono sobre `IDistributedCache`; hoy memoria, mañana Redis sin cambiar controladores ni simulador.
- La API key autentica al simulador como componente interno; no es un mecanismo de login de usuarios. Tiene un valor por defecto de desarrollo en `docker-compose.yml`, por lo que levantar el stack no requiere configuración.
- No se implementan balanceador de carga ni réplicas en esta iteración, pero el diseño queda preparado para agregarlos.
- No se envían comandos al inversor ni se integra hardware real.
- Tests: solo xUnit de la lógica pura del simulador (`SolarUY.Simulador.Tests`). No se crean tests en Application, Infrastructure ni Presentation del monolito, ni de los clientes HTTP, el hosted service o `Program.cs` del simulador.
- No se modifica `CodeQuality.Tests` sin autorización explícita.

## Evidencias de la verificación

- **Build y tests (contenedor `tools`):** `docker compose run --rm tools dotnet build SolarUY.sln` sin errores ni advertencias; `docker compose run --rm tools dotnet test SolarUY.sln`: `SolarUY.Simulador.Tests` 102 correctos, `SolarUY.Tests` 80 correctos y `CodeQuality.Tests` (ArchUnitNET) 9 correctos. `CodeQuality.Tests`, `SolarUY/Domain/`, `SolarUY.Tests/` y `SolarUY/Migrations/` sin cambios (`git status`).
- **Stack:** `docker compose up -d --build` deja `mysql` y `app` healthy y `simulador` running. Un tick exacto cada 5,0 s; el log del simulador muestra la lectura generada (parques, nodos, paneles, W) y el envío, y el de `app` la ingesta.
- **Endpoints internos:** topología sin key → `401`, con key incorrecta → `401`, con key por query string → `401`, con key → `200`; `POST` sin key o con key incorrecta → `401`. Con `SimulacionApi:ApiKey` vacía la app arranca, registra la advertencia y responde `401`.
- **Consulta:** `GET /api/telemetria` sin cookie → `401` (no `302`: con `[ApiController]` el middleware no redirige), con la API key (sin rol) → `401`, con sesión de desarrollo → `200`; tras reiniciar `app` sin simulador → `204` y `200` tras el primer tick.
- **Ingesta:** `POST` con payload inválido (potencia negativa, estado inexistente), `null` y JSON malformado → `400` y la lectura almacenada no cambia; `{}` y `{"fechaHora":null,"parques":[]}` son lecturas vacías válidas (`204`, luego `GET` → `200` con `parques: []`); parques vacíos con `fechaHora` → `400`.
- **Simulación (simulador efímero con `Simulador__DesfaseHorarioUruguayHoras=-9`, ~13:30 hora local):** potencia de nodo = suma de paneles, potencia de parque = suma de nodos; cada panel ≤ su `PotenciaWp`; nodo `EnMantenimiento` excluido; energía creciente entre lecturas; con el inversor en 1 kW el nodo informa 1000 W con paneles sumando 1074,04 W; al bajar un panel a 100 Wp la lectura siguiente informa 99 W. Con `Simulador__ProbabilidadFalla=1` y `IntervaloSegundos=2` los paneles informan 6 lecturas `EnFalla` (0 W) y 1 `Operativo`, en ciclo. Fuera de la ventana horaria (hora real) todo informa 0 W.
- **Resiliencia:** con `app` detenida el simulador sigue corriendo y registra `No se pudo obtener la topología` / `Topología no disponible: se omite el tick`; al volver `app`, la telemetría se reanuda sin reiniciarlo y la energía continúa. Al reiniciar el simulador la energía vuelve a cero.
- **Independencia del simulador:** `docker compose config` muestra que `simulador` solo depende de `app` y no recibe `ConnectionStrings__SolarUYDb` ni variables de MySQL; `dotnet publish` del simulador solo contiene su `appsettings.json` y ninguna DLL de EF Core, MySQL ni ASP.NET Core; `grep` confirma que no referencia EF Core, MySQL ni cadenas de conexión.
- **Dependencias de capas:** los archivos nuevos de `SolarUY/Application/` no usan EF Core, ASP.NET Core, `Infrastructure` ni `Controllers` (el `using Microsoft.AspNetCore.Http` de `TareaMantenimientoService`, de req005, es previo); los controladores nuevos no usan repositorios ni `DbContext`.
- **SonarQube (pendiente):** no hay `SONAR_TOKEN` en el `.env` local. Falta ejecutar `./ControlCalidad/SonarQube/analizar.ps1` y registrar los hallazgos; con eso el requerimiento pasa a `Implementado` en `ControlCalidad/README.md` (hoy figura `En progreso`).
- **Ajustes surgidos de la verificación:** (1) `GET /api/telemetria` sin sesión responde `401`, no `302` (spec y tareas corregidas); (2) se bajó a `Warning` el log de `Microsoft.EntityFrameworkCore.Database.Command` en la app y de `System.Net.Http.HttpClient` en el simulador, porque la topología se consulta cada 5 s; (3) el hosted service usa `Transport` (spec ajustada).
