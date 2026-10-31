# Especificación Técnica: Históricos de generación

## Objetivo

Persistir un resumen diario por parque, nodo y panel a partir de la telemetría que ya envía el simulador, y exponer una pantalla autenticada para consultarlos por tipo de entidad, entidad y rango de fechas, con agrupación por día, mes o año, tarjetas de totales, tabla y gráfico.

## Decisiones y supuestos

- Se agregan tres entidades de resumen diario; no se persisten lecturas crudas ni se modifican el simulador, el contrato de telemetría o los endpoints de simulación.
- Un resumen se identifica por entidad y `FechaLocal`; la fecha local se obtiene de `FechaHora` UTC con un desfase fijo de `-3` horas.
- Cada lectura válida actualiza potencia acumulada, cantidad de lecturas, energía, potencia máxima, fallas, último estado y última fecha/hora UTC.
- La energía se integra en el Dominio con el intervalo real desde la lectura anterior de la misma entidad, con tope de `15` segundos. La primera lectura del día suma `0`.
- El promedio no se persiste: se calcula como `PotenciaAcumuladaW / CantidadLecturas`; en agrupaciones se usa el promedio ponderado por cantidad de lecturas.
- Las lecturas con fecha/hora menor o igual a la última registrada para esa entidad se ignoran sin modificar el resumen.
- Se guardan los nombres o códigos de la entidad en el resumen para conservar el histórico aunque la topología se dé de baja lógicamente.
- Los resúmenes se conservan indefinidamente; no hay retención ni purga.
- El filtro de entidad es opcional: sin entidad seleccionada se consultan todas las entidades del tipo elegido.
- El histórico comienza con la primera lectura posterior al despliegue; no hay backfill.
- La pantalla es MVC con `GET`, sin SignalR, y usa Chart.js 4.4.9 vendorizado.
- No se tocan `README.md`, `ControlCalidad/README.md` ni `CodeQuality.Tests`; el usuario actualiza la documentación.

## Flujo de ingesta

1. `SimulacionController` recibe la telemetría y llama a `ITelemetriaService.RegistrarLecturaAsync`.
2. `TelemetriaService` valida el payload con `ValidadorTelemetria`.
3. Si es válido, `RegistroHistoricoService` calcula la fecha local, carga en una sola consulta los resúmenes de parques de esa fecha con sus nodos y paneles, actualiza o crea los resúmenes y ejecuta un único `SaveChangesAsync`.
4. Si la actualización histórica falla, la excepción se propaga: no se reemplaza la última lectura en caché y el simulador puede reintentar el envío.
5. Si la actualización es correcta, `TelemetriaService` guarda la última lectura en `IAlmacenTelemetria` como hoy.

## 1) Dominio (`SolarUY.Domain`)

### Entidades

Archivos en `SolarUY/Domain/Entities/`:

- `ResumenDiarioParque`: `Id`, `ParqueSolarId` (FK lógica a `ParqueSolar`), `NombreParque`, `FechaLocal`, `PotenciaAcumuladaW`, `CantidadLecturas`, `EnergiaKWh`, `PotenciaMaximaW`, `Fallas`, `UltimoEstado`, `UltimaFechaHoraUtc`, colección `Nodos`.
- `ResumenDiarioNodo`: `Id`, `ResumenDiarioParqueId`, `NodoId` (FK lógica a `Nodo`), `CodigoNodo`, `FechaLocal`, los mismos acumuladores, `UltimaFechaHoraUtc`, colección `Paneles` y navegación al resumen de parque.
- `ResumenDiarioPanel`: `Id`, `ResumenDiarioNodoId`, `PanelId` (FK lógica a `Panel`), `CodigoSerie`, `FechaLocal`, los mismos acumuladores y `UltimaFechaHoraUtc`.

Comportamiento común:

- `Crear(...)` valida identificadores no vacíos, texto obligatorio, fecha local distinta de `default` y crea un `Id` nuevo.
- `RegistrarLectura(DateTime fechaHoraUtc, decimal potenciaW, EstadoPanel estado, int brechaMaximaSegundos)` devuelve `false` sin cambios si ya existe una lectura con fecha/hora igual o posterior.
- Si la lectura es nueva, suma potencia, incrementa lecturas, suma energía con `CalculadoraEnergia`, actualiza la potencia máxima, incrementa fallas si `estado == EnFalla`, actualiza el último estado y la última fecha/hora, y devuelve `true`.
- `ResumenDiarioParque.AgregarNodo(...)` y `ResumenDiarioNodo.AgregarPanel(...)` son idempotentes por identificador y validan que la fecha local coincida con la del padre.
- `CantidadLecturas` y `Fallas` no pueden ser negativas; potencias y energía no pueden ser negativas; `PotenciaMaximaW` no puede superar la potencia acumulada cuando hay lecturas.
- Los constructores sin parámetros quedan privados para Entity Framework Core.

