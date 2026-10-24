# Tareas de Implementación: Gestión de Parques Solares

## 1. Capa Domain (`SolarUY.Domain`)
- [x] 1.1 Agregar `ActualizarDatos(string nombre, string ubicacion, decimal? capacidadEstimadaMW)` a `ParqueSolar`, reutilizando las validaciones existentes y sin modificar `Id` ni `FechaCreacion`.
- [x] 1.2 Crear la excepción `ParqueSolarNoEncontradoException` en `Domain/Exceptions`, con la propiedad `Id`.
- [x] 1.3 Ampliar `IParqueSolarRepository` con:
  - `ObtenerPorIdAsync(Guid id, CancellationToken)`.
  - `ExisteNombreAsync(string nombre, Guid? parqueIdExcluido = null, CancellationToken)`.
  - `Eliminar(ParqueSolar parque)`.

## 2. Tests del Dominio (`SolarUY.Tests`)
- [x] 2.1 Agregar tests xUnit de `ParqueSolar.ActualizarDatos`:
  - Actualiza nombre, ubicación y capacidad con datos válidos.
  - Recorta espacios de nombre y ubicación.
  - Nombre nulo, vacío o solo espacios lanza `ArgumentException`.
  - Ubicación nula, vacía o solo espacios lanza `ArgumentException`.
  - Nombre mayor a 100 caracteres lanza `ArgumentException`.
  - Ubicación mayor a 200 caracteres lanza `ArgumentException`.
  - Capacidad negativa lanza `ArgumentOutOfRangeException`.
  - No modifica `Id` ni `FechaCreacion`.
- [x] 2.2 Ejecutar los tests del dominio y confirmar que pasan.

## 3. Capa Infrastructure (`SolarUY.Infrastructure`)
- [x] 3.1 Implementar `ObtenerPorIdAsync` en `ParqueSolarRepository` usando `FirstOrDefaultAsync` (sin `AsNoTracking`).
- [x] 3.2 Actualizar `ExisteNombreAsync` para excluir opcionalmente el parque indicado por `parqueIdExcluido`.
- [x] 3.3 Implementar `Eliminar` con `_contexto.ParquesSolares.Remove(parque)`.
- [x] 3.4 Confirmar que no se requieren cambios en `SolarUYDbContext`, configuraciones ni migraciones.

## 4. Capa Application (`SolarUY.Application`)
- [x] 4.1 Crear `EditarParqueSolarDto` con `Id`, `Nombre`, `Ubicacion` y `CapacidadEstimadaMW`, con DataAnnotations (`[Required]`, `[StringLength]`, `[Range]`).
- [x] 4.2 Agregar a `IParqueSolarService`:
  - `ObtenerParqueAsync(Guid id, CancellationToken)`.
  - `EditarParqueSolarAsync(EditarParqueSolarDto dto, CancellationToken)`.
  - `EliminarParqueSolarAsync(Guid id, CancellationToken)`.
- [x] 4.3 Implementar `ObtenerParqueAsync` con mapeo a `ParqueSolarDto` y retorno `null` si no existe.
- [x] 4.4 Implementar `EditarParqueSolarAsync`:
  - Buscar por id y lanzar `ParqueSolarNoEncontradoException` si no existe.
  - Validar nombre duplicado excluyendo el mismo parque.
  - Invocar `ActualizarDatos` y persistir con `SaveChangesAsync`.
- [x] 4.5 Implementar `EliminarParqueSolarAsync`:
  - Buscar por id y lanzar `ParqueSolarNoEncontradoException` si no existe.
  - Invocar `Eliminar` y persistir con `SaveChangesAsync`.
- [x] 4.6 Ajustar la llamada existente de `CrearParqueSolarAsync` a la nueva firma de `ExisteNombreAsync`.

## 5. Capa Presentation (`SolarUY.Controllers` + `Views`)
- [x] 5.1 Agregar `Details(Guid id)` (GET) en `ParquesSolaresController`, retornando `NotFound()` si no existe.
- [x] 5.2 Agregar `Edit(Guid id)` (GET), mapeando `ParqueSolarDto` a `EditarParqueSolarDto`.
- [x] 5.3 Agregar `Edit(EditarParqueSolarDto dto)` (POST) con `[ValidateAntiForgeryToken]`, validación de `ModelState`, manejo de nombre duplicado y `ParqueSolarNoEncontradoException`, mensaje en `TempData` y redirección a `Index`.
- [x] 5.4 Agregar `Delete(Guid id)` (GET) para mostrar la confirmación de baja.
- [x] 5.5 Agregar `DeleteConfirmed(Guid id)` (POST) con `[ValidateAntiForgeryToken]`, manejo de `ParqueSolarNoEncontradoException`, mensaje en `TempData` y redirección a `Index`.
- [x] 5.6 Crear `Views/ParquesSolares/Details.cshtml` con los datos del parque y botones "Editar" / "Volver".
- [x] 5.7 Crear `Views/ParquesSolares/Edit.cshtml` con formulario Bootstrap, campo oculto `Id`, validaciones y botones "Guardar" / "Cancelar".
- [x] 5.8 Crear `Views/ParquesSolares/Delete.cshtml` con resumen del parque y formulario POST de confirmación.
- [x] 5.9 Actualizar `Views/ParquesSolares/Index.cshtml` con una columna de acciones: Detalle, Editar y Eliminar.
- [x] 5.10 Mostrar en `Index.cshtml` el mensaje de éxito y los posibles errores guardados en `TempData`.

## 6. Documentación
- [x] 6.1 Agregar `req002_gestion_parques` a la tabla de requerimientos de `ControlCalidad/README.md` con estado "Implementado".
- [x] 6.2 Actualizar el README principal indicando que la gestión de parques incluye detalle, edición y baja.
- [x] 6.3 Marcar como completadas las tareas de este archivo una vez verificadas.

## 7. Verificación de Calidad
- [x] 7.1 Compilar con `docker compose --profile tools run --rm tools dotnet build SolarUY.sln`.
- [x] 7.2 Ejecutar `docker compose --profile tools run --rm tools dotnet test SolarUY.sln` y confirmar tests de dominio + ArchUnitNET en verde.
- [x] 7.3 Si falla un test de arquitectura, corregir el código fuente; no modificar `CodeQuality.Tests`.
- [ ] 7.4 Ejecutar SonarQube con `./ControlCalidad/SonarQube/analizar.sh` y registrar los hallazgos. **Pendiente: falta `SONAR_TOKEN` en `.env`.**
- [x] 7.5 Probar manualmente el flujo: listado → detalle → editar → eliminar.

## Notas
- No se crea migración porque el esquema de base de datos no cambia.
- No se agregan tests para Application, Infrastructure ni Presentation, según la regla 5 del flujo de trabajo.
- El controlador solo orquesta llamadas a `IParqueSolarService`.
- El análisis de SonarQube queda pendiente hasta configurar `SONAR_TOKEN` en el `.env` de la máquina.
