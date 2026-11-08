# Especificacion Tecnica: Mejoras visuales y de experiencia de usuario

## Objetivo

Mejorar la experiencia de uso de todo el sitio SolarUY sin cambiar reglas de
negocio ni contratos existentes. El cambio se concentra en la capa de
Presentacion: navegacion, jerarquia visual, acciones directas, estados vacios,
mensajes al usuario y formularios. El flujo Parque Solar -> Nodo -> Panel pasa a
ser un "hub" navegable donde cada nivel muestra y permite gestionar a sus hijos
sin obligar a entrar a pantallas intermedias.

## Decisiones y supuestos

- El requerimiento es exclusivamente de Presentacion. No se modifican
  `SolarUY.Domain`, `SolarUY.Application` ni `SolarUY.Infrastructure`.
- No se crean nuevos servicios ni metodos en `Application`: se reutilizan
  `IParqueSolarService`, `INodoService` y `IPanelService` ya existentes.
- No hay cambios de esquema ni migraciones.
- No se agregan librerias externas. Se usa Bootstrap 5, jQuery, jQuery
  Validation, Chart.js y SignalR ya presentes en `wwwroot/lib` y en el layout.
- No se usan paquetes de iconos. Si se necesitan iconos, se usan SVG inline
  locales definidos en las vistas o partials.
- Se mantiene la autorizacion actual: `[Authorize(Roles = "Administrador")]` en
  Parques, Nodos, Paneles y Tareas de mantenimiento; `[Authorize]` en
  Historico; Dashboard y Alertas siguen sin cambios de autorizacion.
- El alta y edicion de nodos y paneles se hace en paginas dedicadas (no en
  modales), con botones directos desde los listados y desde el hub.
- Se agrega un parametro opcional `returnUrl` a las acciones de alta, edicion y
  baja de nodos y paneles para volver al contexto de origen. Se valida con
  `Url.IsLocalUrl` para evitar redirecciones abiertas.
- No se modifican los archivos `README.md` ni `ControlCalidad/README.md` por
  pedido explicito del usuario.
- El hub del parque carga los paneles de cada nodo con
  `IPanelService.ObtenerPanelesPorNodoAsync`. Se acepta el patron N+1 para el
  volumen de datos del taller; no se agregan metodos nuevos a `Application`.
- No se modifica `ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests`.

## 1) Dominio (`SolarUY.Domain`)

### Entidades

Sin cambios. No se agregan propiedades, metodos ni validaciones.

### Validaciones

Sin cambios. Las validaciones actuales de entidades y DTOs se mantienen.

### Excepciones

Sin cambios.

### Repositorios (contratos)

Sin cambios.

## 2) Aplicacion (`SolarUY.Application`)

### DTOs

Sin cambios. No se modifican los DTOs existentes ni se crean DTOs nuevos.

### Servicios y casos de uso

Sin cambios. La Presentacion reutiliza los servicios existentes:

- `IParqueSolarService.ObtenerParqueAsync`
- `INodoService.ObtenerNodosPorParqueAsync`
- `INodoService.ObtenerNodoAsync`
- `IPanelService.ObtenerPanelesPorNodoAsync`
- `IPanelService.ObtenerPanelAsync`

Las agregaciones visuales (cantidad de nodos, cantidad de paneles y potencia
total instalada en Wp) se calculan en los view models de Presentacion, no en
Razor ni en los controladores.

## 3) Infraestructura (`SolarUY.Infrastructure`)

### DbSet y configuraciones

Sin cambios.

### Repositorios

Sin cambios.

### Migracion

No se necesita migracion: no hay cambios de esquema.

### Registro de dependencias (`Program.cs`)

Sin cambios. `INodoService` e `IPanelService` ya estan registrados y se inyectan
en los controladores por constructor.

## 4) Presentacion (`SolarUY.Controllers` + `Views`)

### Modelos de presentacion (`SolarUY.Models`)

Se agregan las siguientes clases, todas en el namespace `SolarUY.Models` y sin
dependencias de `Infrastructure`:

- `BreadcrumbItem`
  - `string Texto`
  - `string? Url` (null para el elemento activo)
