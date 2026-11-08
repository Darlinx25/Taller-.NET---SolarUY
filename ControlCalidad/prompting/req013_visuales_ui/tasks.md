# Tareas: req013 Mejoras visuales y de experiencia de usuario

> Cada tarea debe ser verificable, indicar el archivo o la ruta cuando aplique
> y comenzar con `[ ]`. Marcar `[x]` solo cuando este terminada y verificada.

## 0. Preparacion

- [x] 0.1 Confirmar que `requirements.md`, `specs.md` y este plan estan
  aprobados.
- [x] 0.2 Revisar las vistas y controladores actuales de Parques, Nodos,
  Paneles, Dashboard, Alertas, Historico, Tareas, Home, Error y Acceso denegado
  para respetar nombres y patrones existentes.
- [x] 0.3 Confirmar que no se modificaran `README.md` ni
  `ControlCalidad/README.md` (restriccion explicita del usuario).

## 1. Dominio (`SolarUY.Domain`)

- [x] 1.1 Confirmar que no se requieren cambios en entidades, validaciones,
  excepciones ni repositorios. Verificar con `git diff -- SolarUY/Domain` que no
  haya cambios.

## 2. Tests del Dominio (`SolarUY.Tests/Domain`)

- [x] 2.1 Confirmar que no se agregan tests de Dominio porque no hay
  comportamiento nuevo ni modificado. Verificar con
  `git diff -- SolarUY.Tests` que no haya cambios.

## 3. Aplicacion (`SolarUY.Application`)

- [x] 3.1 Confirmar que no se modifican DTOs ni servicios. Verificar con
  `git diff -- SolarUY/Application` que no haya cambios.
- [x] 3.2 Confirmar que se reutilizan `IParqueSolarService`, `INodoService` e
  `IPanelService` desde los controladores.

## 4. Infraestructura (`SolarUY.Infrastructure`)

- [x] 4.1 Confirmar que no hay cambios de esquema ni migraciones. Verificar con
  `git diff -- SolarUY/Infrastructure SolarUY/Migrations` que no haya cambios.
- [x] 4.2 Confirmar que no se modifica `SolarUY/Program.cs` ni el registro de
  dependencias existente.

## 5. Presentacion (`SolarUY.Controllers` + `Views` + `Models` + `wwwroot`)

### 5.1 Modelos de presentacion

- [x] 5.1.1 Crear `SolarUY/Models/BreadcrumbItem.cs` con `Texto` y `Url`.
- [x] 5.1.2 Crear `SolarUY/Models/EmptyStateViewModel.cs` con `Mensaje`,
  `TextoAccion` y `UrlAccion`.
- [x] 5.1.3 Crear `SolarUY/Models/NodoConPanelesViewModel.cs` con `Nodo`,
  `Paneles`, `CantidadPaneles` y `PotenciaTotalWp`.
- [x] 5.1.4 Crear `SolarUY/Models/ParqueSolarDetalleViewModel.cs` con `Parque`,
  `Nodos`, `CantidadNodos`, `CantidadPaneles` y `PotenciaTotalWp`.
- [x] 5.1.5 Crear `SolarUY/Models/NodoDetalleViewModel.cs` con `Nodo`,
  `Paneles`, `CantidadPaneles` y `PotenciaTotalWp`.
- [x] 5.1.6 Verificar que los modelos solo dependan de
  `SolarUY.Application.Dtos` y no de `SolarUY.Infrastructure`.

### 5.2 Partials compartidos

- [x] 5.2.1 Crear `SolarUY/Views/Shared/_Breadcrumbs.cshtml` con modelo
  `IReadOnlyList<BreadcrumbItem>`.
- [x] 5.2.2 Crear `SolarUY/Views/Shared/_TempDataAlertas.cshtml` para
  `Mensaje`, `Error` e `Info` con alertas Bootstrap descartables.
- [x] 5.2.3 Crear `SolarUY/Views/Shared/_EstadoBadge.cshtml` con el mapeo de
  estados a `text-bg-*`.
- [x] 5.2.4 Crear `SolarUY/Views/Shared/_EmptyState.cshtml` con modelo
  `EmptyStateViewModel`.

### 5.3 Layout y estilos compartidos

- [x] 5.3.1 Actualizar `SolarUY/Views/Shared/_Layout.cshtml` con navbar,
  enlace activo, visibilidad por rol, menu de usuario, badge de rol,
  `_TempDataAlertas`, `container-xl` y footer.
