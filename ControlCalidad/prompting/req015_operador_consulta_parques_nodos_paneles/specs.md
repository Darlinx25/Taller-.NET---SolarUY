# Especificacion Tecnica: consulta de parques, nodos y paneles para Operador

## Objetivo

Habilitar el acceso de solo lectura del rol `Operador` a las pantallas de
parques solares, nodos e inversores y paneles, reutilizando los controladores y
vistas existentes. Las acciones de alta, edicion, baja y administracion de
tareas de mantenimiento siguen siendo exclusivas del rol `Administrador`.

## Decisiones y supuestos

- El rol de Keycloak se llama exactamente `Operador`.
- Se reutilizan las rutas y vistas actuales; no se crea un area `/Operador`.
- La autorizacion real se aplica en el servidor por accion. Ocultar botones en
  las vistas es solo una mejora de experiencia, no el mecanismo de seguridad.
- `TareasMantenimiento` queda fuera de alcance y sigue siendo exclusivo de
  `Administrador`.
- No hay cambios de Dominio, Application, Infrastructure ni de esquema.
- No se agregan tests unitarios en Presentation; la validacion de autorizacion
  se realiza con build, tests existentes y prueba manual.

## 1) Dominio (`SolarUY.Domain`)

### Entidades

Sin cambios. `ParqueSolar`, `Nodo`, `Inversor` y `Panel` conservan sus reglas y
contratos actuales.

### Validaciones

Sin cambios.

### Excepciones

Sin cambios.

### Repositorios (contratos)

Sin cambios.

## 2) Aplicacion (`SolarUY.Application`)

### DTOs

Sin cambios.

### Servicios y casos de uso

Sin cambios. Los servicios actuales ya exponen las consultas necesarias para
listados y detalles.

## 3) Infraestructura (`SolarUY.Infrastructure`)

### DbSet y configuraciones

Sin cambios.

### Repositorios

Sin cambios.

### Migracion

No se necesita migracion porque no cambia el esquema de base de datos.

### Registro de dependencias (`Program.cs`)

Sin cambios. Los servicios y repositorios existentes ya estan registrados.

## 4) Presentacion (`SolarUY.Controllers` + `Views`)

### Controlador y acciones

Se mantienen los controladores actuales:

- `ParquesSolaresController`
- `NodosController`
- `PanelesController`

Acciones de lectura habilitadas para `Administrador` y `Operador`:

| Controlador | Acciones |
| --- | --- |
| `ParquesSolaresController` | `Index`, `Details` |
| `NodosController` | `Index`, `Details` |
| `PanelesController` | `Index`, `Details` |

Acciones exclusivas de `Administrador`:

| Controlador | Acciones |
| --- | --- |
| `ParquesSolaresController` | `Create` GET/POST, `Edit` GET/POST, `Delete` GET, `DeleteConfirmed` POST |
| `NodosController` | `Create` GET/POST, `Edit` GET/POST, `Delete` GET, `DeleteConfirmed` POST |
| `PanelesController` | `Create` GET/POST, `Edit` GET/POST, `Delete` GET, `DeleteConfirmed` POST |

`TareasMantenimientoController` no se modifica y conserva
`[Authorize(Roles = "Administrador")]`.

### Autorizacion

- Cambiar el atributo de clase de los tres controladores a
  `[Authorize(Roles = "Administrador,Operador")]`.
- Agregar `[Authorize(Roles = "Administrador")]` a cada accion de gestion
  indicada arriba.
- Los atributos de autorizacion se combinan: un `Operador` satisface el rol de
  clase, pero no el rol de la accion de gestion, por lo que recibe acceso
  denegado.
- Los accesos anonimos siguen redirigiendo al inicio de sesion.
- No se modifica `Program.cs` ni la configuracion de autenticacion.

### Vistas

En `Views/ParquesSolares/Index.cshtml`, `Details.cshtml`,
`Views/Nodos/Index.cshtml`, `Details.cshtml`, `Views/Paneles/Index.cshtml` y
`Details.cshtml`:

- Definir `var puedeGestionar = User.IsInRole("Administrador");`.
- Mantener visibles para el Operador los enlaces de navegacion y consulta:
  `Detalle`, `Ver nodos`, `Ver paneles`, `Volver`.
- Ocultar para el Operador:
  - `Registrar parque solar`, `Nuevo nodo e inversor`, `Agregar panel`.
  - `Editar` y `Eliminar` de parques, nodos y paneles.
  - `Agregar nodo`, `Agregar panel` desde los detalles.
  - Cualquier enlace a `TareasMantenimiento` (`Tareas`, `Nueva tarea`).
- Configurar los `EmptyStateViewModel` para que el Operador vea solo el mensaje,
  sin `TextoAccion` ni `UrlAccion`.
- No se modifican las vistas `Create`, `Edit` ni `Delete`, porque siguen siendo
  exclusivas de `Administrador`.

En `Views/Shared/_Layout.cshtml`:

- Mostrar el enlace de la seccion `Parques Solares` para
  `Administrador` y `Operador`.

En `Views/Home/Index.cshtml`:

- Mostrar la tarjeta de acceso a parques para `Administrador` y `Operador`.
- Para `Administrador`, mantener el texto y boton de gestion actuales.
- Para `Operador`, usar un texto de consulta y el boton `Consultar parques`.

No se modifica `TareasMantenimiento` ni se agregan enlaces nuevos a esa seccion
para el Operador.

## 5) Pruebas

- Tests xUnit del Dominio (obligatorios): no se agregan porque no hay cambios de
  comportamiento en Dominio.
- No se agregan tests en Application, Infrastructure ni Presentation.
- Prueba manual obligatoria:
  - Iniciar sesion con `operador@solaruy.uy`.
  - Abrir Parques, Nodos y Paneles desde el inicio y el menu.
  - Verificar que no aparecen acciones de alta, edicion, eliminacion ni tareas.
  - Intentar acceder por URL directa a `Create`, `Edit` y `Delete`; debe
    denegarse el acceso.
  - Iniciar sesion con `admin@solaruy.uy` y verificar que todas las acciones
    siguen disponibles.

## 6) Calidad y verificacion

- [ ] Build: `docker compose run --rm tools dotnet build SolarUY.sln`
- [ ] Tests: `docker compose run --rm tools dotnet test SolarUY.sln`
- [ ] ArchUnitNET en verde
- [ ] SonarQube ejecutado y hallazgos revisados, si el entorno y el token estan disponibles
- [ ] Prueba manual del flujo completo

## 7) Fuera de alcance

- Consulta de tareas de mantenimiento por parte del Operador.
- Nuevas rutas o vistas exclusivas para el Operador.
- Cambios en reglas de Dominio, servicios de Application, repositorios o base de datos.
- Cambios en el comportamiento del Administrador.