### Enums y objetos de valor

Archivos en `SolarUY/Domain/Enums/` y `SolarUY/Domain/ValueObjects/`:

- `EstadoPanel`: `Operativo`, `EnFalla`.
- `TipoEntidadHistorico`: `Parque`, `Nodo`, `Panel`.
- `AgrupacionHistorico`: `Dia`, `Mes`, `Anio`.
- `CalculadoraEnergia.CalcularEnergiaKWh(decimal potenciaW, DateTime fechaHoraUtc, DateTime? fechaHoraUltimaLecturaUtc, int brechaMaximaSegundos)`:
  - valida potencia no negativa, fecha actual distinta de `default`, fecha anterior válida si existe y `brechaMaximaSegundos > 0`;
  - devuelve `0` si no hay lectura anterior o si el intervalo no es positivo;
  - limita el intervalo a `brechaMaximaSegundos` y calcula `potenciaW / 1000 * segundos / 3600` sin redondear.
- `FiltroHistorico.Crear(...)`: `TipoEntidad`, `EntidadId` opcional, `Desde`, `Hasta`, `Agrupacion`; valida enums definidos, fechas no `default`, `Desde <= Hasta` y `EntidadId` no vacío cuando se informa.
- `ResumenDiarioGeneracion`: proyección inmutable con `EntidadId`, `NombreEntidad`, `FechaLocal`, `PotenciaAcumuladaW`, `CantidadLecturas`, `EnergiaKWh`, `PotenciaMaximaW`, `Fallas`, `UltimoEstado` y `PotenciaPromedioW` calculado.

### Validaciones

- No se aceptan lecturas con fecha/hora anterior o igual a la última de la entidad; se consideran duplicadas o fuera de orden.
- No se crean resúmenes de nodo o panel con una fecha local distinta a la del padre.
- No se aceptan estados fuera de `EstadoPanel`; la conversión desde el texto de telemetría la hace `RegistroHistoricoService`.

### Excepciones

`SolarUY/Domain/Exceptions/FiltroHistoricoInvalidoException.cs` con mensaje en español. Las validaciones de entidades usan `ArgumentException`, `ArgumentOutOfRangeException` o `ArgumentNullException`, como el resto del Dominio.

### Repositorios (contratos)

`SolarUY/Domain/Repositories/IHistoricoRepository.cs`:

```csharp
Task<IReadOnlyList<ResumenDiarioParque>> ObtenerPorFechaAsync(DateOnly fechaLocal, CancellationToken cancellationToken = default);
Task<IReadOnlyList<ResumenDiarioGeneracion>> ObtenerResumenesAsync(FiltroHistorico filtro, CancellationToken cancellationToken = default);
Task AgregarAsync(ResumenDiarioParque resumen, CancellationToken cancellationToken = default);
Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
```

`ObtenerPorFechaAsync` devuelve resúmenes trackeados con `Include` de nodos y paneles. `ObtenerResumenesAsync` devuelve solo resúmenes del tipo filtrado, dentro del rango y, si hay `EntidadId`, de esa entidad.

## 2) Aplicación (`SolarUY.Application`)

### DTOs

En `SolarUY/Application/Dtos/`:

- `FiltroHistoricoDto`: `TipoEntidad`, `EntidadId`, `DateOnly? Desde`, `DateOnly? Hasta`, `Agrupacion`; `Desde` y `Hasta` con `[Required]`.
- `OpcionEntidadDto`: `Id`, `Nombre`.
- `OpcionesHistoricoDto`: listas `Parques`, `Nodos`, `Paneles`.
- `ResumenHistoricoDto`: `EntidadId`, `EntidadNombre`, `Periodo`, `EnergiaKWh`, `PotenciaPromedioW`, `PotenciaMaximaW`, `Fallas`, `UltimoEstado`.
- `PuntoGraficoHistoricoDto`: `Periodo`, `EnergiaKWh`.
- `HistoricoGeneracionDto`: `Resumenes`, `PuntosGrafico`, `EnergiaTotalKWh`, `PotenciaPromedioW`, `PotenciaMaximaW`, `Fallas`.

### `ConfiguracionHistorico`

En `SolarUY/Application/Services/ConfiguracionHistorico.cs`:

```csharp
public const string Seccion = "Historico";
public int BrechaMaximaSegundos { get; set; } = 15;
public int DesfaseHorarioUruguayHoras { get; set; } = -3;
```

### `IRegistroHistoricoService` / `RegistroHistoricoService`

En `SolarUY/Application/Services/`. Registro scoped.