- `EmptyStateViewModel`
  - `string Mensaje`
  - `string? TextoAccion`
  - `string? UrlAccion`
- `NodoConPanelesViewModel`
  - `NodoDetalleDto Nodo`
  - `IReadOnlyList<PanelDto> Paneles`
  - `int CantidadPaneles => Paneles.Count`
  - `decimal PotenciaTotalWp => Paneles.Sum(p => p.PotenciaWp)`
- `ParqueSolarDetalleViewModel`
  - `ParqueSolarDto Parque`
  - `IReadOnlyList<NodoConPanelesViewModel> Nodos`
  - `int CantidadNodos => Nodos.Count`
  - `int CantidadPaneles => Nodos.Sum(n => n.CantidadPaneles)`
  - `decimal PotenciaTotalWp => Nodos.Sum(n => n.PotenciaTotalWp)`
- `NodoDetalleViewModel`
  - `NodoDetalleDto Nodo`
  - `IReadOnlyList<PanelDto> Paneles`
  - `int CantidadPaneles => Paneles.Count`
  - `decimal PotenciaTotalWp => Paneles.Sum(p => p.PotenciaWp)`

### Controlador y acciones

#### `ParquesSolaresController`

- Constructor con `IParqueSolarService`, `INodoService` e `IPanelService`,
  inyectados por constructor y validados con `ArgumentNullException`.
- `Details(Guid id, CancellationToken cancellationToken)`:
  - Obtiene el parque con `ObtenerParqueAsync`. Si no existe, `NotFound()`.
  - Obtiene los nodos con `ObtenerNodosPorParqueAsync`.
  - Por cada nodo, obtiene sus paneles con `ObtenerPanelesPorNodoAsync`.
  - Construye `ParqueSolarDetalleViewModel` y lo pasa a la vista.
- `Index`, `Create`, `Edit` y `Delete` mantienen su comportamiento actual; solo
  cambian sus vistas.

#### `NodosController`

- Constructor con `INodoService`, `IParqueSolarService` e `IPanelService`,
  inyectados por constructor y validados con `ArgumentNullException`.
- `Details(Guid id, CancellationToken cancellationToken)`:
  - Obtiene el nodo con `ObtenerNodoAsync`. Si no existe, `NotFound()`.
  - Obtiene sus paneles con `ObtenerPanelesPorNodoAsync`.
  - Construye `NodoDetalleViewModel` y lo pasa a la vista.
- `Create(Guid parqueId, string? returnUrl, CancellationToken)`:
  - Agrega `ViewBag.ReturnUrl = returnUrl`.
  - Mantiene el alta de nodo e inversor y las validaciones actuales.
- `Create(CrearNodoInversorDto dto, string? returnUrl, CancellationToken)`:
  - Si el modelo es invalido, vuelve a la vista con `ViewBag.ReturnUrl`.
  - Si la operacion es exitosa y `Url.IsLocalUrl(returnUrl)` es verdadero,
    redirige a `returnUrl`; en caso contrario mantiene el
    `RedirectToAction(nameof(Index), new { parqueId = dto.ParqueSolarId })`.
- `Edit(Guid id, string? returnUrl, CancellationToken)` y
  `Edit(EditarNodoInversorDto dto, string? returnUrl, CancellationToken)`:
  mismo criterio de `returnUrl` para volver al origen.
- `Delete(Guid id, string? returnUrl, CancellationToken)` y
  `DeleteConfirmed(Guid id, string? returnUrl, CancellationToken)`:
  mismo criterio de `returnUrl` despues de eliminar.
- `Index` mantiene su comportamiento actual; solo cambia su vista.

#### `PanelesController`

- `Create(Guid nodoId, string? returnUrl, CancellationToken)` y
  `Create(CrearPanelDto dto, string? returnUrl, CancellationToken)`:
  agregan `ViewBag.ReturnUrl` y redirigen a `returnUrl` si es local; si no,
  mantienen el `RedirectToAction(nameof(Index), new { nodoId = dto.NodoId })`.
- `Edit(Guid id, string? returnUrl, CancellationToken)` y
  `Edit(EditarPanelDto dto, string? returnUrl, CancellationToken)`:
  mismo criterio de `returnUrl`.
