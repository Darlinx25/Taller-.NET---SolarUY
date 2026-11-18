# Tareas: req014 Estado tipado de Nodos

> Cada tarea debe ser verificable, indicar el archivo o la ruta cuando aplique
> y comenzar con `[ ]`. Marcar `[x]` solo cuando este terminada y verificada.

## 0. Preparacion

- [x] 0.1 Confirmar que `requirements.md`, `specs.md` y este plan estan aprobados.
- [x] 0.2 Revisar el codigo existente para respetar nombres y patrones.

## 1. Dominio (`SolarUY.Domain`)

- [x] 1.1 Crear `SolarUY/Domain/Enums/EstadoNodo.cs` con `Activo`, `Inactivo` y `EnMantenimiento`.
- [x] 1.2 Cambiar `Nodo.Estado` a `EstadoNodo`, incluidos constructor y `ActualizarDatos`.
- [x] 1.3 Validar el estado con `Enum.IsDefined` y mantener `Activo` como default.

## 2. Tests del Dominio (`SolarUY.Tests/Domain`)

- [x] 2.1 Actualizar `NodoTests` para usar `EstadoNodo`.
- [x] 2.2 Agregar tests de estado por defecto, estados validos y estado invalido.
- [x] 2.3 Ejecutar `dotnet test` sobre `SolarUY.Tests` y corregir hasta verde.

## 3. Aplicacion (`SolarUY.Application`)

- [x] 3.1 Cambiar `CrearNodoInversorDto` y `EditarNodoInversorDto` a `EstadoNodo` con `[EnumDataType]`.
- [x] 3.2 Cambiar `NodoDto` y `NodoDetalleDto` a `EstadoNodo`.
- [x] 3.3 Ajustar `NodoService` y `TopologiaSimulacionService`; `NodoSimulacionDto` sigue string.

## 4. Infraestructura (`SolarUY.Infrastructure`)

- [x] 4.1 Configurar `NodoConfiguration` con `HasConversion<string>()` y check constraint `CK_Nodos_Estado`.
- [x] 4.2 Generar la migracion `EstadoNodoEnum`.
- [x] 4.3 Normalizar en la migracion `prendido` -> `Activo` y `EnFalla` -> `Inactivo` antes del constraint.
- [x] 4.4 Aplicar la migracion a la base local y verificar que no queden estados invalidos.

## 5. Presentacion (`SolarUY.Controllers` + `Views`)

- [x] 5.1 Crear `SolarUY/Models/EstadoNodoOpciones.cs` con opciones y etiquetas legibles.
- [x] 5.2 Ajustar `NodosController` para usar `EstadoNodo.Activo` y cargar las opciones.
- [x] 5.3 Reemplazar los inputs de estado por combos en `Views/Nodos/Create.cshtml` y `Edit.cshtml`.
- [x] 5.4 Actualizar badges de Nodo para mostrar `En mantenimiento` con espacio.
- [x] 5.5 Actualizar `_EstadoBadge.cshtml` para reconocer `En mantenimiento`.

## 6. Documentacion

- [x] 6.1 Actualizar `ControlCalidad/README.md` con el estado de `req014_estado_nodo_enum`.
- [x] 6.2 Actualizar `README.md` si cambia la descripcion de Nodos.

## 7. Verificacion

- [x] 7.1 `docker compose run --rm tools dotnet build SolarUY.sln`
- [x] 7.2 `docker compose run --rm tools dotnet test SolarUY.sln`
- [ ] 7.3 Ejecutar `./ControlCalidad/SonarQube/analizar.sh` y registrar hallazgos.
- [ ] 7.4 Prueba manual: crear/editar nodo con combo y verificar badges.

## Notas

- No modificar `ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests` sin autorizacion.
- No se agregan tests en Application, Infrastructure ni Presentation.
