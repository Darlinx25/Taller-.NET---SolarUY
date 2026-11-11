# Especificación Técnica: Dashboard de telemetría en vivo

## Objetivo

Implementar un dashboard de monitoreo en tiempo real para visualizar la generación de energía y el estado de salud de Parques Solares, Nodos y Paneles, consumiendo la telemetría ya existente vía **SignalR**, con carga inicial por MVC y actualizaciones en vivo. Acceso por ruta, sin restricción por rol, siguiendo Clean Architecture y las convenciones del proyecto.

## Decisiones y supuestos

- Carpeta del requerimiento: `ControlCalidad/prompting/req008_dashboard/`
- Acceso: **por ruta**. Acción propuesta: `GET /Dashboard/Index`.
- Presentación: **MVC** (`Views/Dashboard/Index.cshtml`) + **SignalR** para actualizaciones en tiempo real.
- Hub: `TelemetriaHub` en `SolarUY/Hubs/` respetando convenciones (clase que hereda de `Microsoft.AspNetCore.SignalR.Hub`).
- Reutilizar el contrato existente: `SolarUY.Application.Dtos.TelemetriaDto`, `SolarUY.Application.Services.ITelemetriaService`, `SolarUY.Application.Services.IAlmacenTelemetria`.
- **Sin restricción por rol**. No se aplica `[Authorize(Roles = "Administrador")]`. Debe ser accesible sin restricción por rol.
- **Ambas formas**: carga inicial desde controlador vía servicio de Application (MVC "vista tonta") + suscripción al Hub `TelemetriaHub` para recibir actualizaciones en tiempo real cuando llegue una nueva lectura.
- Dominio **no** debe tener referencias a SignalR ni a ASP.NET Core.
- Infraestructura **no** debe ser accedida directamente desde Controllers/Views. Solo vía Application.
- Migración: **NO**. No se agregan entidades, ni DbSets, ni cambios de esquema. Se reutiliza telemetría + topología existente (`ParqueSolar`, `Nodo`, `Panel`).
- La jerarquía se construye desde la topología registrada: parques, nodos y paneles.
- La última lectura de telemetría se cruza por `ParqueSolarId`, `NodoId` y `PanelId`.
- Si falta telemetría para un parque/nodo/panel, se muestran valores `0` y estado `Sin telemetría`.
- `TieneDatos` indica si existe una lectura de telemetría, pero la topología se renderiza igual.
- La actualización en vivo se resuelve con SignalR + un endpoint MVC que devuelve el partial actualizado. No se hace `window.location.reload()`.
- Respetar decisiones de arquitectura tomadas en `Iteracion 1 Grupo 2 .NET.pdf`.

## 1) Dominio (`SolarUY.Domain`)

### Entidades
- Sin cambios. Se reutilizan `ParqueSolar`, `Nodo`, `Panel`, `Inversor` tal como están modeladas.

### Validaciones
- Sin cambios. Las validaciones siguen correspondiendo al modelo existente.

### Excepciones
- Sin nuevas excepciones de dominio requeridas para este requerimiento.

### Repositorios (contratos)
- Sin nuevos contratos obligatorios. Si se necesitara consultar topología para armar la vista, se deberá usar los repositorios/servicios ya existentes. No crear contratos nuevos sin necesidad estrictamente comprobada contra el código actual.

## 2) Aplicación (`SolarUY.Application`)

### DTOs
Crear DTOs de vista para el dashboard (en `SolarUY.Application.Dtos/`):

| DTO | Ubicación | Descripción |
|---|---|---|
| `DashboardTelemetriaViewDto` | `Application/Dtos/` | Agregado para la vista. Debe incluir: `FechaHora`, `Parques` (lista), indicador `TieneDatos`, `MensajeSinDatos`. |
| `DashboardParqueDto` | `Application/Dtos/` | Parque con `Id`, `Nombre`, `Ubicacion`, `GeneracionTotal`, `Nodos`. |
| `DashboardNodoDto` | `Application/Dtos/` | Nodo con `Id`, `Codigo`, `Generacion`, `Paneles`. |
| `DashboardPanelDto` | `Application/Dtos/` | Panel con `Id`, `CodigoSerie`, `Marca`, `Modelo`, `PotenciaWp`, `Generacion`, `EstadoSalud` (texto), `EsFallo` (bool). Debe mostrar estado de salud textual + indicador visual. |

