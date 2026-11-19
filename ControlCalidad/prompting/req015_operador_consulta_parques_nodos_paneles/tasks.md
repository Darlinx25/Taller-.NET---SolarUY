# Tareas: req015 consulta de parques, nodos y paneles para Operador

> Cada tarea debe ser verificable, indicar el archivo o la ruta cuando aplique
> y comenzar con `[ ]`. Marcar `[x]` solo cuando este terminada y verificada.

## 0. Preparacion

- [x] 0.1 Confirmar que `requirements.md`, `specs.md` y este plan estan aprobados.
- [x] 0.2 Revisar `SolarUY/Controllers/ParquesSolaresController.cs`, `NodosController.cs`, `PanelesController.cs`, `Views/Shared/_Layout.cshtml` y las vistas de Parques, Nodos y Paneles para respetar los patrones existentes.

## 1. Dominio (`SolarUY.Domain`)

- [x] 1.1 Confirmar que no se requieren cambios en entidades, validaciones, excepciones ni contratos de repositorio.

## 2. Tests del Dominio (`SolarUY.Tests/Domain`)

- [x] 2.1 Confirmar que no se agregan tests de Dominio porque no hay comportamiento nuevo en esa capa.

## 3. Aplicacion (`SolarUY.Application`)

- [x] 3.1 Confirmar que los DTOs y servicios existentes cubren las consultas de parques, nodos y paneles sin cambios.

## 4. Infraestructura (`SolarUY.Infrastructure`)

- [x] 4.1 Confirmar que no hay cambios de `DbSet`, configuraciones Fluent API, repositorios, migraciones ni `Program.cs`.

## 5. Presentacion (`SolarUY.Controllers` + `Views`)

### Autorizacion

- [x] 5.1 En `SolarUY/Controllers/ParquesSolaresController.cs`, cambiar el atributo de clase a `[Authorize(Roles = "Administrador,Operador")]` y agregar `[Authorize(Roles = "Administrador")]` a `Create` GET/POST, `Edit` GET/POST, `Delete` GET y `DeleteConfirmed` POST.
- [x] 5.2 En `SolarUY/Controllers/NodosController.cs`, cambiar el atributo de clase a `[Authorize(Roles = "Administrador,Operador")]` y agregar `[Authorize(Roles = "Administrador")]` a `Create` GET/POST, `Edit` GET/POST, `Delete` GET y `DeleteConfirmed` POST.
- [x] 5.3 En `SolarUY/Controllers/PanelesController.cs`, cambiar el atributo de clase a `[Authorize(Roles = "Administrador,Operador")]` y agregar `[Authorize(Roles = "Administrador")]` a `Create` GET/POST, `Edit` GET/POST, `Delete` GET y `DeleteConfirmed` POST.
- [x] 5.4 Verificar que `SolarUY/Controllers/TareasMantenimientoController.cs` conserva `[Authorize(Roles = "Administrador")]` sin cambios.

### Navegacion y acceso

- [x] 5.5 En `SolarUY/Views/Shared/_Layout.cshtml`, mostrar el enlace de la seccion `Parques Solares` para `Administrador` y `Operador`.
- [x] 5.6 En `SolarUY/Views/Home/Index.cshtml`, mostrar la tarjeta de parques para `Administrador` y `Operador`, con texto y boton de consulta para el Operador.

### Vistas de parques

- [x] 5.7 En `SolarUY/Views/ParquesSolares/Index.cshtml`, definir `puedeGestionar`, ocultar `Registrar parque solar` y las acciones de edicion/eliminacion para el Operador, y ajustar el `EmptyState` sin accion de alta.
- [x] 5.8 En `SolarUY/Views/ParquesSolares/Details.cshtml`, ocultar para el Operador `Agregar nodo`, `Editar`, `Eliminar`, `Agregar panel` y los enlaces a tareas, manteniendo `Detalle`, `Ver nodos` y `Volver`.

### Vistas de nodos

- [x] 5.9 En `SolarUY/Views/Nodos/Index.cshtml`, definir `puedeGestionar`, ocultar `Nuevo nodo e inversor`, `Agregar panel`, `Editar` y `Eliminar`, y ajustar el `EmptyState` sin accion de alta.
- [x] 5.10 En `SolarUY/Views/Nodos/Details.cshtml`, ocultar para el Operador `Agregar panel`, `Editar`, `Eliminar` y los enlaces a tareas, manteniendo `Detalle` y `Volver`.

### Vistas de paneles

- [x] 5.11 En `SolarUY/Views/Paneles/Index.cshtml`, definir `puedeGestionar`, ocultar `Agregar panel`, `Editar`, `Eliminar` y `Tareas`, y ajustar el `EmptyState` sin accion de alta.
- [x] 5.12 En `SolarUY/Views/Paneles/Details.cshtml`, ocultar para el Operador `Editar`, `Eliminar` y `Tareas`, manteniendo `Volver al nodo`.

### Validacion de presentacion

- [x] 5.13 Verificar que el Operador puede abrir `Index` y `Details` de parques, nodos y paneles sin errores.
- [x] 5.14 Verificar que el Administrador conserva visibles y funcionales todas las acciones de gestion.

## 6. Documentacion

- [x] 6.1 Actualizar `ControlCalidad/README.md` con la fila de `req015_operador_consulta_parques_nodos_paneles` y su estado.
- [x] 6.2 Actualizar `README.md` con el acceso de consulta del Operador a parques, nodos y paneles.

## 7. Verificacion

- [x] 7.1 `docker compose run --rm tools dotnet build SolarUY.sln`
- [x] 7.2 `docker compose run --rm tools dotnet test SolarUY.sln`
- [x] 7.3 Verificar ArchUnitNET en verde.
- [ ] 7.4 Ejecutar `./ControlCalidad/SonarQube/analizar.sh` y registrar hallazgos, si el entorno y el token estan disponibles.
- [x] 7.5 Prueba manual con `operador@solaruy.uy`: navegar parques, nodos y paneles; confirmar que no hay acciones de gestion ni tareas y que las URLs directas de `Create`, `Edit` y `Delete` deniegan el acceso.
- [x] 7.6 Prueba manual de regresion con `admin@solaruy.uy`: confirmar que todas las acciones de gestion siguen funcionando.

## Notas

- No se modifica `CodeQuality.Tests`.
- No se crean controladores, vistas, rutas, servicios, repositorios ni migraciones nuevas.
- La autorizacion por accion es la proteccion principal; ocultar botones es solo experiencia de usuario.