- `Delete(Guid id, string? returnUrl, CancellationToken)` y
  `DeleteConfirmed(Guid id, string? returnUrl, CancellationToken)`:
  mismo criterio de `returnUrl` despues de eliminar.
- `Index` y `Details` mantienen su comportamiento actual; solo cambian sus
  vistas.

#### Otros controladores

- `HomeController`, `DashboardController`, `AlertasController`,
  `HistoricoController` y `TareasMantenimientoController` no cambian sus
  acciones. Solo cambian sus vistas y el estilo compartido.

### Autorizacion

Sin cambios. Se mantienen los atributos actuales:

- `ParquesSolaresController`, `NodosController`, `PanelesController` y
  `TareasMantenimientoController`: `[Authorize(Roles = "Administrador")]`.
- `HistoricoController`: `[Authorize]`.
- `DashboardController`, `AlertasController`, `HomeController`: sin cambios.

En la barra de navegacion, el enlace a Parques Solares se muestra solo a
usuarios con rol Administrador para evitar accesos denegados.

### Partials compartidos

Se agregan en `SolarUY/Views/Shared/`:

- `_Breadcrumbs.cshtml`
  - Modelo: `IReadOnlyList<BreadcrumbItem>`.
  - Renderiza un `<nav aria-label="breadcrumb">` con `<ol class="breadcrumb">`.
  - El ultimo elemento se muestra como activo, sin enlace.
- `_TempDataAlertas.cshtml`
  - Lee `TempData["Mensaje"]`, `TempData["Error"]` y `TempData["Info"]`.
  - Renderiza alertas Bootstrap `alert-dismissible` con `role="alert"`.
  - Los mensajes de exito se marcan con `data-auto-dismiss="true"`.
- `_EstadoBadge.cshtml`
  - Modelo: `string`.
  - Mapea el estado a una clase `text-bg-*` de Bootstrap:
    - `Activo`, `Operativo`, `Completada`: `text-bg-success`.
    - `Inactivo`, `Cancelada`, `Sin telemetria`: `text-bg-secondary`.
    - `EnFalla`, `En falla`, `Fallo`: `text-bg-danger`.
    - `EnProceso`, `En proceso`: `text-bg-info`.
    - `Pendiente`: `text-bg-warning`.
    - Cualquier otro valor: `text-bg-secondary`.
  - Si el estado es nulo o vacio, muestra `Sin estado`.
- `_EmptyState.cshtml`
  - Modelo: `EmptyStateViewModel`.
  - Renderiza un contenedor centrado con el mensaje y, si existe
    `TextoAccion` y `UrlAccion`, un boton primario.

### Vistas

Todas las vistas de gestion incluyen migas de pan mediante
`<partial name="_Breadcrumbs" model="..." />` y un encabezado de pagina
consistente con titulo, subtitulo y acciones.

#### `Views/Shared/_Layout.cshtml`

- Navbar con marca SolarUY, enlaces Inicio, Dashboard, Alertas, Parques Solares
  (solo Administrador) e Historico (autenticados).
- Enlace activo resaltado con la clase `active` y `aria-current="page"`, segun
  `ViewContext.RouteData.Values["controller"]`.
- Menu de usuario con nombre, badge de rol (Administrador u Operador) y boton
  Salir. Si no esta autenticado, muestra Iniciar sesion.
- Contenedor principal con `container-xl`.
- Incluye `_TempDataAlertas` antes de `@RenderBody()`.
- Footer simple y consistente.
- Mantiene las referencias a jQuery, Bootstrap, site.js, SignalR y
  alertas-indicador.js.

#### `wwwroot/css/site.css`

Se agregan clases propias, sin reemplazar las de Bootstrap:

- `.app-page-header`: separacion y alineacion del encabezado de pagina.
- `.app-page-subtitle`: texto secundario del encabezado.
- `.app-stat-card`, `.app-stat-value`: tarjetas de resumen.
- `.app-empty-state`: contenedor de estado vacio.
- `.app-action-bar`: barra de acciones sticky para formularios.
- `.app-table-actions`: alineacion y separacion de botones de accion.
- `.app-card-hover`: elevacion suave al pasar el mouse sobre tarjetas.
- `.app-context-banner`: banner de contexto jerarquico
  Parque -> Nodo -> Panel.