- `Task RegistrarLecturaAsync(TelemetriaDto lectura, CancellationToken cancellationToken = default)`.
- Si `Parques` está vacío, no hace nada.
- Calcula `FechaLocal` desde `lectura.FechaHora` con el desfase configurado.
- Carga una sola vez los resúmenes de esa fecha y actualiza parques, nodos y paneles en memoria.
- Para nodo y parque usa como estado consolidado `EnFalla` si al menos un panel de ese tick está en `EnFalla`; si no, `Operativo`.
- Para panel convierte `Operativo`/`EnFalla` a `EstadoPanel`.
- Crea los resúmenes faltantes con `AgregarNodo`/`AgregarPanel` y ejecuta un único `SaveChangesAsync`.
- Registra en `Debug` las lecturas ignoradas por duplicadas o fuera de orden. No usa `DateTime.Now`.

### `IHistoricoService` / `HistoricoService`

En `SolarUY/Application/Services/`. Registro scoped. Depende de `IHistoricoRepository`, `IParqueSolarRepository`, `INodoRepository`, `IPanelRepository`, `IOptions<ConfiguracionHistorico>` y `TimeProvider`.

```csharp
FiltroHistoricoDto CrearFiltroPredeterminado();
Task<OpcionesHistoricoDto> ObtenerOpcionesAsync(CancellationToken cancellationToken = default);
Task<HistoricoGeneracionDto> ConsultarAsync(FiltroHistoricoDto filtro, CancellationToken cancellationToken = default);
```

- `CrearFiltroPredeterminado`: `Parque`, sin entidad, últimos 30 días locales UTC-3 y agrupación `Dia`.
- `ObtenerOpcionesAsync`: recorre la topología vigente con los repositorios existentes. Etiquetas: parque `Nombre`, nodo `Parque / Código`, panel `Parque / Nodo / CódigoSerie`. No incluye entidades dadas de baja.
- `ConsultarAsync`:
  - construye `FiltroHistorico` y consulta los resúmenes diarios;
  - agrupa por día (`yyyy-MM-dd`), mes (`yyyy-MM`) o año (`yyyy`);
  - devuelve una fila por entidad y período con energía sumada, potencia promedio ponderada, potencia máxima, fallas sumadas y último estado del período;
  - devuelve puntos de gráfico sumando la energía de todas las entidades por período;
  - calcula los totales sobre todos los resúmenes diarios;
  - ordena filas y puntos por período ascendente;
  - sin datos devuelve listas vacías y totales en cero.

### `ITelemetriaService` / `TelemetriaService`

- Se agrega la dependencia `IRegistroHistoricoService`.
- `RegistrarLecturaAsync` conserva la validación actual; si es válida, llama primero a `RegistroHistoricoService.RegistrarLecturaAsync` y después a `IAlmacenTelemetria.GuardarAsync`.
- Si falla la persistencia histórica, no se guarda la última lectura y la excepción se propaga.
- El registro de `ITelemetriaService` pasa de singleton a scoped.

## 3) Infraestructura (`SolarUY.Infrastructure`)

### DbSet y configuraciones

`SolarUYDbContext` agrega:

```csharp
public DbSet<ResumenDiarioParque> ResumenesDiariosParques => Set<ResumenDiarioParque>();
public DbSet<ResumenDiarioNodo> ResumenesDiariosNodos => Set<ResumenDiarioNodo>();
public DbSet<ResumenDiarioPanel> ResumenesDiariosPaneles => Set<ResumenDiarioPanel>();
```

Configuraciones en `SolarUY/Infrastructure/Persistence/Configurations/`:

- Tablas `ResumenesDiariosParques`, `ResumenesDiariosNodos`, `ResumenesDiariosPaneles`.
- `Id` no generado por la base.
- `FechaLocal` como `date`.
- Potencias `decimal(18,2)`, energía `decimal(18,6)`, contadores `int`.
- `UltimoEstado` convertido a texto con longitud 20.
- Nombres: parque 100, nodo 50, panel 50.
- Índices únicos `(ParqueSolarId, FechaLocal)`, `(NodoId, FechaLocal)` y `(PanelId, FechaLocal)`.
- FK de resumen a topología con `Restrict`; FK de nodo a resumen de parque y de panel a resumen de nodo con `Cascade`.
- Sin filtros de consulta globales en los resúmenes.

### Repositorio

`SolarUY/Infrastructure/Repositories/HistoricoRepository.cs` implementa `IHistoricoRepository`.

