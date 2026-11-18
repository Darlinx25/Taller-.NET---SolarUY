# Especificacion Tecnica: Estado tipado de Nodos

## Objetivo

Reemplazar el estado libre de `Nodo` por un enum de Dominio y exponerlo en los
formularios como un combo con etiquetas legibles, manteniendo el contrato de
texto que consumen el simulador y el servidor MCP.

## Decisiones y supuestos

- Los estados de Nodo son `Activo`, `Inactivo` y `EnMantenimiento`.
- `EnMantenimiento` se muestra como `En mantenimiento` en la interfaz.
- El valor existente `prendido` en la base de datos se normaliza a `Activo`.
- `NodoSimulacionDto.Estado` sigue siendo `string` porque es un contrato externo.
- Las etiquetas visuales viven en Presentacion; el Dominio no depende de
  DataAnnotations ni de ASP.NET Core.

## 1) Dominio (`SolarUY.Domain`)

### Entidades

- `Nodo.Estado` pasa de `string` a `EstadoNodo`.
- El constructor publico y `ActualizarDatos(...)` reciben `EstadoNodo`.
- El estado por defecto es `EstadoNodo.Activo`.

### Validaciones

- `ValidarEstado` usa `Enum.IsDefined` para rechazar valores fuera del enum.
- Se conserva `EstadoMaxLength = 50` para la configuracion de persistencia.

### Excepciones

- Se mantiene `ArgumentException` para estados invalidos.

### Repositorios (contratos)

- Sin cambios.

## 2) Aplicacion (`SolarUY.Application`)

### DTOs

- `CrearNodoInversorDto.Estado` y `EditarNodoInversorDto.Estado` pasan a
  `EstadoNodo` con `[EnumDataType]`.
- `NodoDto.Estado` y `NodoDetalleDto.Estado` pasan a `EstadoNodo`.
- `NodoSimulacionDto.Estado` permanece `string`.

### Servicios y casos de uso

- `NodoService` crea y actualiza usando el enum y mapea los DTOs de lectura
  directamente.
- `TopologiaSimulacionService` convierte el enum a texto con `ToString()` al
  armar `NodoSimulacionDto`.

## 3) Infraestructura (`SolarUY.Infrastructure`)

### DbSet y configuraciones

- `NodoConfiguration` configura `Estado` con `HasConversion<string>()`,
  `HasMaxLength(50)` y un check constraint `CK_Nodos_Estado` generado a partir
  de los nombres del enum.

### Repositorios

- Sin cambios.

### Migracion

- Migracion `EstadoNodoEnum`.
- Antes de crear el check constraint normaliza:
  - `prendido`/`activo` -> `Activo`.
  - `inactivo` -> `Inactivo`.
  - `enfalla`/`en falla` -> `Inactivo` (mapeo aprobado para el dato existente).
  - `enmantenimiento`/`en mantenimiento`/`mantenimiento` -> `EnMantenimiento`.
- Si quedan valores desconocidos, la creacion del constraint falla y se debe
  definir el mapeo antes de continuar.

### Registro de dependencias (`Program.cs`)

- Sin cambios.

## 4) Presentacion (`SolarUY.Controllers` + `Views`)

### Controlador y acciones

- `NodosController.Create` usa `EstadoNodo.Activo` por defecto.
- `Create` y `Edit` cargan las opciones del combo en `ViewBag.EstadosNodo`,
  tanto en GET como en POST invalido.

### Autorizacion

- Sin cambios: `[Authorize(Roles = "Administrador")]`.

### Vistas

- `Views/Nodos/Create.cshtml` y `Edit.cshtml` usan
  `<select asp-for="Estado" asp-items="ViewBag.EstadosNodo">`.
- `SolarUY/Models/EstadoNodoOpciones.cs` centraliza las etiquetas:
  `Activo`, `Inactivo`, `En mantenimiento`.
- Los badges de Nodo usan `EstadoNodoOpciones.Etiqueta(...)` para mostrar
  `En mantenimiento` con espacio.
- `_EstadoBadge.cshtml` reconoce `En mantenimiento` y `EnMantenimiento` con un
  color de advertencia.

## 5) Pruebas

- Tests xUnit del Dominio (obligatorios): actualizar `NodoTests` al enum, cubrir
  estado por defecto, estados validos y estado invalido.
- No se agregan tests en Application, Infrastructure ni Presentation.

## 6) Calidad y verificacion

- [ ] Build: `docker compose run --rm tools dotnet build SolarUY.sln`
- [ ] Tests: `docker compose run --rm tools dotnet test SolarUY.sln`
- [ ] ArchUnitNET en verde
- [ ] SonarQube ejecutado y hallazgos revisados
- [ ] Prueba manual del flujo completo

## 7) Fuera de alcance

- Estados de paneles, alertas y tareas de mantenimiento.
- Cambios en el contrato JSON de topologia.
- Nuevos estados de Nodo sin definicion de negocio.