- [x] 5.3.2 Verificar que el layout mantenga las referencias a jQuery,
  Bootstrap, `site.js`, SignalR y `alertas-indicador.js`.
- [x] 5.3.3 Agregar a `SolarUY/wwwroot/css/site.css` las clases
  `app-page-header`, `app-page-subtitle`, `app-stat-card`, `app-stat-value`,
  `app-empty-state`, `app-action-bar`, `app-table-actions`, `app-card-hover` y
  `app-context-banner`.
- [x] 5.3.4 Actualizar `SolarUY/wwwroot/js/site.js` para auto-cerrar alertas
  con `data-auto-dismiss="true"` y enfocar el primer
  `.input-validation-error`.

### 5.4 Controladores

- [x] 5.4.1 Actualizar `SolarUY/Controllers/ParquesSolaresController.cs`:
  inyectar `INodoService` e `IPanelService`, y construir
  `ParqueSolarDetalleViewModel` en `Details`.
- [x] 5.4.2 Actualizar `SolarUY/Controllers/NodosController.cs`: inyectar
  `IPanelService`, construir `NodoDetalleViewModel` en `Details`, y agregar
  `returnUrl` en `Create`, `Edit` y `Delete` con validacion `Url.IsLocalUrl`.
- [x] 5.4.3 Actualizar `SolarUY/Controllers/PanelesController.cs`: agregar
  `returnUrl` en `Create`, `Edit` y `Delete` con validacion `Url.IsLocalUrl`.
- [x] 5.4.4 Confirmar que los controladores solo dependan de
  `SolarUY.Application` y no de `SolarUY.Infrastructure`.
- [x] 5.4.5 Confirmar que no se modifican las acciones de `HomeController`,
  `DashboardController`, `AlertasController`, `HistoricoController` ni
  `TareasMantenimientoController`.

### 5.5 Parques solares

- [x] 5.5.1 Actualizar `SolarUY/Views/ParquesSolares/Index.cshtml` con grilla
  de tarjetas, acciones Ver nodos, Agregar nodo, Editar y Eliminar, y estado
  vacio.
- [x] 5.5.2 Actualizar `SolarUY/Views/ParquesSolares/Details.cshtml` para usar
  `ParqueSolarDetalleViewModel`, mostrar resumen, acordeon de nodos, paneles
  inline y acciones de gestion.
- [x] 5.5.3 Actualizar `SolarUY/Views/ParquesSolares/Create.cshtml` con card,
  grilla responsive, textos de ayuda y barra de acciones.
- [x] 5.5.4 Actualizar `SolarUY/Views/ParquesSolares/Edit.cshtml` con el mismo
  estilo y acciones de guardar/cancelar.
- [x] 5.5.5 Actualizar `SolarUY/Views/ParquesSolares/Delete.cshtml` con card de
  peligro y contexto del parque.

### 5.6 Nodos

- [x] 5.6.1 Actualizar `SolarUY/Views/Nodos/Index.cshtml` con tarjetas/tabla,
  badges de estado, acciones Ver paneles, Agregar panel, Detalle, Editar y
  Eliminar, y estado vacio.
- [x] 5.6.2 Actualizar `SolarUY/Views/Nodos/Details.cshtml` para usar
  `NodoDetalleViewModel`, mostrar tarjeta de inversor, resumen, paneles inline y
  acciones de gestion.
- [x] 5.6.3 Actualizar `SolarUY/Views/Nodos/Create.cshtml` con secciones
  "Datos del nodo" y "Datos del inversor", card, grilla y barra de acciones.
- [x] 5.6.4 Actualizar `SolarUY/Views/Nodos/Edit.cshtml` con el mismo estilo y
  `returnUrl`.
- [x] 5.6.5 Actualizar `SolarUY/Views/Nodos/Delete.cshtml` con card de peligro,
  contexto Parque -> Nodo e `returnUrl`.

### 5.7 Paneles

- [x] 5.7.1 Actualizar `SolarUY/Views/Paneles/Index.cshtml` con tarjetas/tabla,
  acciones Detalle, Editar, Eliminar y Tareas, boton Nuevo panel y estado vacio.
- [x] 5.7.2 Actualizar `SolarUY/Views/Paneles/Details.cshtml` con migas,
  tarjetas de datos, contexto de ubicacion y acciones.
- [x] 5.7.3 Actualizar `SolarUY/Views/Paneles/Create.cshtml` con secciones
  "Datos del panel" e "Instalacion y observaciones", card y barra de acciones.