- `ObtenerPorFechaAsync`: una consulta trackeada con `Include(parque => parque.Nodos).ThenInclude(nodo => nodo.Paneles)`, ordenada por nombre.
- `AgregarAsync`: agrega el resumen de parque; los hijos se agregan por navegación.
- `SaveChangesAsync`: delega en `DbContext`.
- `ObtenerResumenesAsync`: consulta con `AsNoTracking` la tabla del tipo filtrado, filtra `FechaLocal` y `EntidadId`, arma `NombreEntidad` con los nombres guardados y devuelve `ResumenDiarioGeneracion` ordenado por `FechaLocal`.

### Migración

Cambia el esquema. Migración `AgregarResumenesDiariosHistoricos` con las tres tablas, índices y FKs. Se aplica con el servicio `migrate` del stack.

### Registro de dependencias (`Program.cs`)

- `AddOptions<ConfiguracionHistorico>()` con sección `Historico`, validación de `BrechaMaximaSegundos > 0` y desfase entre `-12` y `12`.
- `AddSingleton(TimeProvider.System)`.
- `AddScoped<IHistoricoRepository, HistoricoRepository>()`.
- `AddScoped<IRegistroHistoricoService, RegistroHistoricoService>()`.
- `AddScoped<IHistoricoService, HistoricoService>()`.
- Cambiar `ITelemetriaService` a `AddScoped`.

En `SolarUY/appsettings.json`:

```json
"Historico": {
  "BrechaMaximaSegundos": 15,
  "DesfaseHorarioUruguayHoras": -3
}
```

## 4) Presentación (`SolarUY.Controllers` + `Views`)

### Controlador y acciones

`SolarUY/Controllers/HistoricoController.cs`, `[Authorize]`, inyecta `IHistoricoService`.

- `GET Index(FiltroHistoricoDto? filtro, CancellationToken)`:
  - sin query string usa `CrearFiltroPredeterminado()` y limpia `ModelState`;
  - carga las opciones de topología;
  - si el filtro es inválido agrega un error y devuelve un resultado vacío;
  - si es válido consulta y devuelve `HistoricoViewModel`.
- No hay acciones de escritura.

### Autorización

Cualquier usuario autenticado, sin rol específico. Sin sesión, la cookie redirige al login de desarrollo.

### Vistas

- `SolarUY/Models/HistoricoViewModel.cs`: `Filtro`, `Opciones`, `Resultado`.
- `SolarUY/Views/Historico/Index.cshtml`:
  - formulario `GET` con tipo de entidad, entidad (opción vacía “Todas”), desde, hasta y agrupación;
  - tarjetas con energía total, potencia promedio, potencia máxima y fallas;
  - tabla con período, entidad, energía, potencia promedio, potencia máxima, fallas y último estado;
  - `canvas` del gráfico y bloque JSON con `PuntosGrafico`;
  - mensaje informativo si no hay datos.
- `SolarUY/wwwroot/js/historico.js`: filtra las opciones de entidad según el tipo y construye un gráfico de línea con Chart.js usando los puntos del bloque JSON.
- `SolarUY/wwwroot/lib/chart.js/chart.umd.min.js` y `LICENSE.md`: Chart.js 4.4.9 vendorizado.
- `_Layout.cshtml`: enlace “Histórico” visible solo para usuarios autenticados.

## 5) Pruebas

Tests xUnit del Dominio en `SolarUY.Tests/Domain/`:

- `CalculadoraEnergiaTests`: sin lectura anterior, intervalo normal, tope de 15 segundos, intervalo no positivo y validaciones.
- `FiltroHistoricoTests`: filtro válido, rango invertido, fechas vacías, entidad vacía y enums inválidos.
- `ResumenDiarioGeneracionTests`: promedio calculado, promedio cero sin lecturas y validaciones.
- `ResumenDiarioParqueTests`: creación, primera lectura con energía 0, acumulados, máximo, fallas, estado, duplicado/fuera de orden y alta idempotente de nodos.
- `ResumenDiarioNodoTests`: los mismos comportamientos y alta idempotente de paneles.
- `ResumenDiarioPanelTests`: los mismos comportamientos sin hijos.

No se agregan tests en Application, Infrastructure ni Presentation.

## 6) Calidad y verificación

- [ ] Build: `docker compose run --rm tools dotnet build SolarUY.sln`
- [ ] Tests: `docker compose run --rm tools dotnet test SolarUY.sln`
- [ ] ArchUnitNET en verde
- [ ] SonarQube ejecutado y hallazgos revisados
- [ ] Prueba manual de ingesta, persistencia y consulta
- [ ] `README.md` y `ControlCalidad/README.md` no modificados por el agente

## 7) Fuera de alcance

- Lecturas crudas, backfill, retención, purga y archivado.
- Dashboard en tiempo real, SignalR y alertas.
- Comandos al inversor y cambios en el simulador o en el contrato de telemetría.
- Cambios en CRUD, autenticación, roles, exportaciones y pronósticos.