#### `wwwroot/js/site.js`

- Cierra automaticamente las alertas con `data-auto-dismiss="true"` a los 5
  segundos usando `bootstrap.Alert`.
- Enfoca el primer control con error de validacion
  (`.input-validation-error`) cuando existe.
- No contiene logica de negocio.

#### `Views/ParquesSolares/Index.cshtml`

- Encabezado con titulo, subtitulo y boton Registrar parque solar.
- Grilla responsive de tarjetas (`row row-cols-1 row-cols-md-2 g-3`), una por
  parque, con nombre, ubicacion, capacidad, fecha de alta y acciones:
  - Ver nodos (Details).
  - Agregar nodo (Nodos/Create con `parqueId` y `returnUrl` al detalle del
    parque).
  - Editar.
  - Eliminar.
- Estado vacio con `_EmptyState` y boton Registrar parque solar.

#### `Views/ParquesSolares/Details.cshtml`

- Modelo: `ParqueSolarDetalleViewModel`.
- Migas: Inicio / Parques solares / Nombre del parque.
- Encabezado con datos del parque y acciones Editar, Agregar nodo y Volver.
- Tarjetas de resumen: cantidad de nodos, cantidad de paneles y potencia total
  instalada en Wp.
- Acordeon Bootstrap de nodos. Cada nodo muestra:
  - Codigo, estado con `_EstadoBadge`, descripcion y datos del inversor.
  - Acciones: Detalle del nodo, Agregar panel (Paneles/Create con `nodoId` y
    `returnUrl` al detalle del nodo), Editar nodo y Eliminar nodo.
  - Tabla responsive de paneles del nodo con codigo/serie, marca, modelo,
    potencia y acciones Detalle, Editar, Eliminar y Tareas de mantenimiento.
- Si el parque no tiene nodos, estado vacio con boton Agregar nodo.

#### `Views/Nodos/Index.cshtml`

- Encabezado con nombre del parque, subtitulo y boton Nuevo nodo e inversor.
- Tarjetas o tabla responsive de nodos con codigo, estado con `_EstadoBadge`,
  inversor, descripcion y fecha de alta.
- Acciones por nodo: Ver paneles (Paneles/Index), Agregar panel
  (Paneles/Create con `nodoId` y `returnUrl` al detalle del nodo), Detalle,
  Editar y Eliminar.
- Estado vacio con boton Nuevo nodo e inversor.

#### `Views/Nodos/Details.cshtml`

- Modelo: `NodoDetalleViewModel`.
- Migas: Inicio / Parques solares / Nombre del parque / Codigo del nodo.
- Encabezado con codigo, estado, descripcion y acciones Editar, Agregar panel y
  Volver al parque.
- Tarjeta de inversor con marca, modelo, numero de serie y potencia nominal.
- Tarjetas de resumen: cantidad de paneles y potencia total instalada.
- Tabla responsive de paneles con acciones Detalle, Editar, Eliminar y Tareas
  de mantenimiento.
- Estado vacio con boton Agregar panel si no hay paneles.

#### `Views/Paneles/Index.cshtml`

- Encabezado con codigo del nodo, nombre del parque, subtitulo y boton Nuevo
  panel.
- Tarjetas o tabla responsive con codigo/serie, marca, modelo, potencia, fecha
  de alta y acciones Detalle, Editar, Eliminar y Tareas de mantenimiento.
- Estado vacio con boton Nuevo panel.

#### `Views/Paneles/Details.cshtml`

- Migas: Inicio / Parques solares / Nombre del parque / Codigo del nodo /
  Codigo/serie del panel.
- Tarjetas de datos del panel y de ubicacion.
- Acciones: Editar, Administrar tareas de mantenimiento, Volver al listado y
  Volver al nodo.

#### Formularios `Create`, `Edit` y `Delete`

- Parques, Nodos y Paneles usan un contenedor `card` centrado, grilla
  responsive y textos de ayuda.