- [x] 5.7.4 Actualizar `SolarUY/Views/Paneles/Edit.cshtml` con el mismo estilo
  y `returnUrl`.
- [x] 5.7.5 Actualizar `SolarUY/Views/Paneles/Delete.cshtml` con card de
  peligro, contexto Parque -> Nodo -> Panel e `returnUrl`.

### 5.8 Resto del sitio

- [x] 5.8.1 Actualizar `SolarUY/Views/Home/Index.cshtml` con hero y tarjetas de
  acceso rapido por rol.
- [x] 5.8.2 Actualizar `SolarUY/Views/Dashboard/Index.cshtml` con encabezado e
  indicador "En vivo", manteniendo `#dashboardLive` y el fetch actual.
- [x] 5.8.3 Actualizar `SolarUY/Views/Dashboard/_DashboardContenido.cshtml` con
  tarjetas por parque, acordeon de nodos, tabla de paneles, badges de salud y
  estado vacio.
- [x] 5.8.4 Actualizar `SolarUY/Views/Alertas/Index.cshtml` con tarjetas de
  resumen, tablas responsive, badges y estados vacios.
- [x] 5.8.5 Actualizar `SolarUY/Views/Historico/Index.cshtml` con formulario en
  card, KPIs, card del grafico, tabla responsive y estado vacio, manteniendo
  `chart.js` y `historico.js`.
- [x] 5.8.6 Actualizar `SolarUY/Views/TareasMantenimiento/Index.cshtml`,
  `Create.cshtml`, `Edit.cshtml`, `Details.cshtml`, `Delete.cshtml` y
  `CambiarEstado.cshtml` con banner de contexto, migas, badges y acciones
  consistentes.
- [x] 5.8.7 Actualizar `SolarUY/Views/DevAuth/AccesoDenegado.cshtml` y
  `SolarUY/Views/Shared/Error.cshtml` con el estilo compartido.

### 5.9 Limpieza de mensajes

- [x] 5.9.1 Quitar de las vistas los bloques duplicados de
  `TempData["Mensaje"]` y `TempData["Error"]` para que los muestre solo
  `_TempDataAlertas` desde el layout.
- [x] 5.9.2 Verificar que los formularios conserven `asp-validation-summary` y
  `asp-validation-for`.

## 6. Documentacion

- [x] 6.1 Confirmar que no se modificaron `README.md` ni
  `ControlCalidad/README.md`. Verificar con `git diff -- README.md
  ControlCalidad/README.md` que no haya cambios.
- [x] 6.2 Mantener actualizados `requirements.md`, `specs.md` y `tasks.md` del
  requerimiento.

## 7. Verificacion

- [x] 7.1 `docker compose run --rm tools dotnet build SolarUY.sln`
- [x] 7.2 `docker compose run --rm tools dotnet test SolarUY.sln`
- [x] 7.3 Confirmar ArchUnitNET en verde (incluido en 7.2).
- [ ] 7.4 Ejecutar `./ControlCalidad/SonarQube/analizar.sh` y registrar
  hallazgos.
- [x] 7.5 Prueba manual del flujo completo:
      Inicio -> Parques -> Agregar nodo desde el listado -> Detalle del parque
      con nodos y paneles -> Agregar panel desde el hub -> Editar/Eliminar ->
      Tareas -> Dashboard -> Alertas -> Historico.
- [x] 7.6 Revisar `git diff` final y confirmar que no hay cambios en
  `SolarUY/Domain`, `SolarUY/Application`, `SolarUY/Infrastructure`,
  `SolarUY/Migrations`, `ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests`
  ni en los README.

## Notas

- No se agregan librerias externas ni paquetes de iconos.
- No hay migraciones.
- El hub del parque carga paneles por nodo; se acepta el N+1 para el volumen
  del taller.
- No se modifica `ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests` sin
  autorizacion explicita.
- No se modifican los README por pedido explicito del usuario.
- 7.4 quedo pendiente: `SONAR_TOKEN` del `.env` es invalido
  (`/api/authentication/validate` devuelve `{"valid":false}`). Regenerar el token
  y volver a ejecutar `bash ControlCalidad/SonarQube/analizar.sh`.
- 7.5 se verifico con smoke test HTTP sobre la aplicacion levantada: alta,
  detalle, edicion y eliminacion de parque, nodo, panel y tarea; todas las
  vistas principales respondieron 200 sin errores Razor. Queda pendiente una
  revision visual en navegador si se desea validar estetica final.
