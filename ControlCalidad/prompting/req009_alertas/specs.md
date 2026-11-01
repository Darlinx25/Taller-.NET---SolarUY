# Especificación Técnica: Alertas por fallos en paneles

## Objetivo

Detectar los fallos de paneles a partir de la telemetría que ya envía el simulador, registrar cada fallo como una alerta con historial en MySQL (se abre cuando el panel pasa a `EnFalla` y se cierra cuando se recupera o deja de aparecer en la lectura), y mostrarlas en una pantalla de solo lectura con un indicador en el menú que se actualiza en vivo por SignalR. El acceso es anónimo (rol: Todos), como el Dashboard.

## Decisiones y supuestos

- Se agrega una entidad `AlertaPanel` y una tabla `AlertasPanel`. **Hay migración** (`AgregarAlertasPanel`).
- Una alerta es por panel. Hay a lo sumo una alerta activa por panel; si el panel se recupera y vuelve a fallar se crea una alerta nueva.
- La alerta se abre cuando un panel informa `EnFalla` y no tiene alerta activa. Se cierra cuando el panel informa `Operativo` o cuando el panel no aparece en una lectura con parques (su nodo dejó de estar `Activo` o fue dado de baja). Las lecturas sin parques no modifican alertas.
- La fecha de apertura y la de cierre son la `FechaHora` UTC del panel en la lectura (para un panel que desaparece, la `FechaHora` de la lectura). Las lecturas repetidas o anteriores no abren ni cierran alertas: `AlertaPanel.Cerrar` ignora un fin menor o igual al inicio y `AlertaPanel.DebeAbrirse` ignora una apertura menor o igual al fin de la última alerta del panel.
- Se guardan en la alerta los nombres y códigos de parque, nodo y panel de la lectura, para conservar el historial aunque la topología se dé de baja lógicamente.
- Se reutilizan las constantes de longitud de `ParqueSolar.NombreMaxLength` (100), `Nodo.CodigoMaxLength` (50) y `Panel.CodigoSerieMaxLength` (50).
- La detección ocurre en la ingesta, dentro de `TelemetriaService`, después del registro histórico y antes de guardar la última lectura. Si el registro de alertas falla, la excepción se propaga: no se reemplaza la última lectura y el simulador reintenta, igual que en req007. El reintento es seguro: las lecturas repetidas son ignoradas por el histórico y por las alertas.
- El aviso en vivo usa el hub existente `TelemetriaHub` (`/hubs/telemetria`) con un evento nuevo, `ActualizarAlertas`, que se envía **solo cuando una lectura abrió o cerró alguna alerta** (no en cada lectura). Un fallo al notificar no falla la ingesta.
- El indicador está en el menú de todas las pantallas. Su cantidad se obtiene con `GET /Alertas/CantidadActivas` al cargar la página, al recibir `ActualizarAlertas` y al reconectar el hub. Si el cliente de SignalR no carga, el indicador igual se actualiza al cargar la página.
- La pantalla de alertas se carga por `GET` y no se actualiza sola; el indicador del menú sí.
- Las fechas se guardan y muestran en UTC, rotuladas "UTC", como las del Dashboard. La duración se muestra como `horas:minutos:segundos` (las horas pueden superar 24).
- El historial muestra las 50 alertas cerradas más recientes, ordenadas por fecha de fin descendente; las activas se ordenan por fecha de inicio descendente. Es una constante del servicio (`CantidadHistorial = 50`).
- Las alertas se conservan indefinidamente. No hay retención ni purga.
- Autorización: sin `[Authorize]` en el controlador, igual que `DashboardController` y el hub. No hay acciones de escritura ni formularios.
- Se reutiliza `TimeProvider` (ya registrado como singleton). No se agrega configuración en `appsettings.json`.
- No se modifica el simulador, el contrato de telemetría ni los endpoints `/api/simulacion/*`. No se modifica `CodeQuality.Tests`.
- El cliente de SignalR se carga desde el mismo CDN y con la misma versión y hash de integridad que usa `Views/Dashboard/Index.cshtml`.

## Flujo de ingesta

