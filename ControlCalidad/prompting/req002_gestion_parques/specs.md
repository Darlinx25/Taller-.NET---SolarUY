# Especificaciones Técnicas: Gestión de Parques Solares

## Arquitectura y Patrones
- **Estilo:** monolito ASP.NET Core MVC organizado en capas (`Domain`, `Application`, `Infrastructure` y Presentación MVC), sin nuevos proyectos.
- **Patrones:** Application Services (casos de uso) + Repository para el acceso a datos.
- **Alcance:** el listado de parques ya existe (`req001`); este requerimiento agrega detalle, edición y baja.
- **Base de datos:** no hay cambios de esquema, por lo que no se genera una nueva migración.

## Detalles de Implementación por Capa

### 1. Capa Domain (`SolarUY.Domain`)

- **Entidad `ParqueSolar`:**
  - Agregar el método `ActualizarDatos(string nombre, string ubicacion, decimal? capacidadEstimadaMW)`.
  - Debe reutilizar las validaciones privadas existentes (`ValidarNombre`, `ValidarUbicacion`, `ValidarCapacidad`).
  - Debe asignar los valores recortados (`Trim`) de `Nombre` y `Ubicacion`.
  - No debe modificar `Id` ni `FechaCreacion`.
  - Se mantiene el método existente `ActualizarCapacidadEstimada`.
- **Excepción:** crear `ParqueSolarNoEncontradoException` (hereda de `Exception`, expone `Id`), para cuando se intenta editar o eliminar un parque inexistente.
- **Repositorio (`IParqueSolarRepository`):**
  - `Task<ParqueSolar?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)`.
  - Ampliar `ExisteNombreAsync` con un parámetro opcional `Guid? parqueIdExcluido = null`, para permitir conservar el nombre actual al editar.
  - `void Eliminar(ParqueSolar parque)`.
- **Tests:** agregar pruebas xUnit del comportamiento de `ActualizarDatos` (actualización válida, recorte de espacios, nombre/ubicación vacíos, capacidad negativa, y que no cambien `Id` ni `FechaCreacion`).

### 2. Capa Application (`SolarUY.Application`)

- **DTO de edición:** `EditarParqueSolarDto`.
  - Propiedades: `Id` (Guid), `Nombre`, `Ubicacion`, `CapacidadEstimadaMW`.
  - Validaciones con **DataAnnotations**: `[Required]`, `[StringLength]` y `[Range(0, ...)]`, igual que `CrearParqueSolarDto`.
- **Servicio (`IParqueSolarService` / `ParqueSolarService`):**
  - `Task<ParqueSolarDto?> ObtenerParqueAsync(Guid id, CancellationToken cancellationToken = default)`:
    - Busca por id; si no existe retorna `null`; si existe, mapea la entidad a `ParqueSolarDto`.
  - `Task EditarParqueSolarAsync(EditarParqueSolarDto dto, CancellationToken cancellationToken = default)`:
    1. Obtener el parque por `dto.Id`; si no existe, lanzar `ParqueSolarNoEncontradoException`.
    2. Verificar que el nombre no esté usado por **otro** parque (`ExisteNombreAsync(nombre, dto.Id, ...)`); si existe, lanzar `NombreParqueDuplicadoException`.
    3. Invocar `ParqueSolar.ActualizarDatos(...)`.
    4. Persistir con `SaveChangesAsync()`.
  - `Task EliminarParqueSolarAsync(Guid id, CancellationToken cancellationToken = default)`:
    1. Obtener el parque por id; si no existe, lanzar `ParqueSolarNoEncontradoException`.
    2. Invocar `IParqueSolarRepository.Eliminar(parque)`.
    3. Persistir con `SaveChangesAsync()`.
  - El servicio sigue dependiendo únicamente de `IParqueSolarRepository`; no conoce MVC ni EF Core.

### 3. Capa Infrastructure (`SolarUY.Infrastructure`)

- **`ParqueSolarRepository`:**
  - Implementar `ObtenerPorIdAsync` con `FirstOrDefaultAsync` (sin `AsNoTracking`, porque la entidad se usa para editar/eliminar).
  - Adaptar `ExisteNombreAsync` para excluir opcionalmente el parque indicado (`parque.Id != parqueIdExcluido`).
  - Implementar `Eliminar` con `_contexto.ParquesSolares.Remove(parque)`.
- **Persistencia:** el índice único sobre `Nombre` ya existe y sigue respaldando la regla de unicidad. No se requieren cambios en `SolarUYDbContext`, configuraciones ni migraciones.

### 4. Capa Presentation (`SolarUY.Controllers` + `Views`)

- **Controlador `ParquesSolaresController`** (mantiene `[Authorize(Roles = "Administrador")]`):
  - `Details(Guid id)` (GET): invoca `ObtenerParqueAsync`; si no existe devuelve `NotFound()`, si existe retorna la vista con `ParqueSolarDto`.
  - `Edit(Guid id)` (GET): invoca `ObtenerParqueAsync`; si no existe devuelve `NotFound()`; si existe mapea a `EditarParqueSolarDto` y retorna la vista.
  - `Edit(EditarParqueSolarDto dto)` (POST, `[ValidateAntiForgeryToken]`): valida `ModelState`; invoca `EditarParqueSolarAsync`; ante `NombreParqueDuplicadoException` agrega el error a `ModelState` y vuelve a la vista; ante éxito guarda mensaje en `TempData` y redirige a `Index`.
  - `Delete(Guid id)` (GET): invoca `ObtenerParqueAsync`; si no existe devuelve `NotFound()`; si existe retorna la vista de confirmación.
  - `DeleteConfirmed(Guid id)` (POST, `[ValidateAntiForgeryToken]`): invoca `EliminarParqueSolarAsync`; ante éxito guarda mensaje en `TempData` y redirige a `Index`.
  - El controlador no contiene reglas de negocio ni accede a `Infrastructure`.
- **Vistas Razor (Bootstrap + Tag Helpers):**
  - `Views/ParquesSolares/Details.cshtml`: muestra nombre, ubicación, capacidad estimada y fecha de creación, con botones "Editar" y "Volver".
  - `Views/ParquesSolares/Edit.cshtml`: formulario con `asp-for` / `asp-validation-for`, campo oculto `Id`, validación cliente con `_ValidationScriptsPartial` y botones "Guardar" / "Cancelar".
  - `Views/ParquesSolares/Delete.cshtml`: resumen del parque y formulario POST de confirmación, con botones "Eliminar" / "Cancelar".
  - `Views/ParquesSolares/Index.cshtml`: agregar una columna de acciones con enlaces a "Detalle", "Editar" y "Eliminar" por cada parque.

## Seguridad
- Todas las acciones quedan protegidas por el rol `Administrador` a nivel de controlador.
- Los POST de edición y baja usan `[ValidateAntiForgeryToken]`.
- La baja requiere confirmación explícita en una vista intermedia.

## Verificación
1. Compilar y ejecutar `docker compose --profile tools run --rm tools dotnet test SolarUY.sln`.
2. Confirmar que pasan los tests de dominio y los guardarraíles de ArchUnitNET.
3. Ejecutar el análisis de SonarQube y registrar los hallazgos.
4. Probar manualmente el flujo: listado → detalle → editar → eliminar.