### Servicios y casos de uso

| Elemento | Tipo | Ubicación | Descripción |
|---|---|---|---|
| `IDashboardService` | Interfaz | `Application/Services/` | Caso de uso para obtener el estado inicial del dashboard (topología + última lectura). |
| `DashboardService` | Implementación | `Application/Services/` | Orquesta la topología registrada y la combina con `ITelemetriaService.ObtenerUltimaLecturaAsync(...)`. Lógica puramente de Application. Sin acceso directo a `DbContext`. |

`DashboardService` orquesta:
- Obtiene todos los parques con `IParqueSolarRepository.ObtenerTodosAsync`.
- Obtiene los nodos de cada parque con `INodoRepository.ObtenerPorParqueAsync`.
- Obtiene los paneles de cada nodo con `IPanelRepository.ObtenerPorNodoAsync`.
- Obtiene la última lectura con `ITelemetriaService.ObtenerUltimaLecturaAsync`.
- Cruza telemetría por IDs y arma la jerarquía completa.
- Usa `Marca`, `Modelo` y `PotenciaWp` desde la entidad `Panel`.
- Usa generación y estado desde la telemetría cuando existen; si no, generación `0` y estado `Sin telemetría`.
- Calcula la generación total del parque con la telemetría del parque o, si no existe, con la suma de sus nodos.
- No accede a `DbContext`, SignalR ni Infrastructure.

**Reglas:** Inyección por constructor. Solo depende de `Domain`/contratos de Application. No referencia `Infrastructure` ni `Hubs`/SignalR.

## 3) Infraestructura (`SolarUY.Infrastructure`)

### DbSet y configuraciones
- Sin cambios.

### Repositorios
- Sin cambios obligatorios. Se reutilizan los existentes.

### Migración
**NO**. No hay cambios de esquema. Justificar: se reutiliza `TelemetriaDto` y entidades/topología ya existentes.

### Registro de dependencias (`Program.cs`)
Agregar registros en `Program.cs`:

| Registro | Tipo | Notas |
|---|---|---|
| `AddSignalR()` | Servicio | Habilitar SignalR. |
| `IDashboardService` -> `DashboardService` | Scoped | Registrar con `AddScoped` siguiendo el patrón existente. |
| `MapHub<TelemetriaHub>("/hubs/telemetria")` | Endpoint | Mapear Hub en el pipeline. |

No modificar registros existentes de telemetría.

## 4) Presentación (`SolarUY.Controllers` + `Views` + `Hubs`)

### Controlador y acciones
| Elemento | Ubicación | Descripción |
|---|---|---|
| `DashboardController` | `Controllers/` | Controlador MVC. Acciones `Index()` y `Contenido()` (GET). Ambas son "vistas tontas": inyectan `IDashboardService`, obtienen `DashboardTelemetriaViewDto` y lo pasan a la vista o partial. Sin lógica de negocio/cálculos complejos. |

### Hubs (SignalR)
| Elemento | Ubicación | Descripción |
|---|---|---|
| `TelemetriaHub` | `Hubs/` | Hereda `Hub`. Ubicado en capa Presentation. Respetar convenciones de nomenclatura. **Sin restricción por rol**. |

**Mecanismo de broadcast:** Cuando `ITelemetriaService.RegistrarLecturaAsync` recibe y guarda una lectura válida, se notifica a los clientes conectados al `TelemetriaHub` mediante el evento `ActualizarTelemetria`. La notificación se hace respetando las fronteras de arquitectura (inyectando `IHubContext<TelemetriaHub>` únicamente donde corresponda). El evento indica que hay una nueva lectura; la vista solicita el estado consolidado al endpoint de partial.

### Autorización
- **Sin restricción por rol.** No agregar `[Authorize(Roles = "Administrador")]` al controlador ni al Hub.

### Vistas
| Elemento | Ubicación | Descripción |
|---|---|---|
| `Index.cshtml` | `Views/Dashboard/` | Vista MVC principal. Contiene el layout, el aviso de sin telemetría y el script SignalR. |
| `_DashboardContenido.cshtml` | `Views/Dashboard/` | Partial con la jerarquía Parque -> Nodo -> Panel, generación por nivel y estado de salud textual + indicador visual. Se reutiliza en la carga inicial y en la actualización en vivo. |