1. `SimulacionController` recibe la telemetría y llama a `ITelemetriaService.RegistrarLecturaAsync`.
2. `TelemetriaService` valida el payload con `ValidadorTelemetria`.
3. Si es válido, llama a `IRegistroHistoricoService.RegistrarLecturaAsync` (sin cambios, req007).
4. Luego llama a `IRegistroAlertasService.RegistrarLecturaAsync`, que abre y cierra alertas y hace un único `SaveChangesAsync` si hubo cambios.
5. Si hubo cambios y el guardado fue correcto, `RegistroAlertasService` llama a `IAlertasNotifier`, que emite `ActualizarAlertas` por SignalR.
6. Si el histórico o las alertas fallan, la excepción se propaga y no se guarda la última lectura.
7. Si todo es correcto, `TelemetriaService` guarda la última lectura en `IAlmacenTelemetria` y notifica `ActualizarTelemetria` como hoy.

## 1) Dominio (`SolarUY.Domain`)

### Entidades

Archivo `SolarUY/Domain/Entities/AlertaPanel.cs`, namespace `SolarUY.Domain.Entities`.

`AlertaPanel` — alerta de un panel en falla.

| Miembro | Tipo | Regla |
|---|---|---|
| `NombreParqueMaxLength` | `const int` | `= ParqueSolar.NombreMaxLength`. |
| `CodigoNodoMaxLength` | `const int` | `= Nodo.CodigoMaxLength`. |
| `CodigoSerieMaxLength` | `const int` | `= Panel.CodigoSerieMaxLength`. |
| `Id` | `Guid` | Generado en `Abrir`; `private set`. |
| `ParqueSolarId` | `Guid` | Obligatorio, no vacío. |
| `NombreParque` | `string` | Obligatorio, trim, máximo `NombreParqueMaxLength`. |
| `NodoId` | `Guid` | Obligatorio, no vacío. |
| `CodigoNodo` | `string` | Obligatorio, trim, máximo `CodigoNodoMaxLength`. |
| `PanelId` | `Guid` | Obligatorio, no vacío. |
| `CodigoSerie` | `string` | Obligatorio, trim, máximo `CodigoSerieMaxLength`. |
| `FechaInicioUtc` | `DateTime` | Obligatoria, distinta de `default`. |
| `FechaFinUtc` | `DateTime?` | `null` mientras la alerta está activa. |
| `EstaActiva` | `bool` (calculada) | `FechaFinUtc is null`. |

Todas las propiedades tienen `private set`. Constructor privado sin parámetros para EF Core.

Comportamiento:

- `public static AlertaPanel Abrir(Guid parqueSolarId, string nombreParque, Guid nodoId, string codigoNodo, Guid panelId, string codigoSerie, DateTime fechaInicioUtc)`: valida los datos, normaliza con `Trim`, genera el `Id` y devuelve una alerta activa.
- `public bool Cerrar(DateTime fechaFinUtc)`:
  - si la alerta ya está cerrada lanza `InvalidOperationException`;
  - si `fechaFinUtc` es `default` lanza `ArgumentException`;
  - si `fechaFinUtc <= FechaInicioUtc` devuelve `false` sin modificar la alerta;
  - en caso contrario asigna `FechaFinUtc` y devuelve `true`.
- `public TimeSpan CalcularDuracion(DateTime ahoraUtc)`: valida que `ahoraUtc` no sea `default` (`ArgumentException`); devuelve `(FechaFinUtc ?? ahoraUtc) - FechaInicioUtc`, o `TimeSpan.Zero` si el resultado fuera negativo.
- `public static bool DebeAbrirse(AlertaPanel? ultimaAlertaDelPanel, DateTime fechaHoraUtc)`: valida que `fechaHoraUtc` no sea `default` (`ArgumentException`); devuelve `true` si no hay alerta previa; `false` si la previa está activa; y, si la previa está cerrada, `true` solo cuando `fechaHoraUtc > FechaFinUtc`.

### Validaciones

- Los identificadores no pueden ser `Guid.Empty` (`ArgumentException`).
- Los textos son obligatorios, se normalizan con `Trim` y no pueden superar su longitud máxima (`ArgumentException`).
- Las fechas no pueden ser `default` (`ArgumentException`).
- Mensajes en español, igual que `Panel` y `Nodo`.

### Excepciones

No se crean excepciones nuevas: se usan `ArgumentException` e `InvalidOperationException`.

### Repositorios (contratos)

Archivo `SolarUY/Domain/Repositories/IAlertaPanelRepository.cs`.