- Nodos: secciones "Datos del nodo" y "Datos del inversor".
- Paneles: secciones "Datos del panel" e "Instalacion y observaciones".
- Barra de acciones sticky (`app-action-bar`) con boton principal y Cancelar.
- Los formularios incluyen un `<input type="hidden" name="returnUrl"
  value="@ViewBag.ReturnUrl" />` cuando corresponda.
- Las vistas de eliminacion usan un `card` con borde y texto de peligro, y
  muestran el contexto Parque -> Nodo -> Panel.
- Se conservan los `asp-validation-summary`, `asp-validation-for` y
  `_ValidationScriptsPartial` existentes.

#### `Views/Home/Index.cshtml`

- Hero con nombre SolarUY y descripcion breve.
- Tarjetas de acceso rapido:
  - Parques solares (solo Administrador).
  - Dashboard.
  - Alertas.
  - Historico (solo autenticados).
  - Iniciar sesion (solo no autenticados).

#### `Views/Dashboard/Index.cshtml` y `Views/Dashboard/_DashboardContenido.cshtml`

- Encabezado con indicador "En vivo" y ultima lectura.
- Mantener el contenedor `#dashboardLive` y el fetch a
  `Url.Action("Contenido", "Dashboard")` para no romper SignalR.
- `_DashboardContenido` muestra tarjetas por parque con generacion total y un
  acordeon de nodos. Cada nodo muestra su generacion y una tabla responsive de
  paneles con badge de salud (`_EstadoBadge`).
- Estado sin datos con `_EmptyState`.

#### `Views/Alertas/Index.cshtml`

- Tarjetas de resumen: alertas activas e historial.
- Tablas responsive con badges de estado y formato de duracion existente.
- Estado vacio para alertas activas e historial.

#### `Views/Historico/Index.cshtml`

- Formulario de filtros en `card`.
- Tarjetas KPI existentes con el nuevo estilo.
- Card del grafico y tabla responsive.
- Estado vacio cuando no hay datos.
- Se mantienen `chart.js` y `historico.js`.

#### `Views/TareasMantenimiento/*.cshtml`

- Banner de contexto Parque -> Nodo -> Panel.
- Migas de pan cuando el DTO o `ViewBag` permiten reconstruir el contexto.
- Badges de estado con `_EstadoBadge`.
- Tablas responsive y botones de accion consistentes.
- Se mantienen las reglas actuales de habilitacion de Editar y Cambiar estado.

#### `Views/DevAuth/AccesoDenegado.cshtml` y `Views/Shared/Error.cshtml`

- Se adaptan al estilo compartido con `card`, texto claro y boton de volver al
  inicio.

## 5) Pruebas

- Tests xUnit del Dominio (obligatorios): no se agregan porque no hay
  comportamiento nuevo ni modificado en `SolarUY.Domain`.
- No se agregan tests en Application, Infrastructure ni Presentation.
- Los tests existentes de `CodeQuality.Tests` (ArchUnitNET) deben seguir en
  verde. En particular, la Presentacion solo dependera de `SolarUY.Application`
  y nunca de `SolarUY.Infrastructure`.
- No se modifica ningun archivo de `CodeQuality.Tests`.

## 6) Calidad y verificacion

- [ ] Build: `docker compose run --rm tools dotnet build SolarUY.sln`
- [ ] Tests: `docker compose run --rm tools dotnet test SolarUY.sln`
- [ ] ArchUnitNET en verde
- [ ] SonarQube ejecutado y hallazgos revisados
- [ ] Prueba manual del flujo completo:
      Inicio -> Parques -> Agregar nodo desde el listado -> Detalle del parque
      con nodos y paneles -> Agregar panel desde el hub -> Editar/Eliminar ->
      Tareas -> Dashboard -> Alertas -> Historico.

## 7) Fuera de alcance

- Cambios en `Domain`, `Application` o `Infrastructure`.
- Cambios de esquema o migraciones.
- Nuevas funcionalidades de negocio.
- Librerias, frameworks de UI o paquetes de iconos externos.
- Cambios en autenticacion OIDC, MCP, simulacion o telemetria.
- Rediseno de marca o paleta corporativa nueva.
- Modificacion de `README.md` o `ControlCalidad/README.md`.
- Modificacion de `ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests`.