Al recibir `ActualizarTelemetria`, el cliente hace `fetch('/Dashboard/Contenido')` y reemplaza el HTML de `#dashboardLive`. No se recarga la página completa.

## 5) Pruebas

- No se agregan tests de Dominio porque este requerimiento no modifica entidades ni reglas de Dominio.
- No se agregan tests en Application, Infrastructure ni Presentation.

## 6) Calidad y verificación

- [ ] Build: `docker compose run --rm tools dotnet build SolarUY.sln`
- [ ] Tests: `docker compose run --rm tools dotnet test SolarUY.sln`
- [ ] ArchUnitNET en verde
- [ ] SonarQube ejecutado y hallazgos revisados
- [ ] Prueba manual del flujo completo

## 7) Fuera de alcance

- Modificar `/api/simulacion/*`
- Cambiar ingesta/validación de telemetría
- Notificaciones por correo/SMS
- Histórico/gráficas históricas
- Nuevos sensores/inversores
- Restricción por rol

## 8) Adenda: gráficas del dashboard (agregado posterior)

### Objetivo de la adenda

Agregar cuatro gráficas al dashboard existente sin alterar la topología, la
ingesta de telemetría ni la pantalla de Histórico:

1. Generación actual por parque (barras).
2. Distribución del estado de los paneles (dona).
3. Generación por nodo (barras horizontales).
4. Tendencia compacta de energía diaria de los últimos 7 días (línea/área).

Esta adenda se agrega a la especificación anterior; no reemplaza ninguna de sus
decisiones.

### Decisiones y supuestos de la adenda

- Se reutiliza **Chart.js** ya incluido en el proyecto
  (`SolarUY/wwwroot/lib/chart.js/chart.umd.min.js`). No se agrega una librería
  nueva ni se usa un CDN para las gráficas.
- Las gráficas 1 a 3 usan la **última lectura de telemetría** y la topología ya
  existente. No requieren persistencia nueva.
- La gráfica 4 reutiliza `IHistoricoService` y los resúmenes diarios existentes.
  No se crea una nueva tabla ni una serie temporal intradía.
- No hay migración ni cambios de entidades, DbSets o repositorios.
- No se modifica el contrato de ingesta ni el simulador.
- El dashboard sigue **sin restricción por rol**.
- La agregación de datos para las gráficas se realiza en **Application**. Las
  vistas solo renderizan y configuran Chart.js.
- El histórico **no se consulta en cada evento SignalR**. La tendencia se carga
  con `Index` y, si se refresca, se hace con una frecuencia baja (por ejemplo,
  cada 5 minutos) o al recargar la página.
- No se usa `EnergiaAcumuladaKWh` de la telemetría como “energía del día”: el
  simulador la acumula desde su arranque. Para la tendencia se usan los
  resúmenes diarios.

### Dominio (`SolarUY.Domain`)

- Sin cambios.

### Aplicación (`SolarUY.Application`)

#### DTOs adicionales

| DTO | Ubicación | Descripción |
|---|---|---|
| `DashboardBarraDto` | `Application/Dtos/` | Par etiqueta/valor para gráficas de barras. |
| `DashboardDistribucionEstadosDto` | `Application/Dtos/` | Conteos de paneles `Operativos`, `EnFalla`, `SinTelemetria` y `TotalPaneles` (para que la vista no sume). |
| `DashboardTendenciaEnergiaDto` | `Application/Dtos/` | Puntos de la tendencia de 7 días y bandera `TieneDatos`. Reutiliza `PuntoGraficoHistoricoDto`. |
| `DashboardPaginaDto` | `Application/Dtos/` | Agregado de la página: `Dashboard` (`DashboardTelemetriaViewDto`) y `Tendencia` (`DashboardTendenciaEnergiaDto`). |

Extender `DashboardTelemetriaViewDto` con:

- `IReadOnlyList<DashboardBarraDto> GeneracionPorParque`
- `IReadOnlyList<DashboardBarraDto> GeneracionPorNodo`
- `DashboardDistribucionEstadosDto DistribucionEstadosPaneles`

#### Servicios

`IDashboardService` / `DashboardService`:

- `ObtenerDashboardAsync(...)` sigue devolviendo el estado en vivo y ahora también
  calcula:
  - `GeneracionPorParque`: una barra por parque usando `GeneracionTotal`.
  - `GeneracionPorNodo`: una barra por nodo con etiqueta
    `"<Parque> / <Nodo>"` y valor `Generacion`.
  - `DistribucionEstadosPaneles`: recorre todos los paneles del modelo y cuenta:
    - `EnFalla` si el panel está en falla.
    - `SinTelemetria` si el estado es `Sin telemetría` o `Desconocido`.
    - `Operativos` en el resto de los casos.
- Agregar `ObtenerTendenciaEnergiaAsync(...)`:
  - Inyecta `IHistoricoService` por constructor.
  - Arma un `FiltroHistoricoDto` con:
    - `TipoEntidad = TipoEntidadHistorico.Parque`
    - `EntidadId = null`
    - `Desde = hoy local - 6 días`
    - `Hasta = hoy local`
    - `Agrupacion = AgrupacionHistorico.Dia`
  - Llama a `IHistoricoService.ConsultarAsync(...)`.
  - Devuelve `DashboardTendenciaEnergiaDto` con `Puntos` y
    `TieneDatos = Puntos.Count > 0`.
- `DashboardService` no accede a `DbContext` ni a Infrastructure.
- No se agregan tests de Application según las reglas del proyecto.

### Infraestructura (`SolarUY.Infrastructure`)

- Sin cambios. `IHistoricoService` ya está registrado y los repositorios
  existentes alcanzan.
- Sin migración.

### Presentación (`SolarUY.Controllers` + `Views` + JavaScript)

`DashboardController`:

- `Index()` obtiene `DashboardTelemetriaViewDto` y
  `DashboardTendenciaEnergiaDto`, y pasa un `DashboardPaginaDto` a la vista.
- `Contenido()` sigue devolviendo solo `DashboardTelemetriaViewDto` para las
  actualizaciones en vivo.
- Opcional para refresco de la tendencia: `GET /Dashboard/Tendencia` devuelve
  `DashboardTendenciaEnergiaDto` como JSON. Si no se implementa, la tendencia se
  actualiza al recargar la página.

Vistas:

- `Index.cshtml`:
  - Carga `~/lib/chart.js/chart.umd.min.js` con `asp-append-version="true"`.
  - Incluye la tarjeta de tendencia de 7 días.
  - Inicializa la gráfica histórica una sola vez.
- `_DashboardContenido.cshtml`:
  - Incluye las tres gráficas en vivo.
  - Serializa los datos con `System.Text.Json` en un `<script type="application/json">`.
  - Muestra un estado vacío por gráfica cuando no haya datos.
- El JavaScript del dashboard debe:
  - Destruir las instancias de Chart.js antes de reemplazar `#dashboardLive`.
  - Volver a inicializar las tres gráficas en vivo después de cada reemplazo.
  - No inicializar Chart.js si el canvas o los datos no existen.
  - Usar `maintainAspectRatio: false` y alturas fijas para que las gráficas sean
    responsivas.
  - Usar colores de Bootstrap: éxito para operativo, peligro para falla, gris
    para sin telemetría.

Sugerencia de distribución visual:

- Fila de tarjetas KPI existente.
- Fila de dos columnas: generación por parque + estado de paneles.
- Fila de dos columnas: generación por nodo + tendencia de 7 días.
- Tablas de topología existentes debajo.

### Pruebas

- No se agregan tests de Dominio porque no hay comportamiento nuevo en
  `SolarUY.Domain`.
- No se agregan tests en Application, Infrastructure ni Presentation.
- La verificación es manual, además de build, tests y ArchUnitNET.

### Calidad y verificación de la adenda

- [ ] Build: `docker compose run --rm tools dotnet build SolarUY.sln`
- [ ] Tests: `docker compose run --rm tools dotnet test SolarUY.sln`
- [ ] ArchUnitNET en verde
- [ ] SonarQube ejecutado y hallazgos revisados
- [ ] Prueba manual con datos y sin datos
- [ ] Verificar actualización SignalR sin romper las gráficas
- [ ] Verificar responsive y acceso sin rol

### Fuera de alcance de la adenda

- Pantalla completa de históricos o filtros avanzados dentro del dashboard.
- Serie intradía de los últimos minutos.
- Nuevas tablas, entidades, migraciones o repositorios.
- Cambios en `/api/simulacion/*` o en el simulador.
- Restricción del dashboard por rol.