```csharp
public interface IAlertaPanelRepository
{
    Task<IReadOnlyList<AlertaPanel>> ObtenerActivasAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AlertaPanel>> ObtenerCerradasRecientesAsync(int cantidad, CancellationToken cancellationToken = default);

    Task<AlertaPanel?> ObtenerUltimaPorPanelAsync(Guid panelId, CancellationToken cancellationToken = default);

    Task<int> ContarActivasAsync(CancellationToken cancellationToken = default);

    Task AgregarAsync(AlertaPanel alerta, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

- `ObtenerActivasAsync`: alertas con `FechaFinUtc` nula, trackeadas, ordenadas por `FechaInicioUtc` descendente.
- `ObtenerCerradasRecientesAsync`: alertas con `FechaFinUtc` no nula, `AsNoTracking`, ordenadas por `FechaFinUtc` descendente, limitadas a `cantidad` (`cantidad > 0`, `ArgumentOutOfRangeException`).
- `ObtenerUltimaPorPanelAsync`: la alerta con mayor `FechaInicioUtc` del panel (activa o cerrada), `AsNoTracking`, o `null`.
- `ContarActivasAsync`: cantidad de alertas con `FechaFinUtc` nula.

## 2) Aplicación (`SolarUY.Application`)

### DTOs

Archivos en `SolarUY/Application/Dtos/`, clases con propiedades `get; set;` como el resto.

| DTO | Propiedades |
|---|---|
| `AlertaPanelDto` | `Guid Id`, `string NombreParque`, `string CodigoNodo`, `string CodigoSerie`, `DateTime FechaInicioUtc`, `DateTime? FechaFinUtc`, `bool EstaActiva`, `TimeSpan Duracion` |
| `ListadoAlertasDto` | `IReadOnlyList<AlertaPanelDto> Activas`, `IReadOnlyList<AlertaPanelDto> Historial` |

### Servicios y casos de uso

Archivos en `SolarUY/Application/Services/`.

**`IAlertasNotifier`** (puerto): `Task NotificarAlertasActualizadasAsync(CancellationToken cancellationToken = default);`. La implementación vive en Presentación (`Hubs/`).

**`IRegistroAlertasService` / `RegistroAlertasService`** (scoped): `Task RegistrarLecturaAsync(TelemetriaDto lectura, CancellationToken cancellationToken = default);`.

- Dependencias: `IAlertaPanelRepository`, `IAlertasNotifier`, `ILogger<RegistroAlertasService>`.
- Constantes privadas `EstadoEnFalla = "EnFalla"` (los estados válidos del contrato, como en `RegistroHistoricoService`).
- Algoritmo:
  1. `ArgumentNullException.ThrowIfNull(lectura)`.
  2. Si `lectura.Parques` es nula o vacía, o `lectura.FechaHora` es nula o `default`, termina sin cambios.
  3. Obtiene las alertas activas con `ObtenerActivasAsync` y las indexa por `PanelId`.
  4. Recorre parques, nodos y paneles de la lectura (omitiendo elementos nulos) y registra cada `PanelId` leído:
     - si el panel está `EnFalla` y no tiene alerta activa: obtiene `ObtenerUltimaPorPanelAsync`; si `AlertaPanel.DebeAbrirse(ultima, panel.FechaHora)` es `true`, crea la alerta con `AlertaPanel.Abrir` (con los nombres y códigos de la lectura) y la agrega con `AgregarAsync`;
     - si el panel no está `EnFalla` y tiene alerta activa: llama a `Cerrar(panel.FechaHora)`.
  5. Para cada alerta activa cuyo panel no aparece en la lectura llama a `Cerrar(lectura.FechaHora.Value)`.
  6. Si se abrió o cerró alguna alerta: `SaveChangesAsync`, registra una línea informativa con las cantidades abiertas y cerradas y llama a `IAlertasNotifier`. Un fallo del notificador se captura y se registra con `LogDebug`, como en `TelemetriaService`.
  7. Sin cambios no escribe en la base ni notifica.
- Una consulta por lectura para las activas, más una por cada panel que pasa a falla.

**`IAlertasService` / `AlertasService`** (scoped):

```csharp
Task<ListadoAlertasDto> ObtenerListadoAsync(CancellationToken cancellationToken = default);
Task<int> ContarActivasAsync(CancellationToken cancellationToken = default);
```

- Dependencias: `IAlertaPanelRepository` y `TimeProvider`.
- Constante `CantidadHistorial = 50`.
- `ObtenerListadoAsync`: obtiene las activas y las `CantidadHistorial` cerradas más recientes, y las mapea a `AlertaPanelDto` calculando `Duracion` con `CalcularDuracion(_tiempo.GetUtcNow().UtcDateTime)`.
- `ContarActivasAsync`: delega en `IAlertaPanelRepository.ContarActivasAsync`.

**`ITelemetriaService` / `TelemetriaService`** (cambio): se agrega la dependencia `IRegistroAlertasService`, inyectada después de `IRegistroHistoricoService`. `RegistrarLecturaAsync` llama a `RegistroAlertasService.RegistrarLecturaAsync` inmediatamente después del registro histórico y antes de `IAlmacenTelemetria.GuardarAsync`. La interfaz y el registro scoped no cambian.

## 3) Infraestructura (`SolarUY.Infrastructure`)

### DbSet y configuraciones

`SolarUYDbContext` agrega:

```csharp
public DbSet<AlertaPanel> AlertasPanel => Set<AlertaPanel>();
```

`SolarUY/Infrastructure/Persistence/Configurations/AlertaPanelConfiguration.cs`:

- Tabla `AlertasPanel`.
- `Id` como clave, `ValueGeneratedNever()`.
- `ParqueSolarId`, `NodoId`, `PanelId`, `NombreParque`, `CodigoNodo`, `CodigoSerie` y `FechaInicioUtc` requeridos; `FechaFinUtc` opcional.
- Longitudes: `NombreParque` 100, `CodigoNodo` 50, `CodigoSerie` 50 (constantes de la entidad).
- FK a `ParqueSolar`, `Nodo` y `Panel` con `OnDelete(DeleteBehavior.Restrict)`, sin navegaciones, igual que `ResumenDiarioPanelConfiguration`.
- Índices: `IX_AlertasPanel_PanelId_FechaInicioUtc` sobre `(PanelId, FechaInicioUtc)` y `IX_AlertasPanel_FechaFinUtc` sobre `FechaFinUtc`. Ninguno es único. EF Core agrega además, por convención, los índices de las claves foráneas `IX_AlertasPanel_NodoId` e `IX_AlertasPanel_ParqueSolarId`.
- Sin filtros de consulta globales.

### Repositorios

`SolarUY/Infrastructure/Repositories/AlertaPanelRepository.cs` implementa `IAlertaPanelRepository` con `SolarUYDbContext` inyectado por constructor y los criterios descritos en el contrato.

### Migración

Cambia el esquema. Migración **`AgregarAlertasPanel`**: crea la tabla `AlertasPanel`, sus tres claves foráneas y los dos índices. Se genera con:

```bash
docker compose run --rm tools bash -lc "dotnet restore SolarUY.sln && dotnet ef migrations add AgregarAlertasPanel --project SolarUY --startup-project SolarUY"
```

(el `restore` previo es necesario cuando los `obj` quedaron restaurados desde Windows, como hace el servicio `migrate`). Al regenerarse, el snapshot del modelo pasa a registrar `FechaLocal` de los resúmenes diarios como `DateTime` (el tipo que ya usa su conversor) en lugar de `DateOnly`; es un ajuste del snapshot sin cambios de esquema.

Se aplica con el servicio `migrate` del stack.

### Registro de dependencias (`Program.cs`)

```csharp
builder.Services.AddScoped<IAlertaPanelRepository, AlertaPanelRepository>();
builder.Services.AddScoped<IRegistroAlertasService, RegistroAlertasService>();
builder.Services.AddScoped<IAlertasService, AlertasService>();
builder.Services.AddScoped<IAlertasNotifier, SolarUY.Hubs.SignalRAlertasNotifier>();
```

Se agregan junto a los registros de históricos y del dashboard. `TimeProvider` y `AddSignalR()` ya están registrados. No hay cambios en `appsettings.json`.

## 4) Presentación (`SolarUY.Controllers` + `Views`)

### Controlador y acciones

`SolarUY/Controllers/AlertasController.cs`, sin `[Authorize]`, inyecta `IAlertasService`.

- `GET Index(CancellationToken)`: obtiene `ListadoAlertasDto` y devuelve la vista.
- `GET CantidadActivas(CancellationToken)`: devuelve `Json(new { cantidadActivas })`.
- No hay acciones de escritura. El controlador no contiene lógica de negocio.

`SolarUY/Hubs/SignalRAlertasNotifier.cs` (namespace `SolarUY.Hubs`) implementa `IAlertasNotifier`: inyecta `IHubContext<TelemetriaHub>` y emite `Clients.All.SendAsync("ActualizarAlertas")`. No se crea un hub nuevo.

### Autorización

Acceso anónimo y de solo lectura (rol: Todos), igual que el Dashboard: sin `[Authorize]` en `AlertasController` ni en el hub. No hay formularios, por lo que no se usa antiforgery.

### Vistas

- `SolarUY/Views/Alertas/Index.cshtml` (modelo `ListadoAlertasDto`):
  - título "Alertas de paneles";
  - sección "Alertas activas" con el total; si no hay, un aviso `alert-success` con "Sin alertas activas."; si hay, una tabla con Parque Solar, Nodo, Panel, Inicio (UTC) y Duración;
  - sección "Historial (últimas 50 alertas cerradas)"; si no hay, el texto "Todavía no hay alertas cerradas."; si hay, una tabla con Parque Solar, Nodo, Panel, Inicio (UTC), Fin (UTC) y Duración;
  - fechas con formato `yyyy-MM-dd HH:mm:ss` y duración como `horas:minutos:segundos`.
- `SolarUY/wwwroot/js/alertas-indicador.js`: función autoejecutable que consulta `/Alertas/CantidadActivas`, actualiza el contador del menú (oculto cuando es 0) y, si `signalR` está disponible, abre una conexión a `/hubs/telemetria` con reconexión automática que vuelve a consultar al recibir `ActualizarAlertas` y al reconectar. Los errores de red se ignoran sin romper la página.
- `SolarUY/Views/Shared/_Layout.cshtml`: enlace "Alertas" visible para todos, con un `<span id="alertasContador" class="badge bg-danger d-none">`, y, antes de `RenderSectionAsync("Scripts")`, el script del cliente de SignalR (mismo CDN, versión e integridad que el Dashboard) y `~/js/alertas-indicador.js`.

## 5) Pruebas

Tests xUnit del Dominio en `SolarUY.Tests/Domain/AlertaPanelTests.cs`:

- `Abrir`: datos válidos (queda activa, `Id` no vacío, trim de textos) y rechazo de identificadores vacíos, textos vacíos o con espacios, textos que superan la longitud máxima y fecha `default`.
- `Cerrar`: cierra y devuelve `true`; con fin igual o anterior al inicio devuelve `false` y no modifica; con fecha `default` lanza `ArgumentException`; sobre una alerta cerrada lanza `InvalidOperationException`.
- `CalcularDuracion`: alerta cerrada, alerta activa hasta `ahoraUtc`, resultado nunca negativo y `ahoraUtc` `default` lanza `ArgumentException`.
- `DebeAbrirse`: sin alerta previa, con previa activa, con previa cerrada y fecha posterior, igual y anterior a su fin, y fecha `default`.

No se agregan tests en Application, Infrastructure ni Presentation.

## 6) Calidad y verificación

- [ ] Build: `docker compose run --rm tools dotnet build SolarUY.sln`
- [ ] Tests: `docker compose run --rm tools dotnet test SolarUY.sln`
- [ ] ArchUnitNET en verde
- [ ] SonarQube ejecutado y hallazgos revisados
- [ ] Migración `AgregarAlertasPanel` aplicada por `migrate` y tabla `AlertasPanel` creada
- [ ] Con `Simulador__ProbabilidadFalla=1`, aparecen alertas activas en `/Alertas` y el indicador del menú sube sin recargar
- [ ] Al recuperarse los paneles, las alertas pasan al historial con fecha de fin y duración, y el indicador baja sin recargar
- [ ] Una lectura repetida no duplica alertas (reenviando la misma lectura)
- [ ] Un nodo que pasa a `EnMantenimiento` cierra las alertas de sus paneles
- [ ] `/Alertas` y `/Alertas/CantidadActivas` responden `200` sin sesión
- [ ] No se modificó `CodeQuality.Tests`, el simulador ni el contrato de telemetría

## 7) Fuera de alcance

- Acuse de recibo o atención manual de alertas, y cualquier acción de escritura sobre ellas.
- Notificaciones por correo electrónico, SMS u otros canales.
- Alertas de nodos, parques o inversores, y alertas por umbrales de potencia o de tiempo.
- Filtros, paginación, búsqueda y exportación del historial; retención y purga.
- Actualización en vivo del listado de la pantalla de alertas (solo se actualiza el indicador del menú).
- Mostrar las fechas en hora de Uruguay.
- Servir el cliente de SignalR desde `wwwroot` (se mantiene el CDN del Dashboard).
- Cambios en el simulador, el contrato de telemetría, el CRUD, la autenticación y los roles.
