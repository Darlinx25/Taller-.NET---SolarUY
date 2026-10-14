# Especificaciones Técnicas: Alta de Parque Solar

## Arquitectura y Patrones
- **Estilo:** Clean Architecture expresada como **monolito ASP.NET Core MVC organizado en capas** (carpetas + namespaces `Domain`, `Application`, `Infrastructure` y Presentación MVC).
- **Patrones:** Application Services (casos de uso) + patrón Repository para el acceso a datos.

## Detalles de Implementación por Capa

### 1. Capa Domain (`SolarUY.Domain`)
- **Entidad:** `ParqueSolar`
  - **Propiedades:**
    - `Id` (Guid, PK)
    - `Nombre` (string, max 100 caracteres)
    - `Ubicacion` (string, max 200 caracteres)
    - `CapacidadEstimadaMW` (decimal, opcional)
    - `FechaCreacion` (DateTime, asignado en el constructor)
  - **Comportamiento:** Constructor con validaciones básicas (por ejemplo, `Nombre` y `Ubicacion` no pueden ser null o whitespace). La entidad representa un objeto con estado, no una simple tabla.

### 2. Capa Application (`SolarUY.Application`)
- **DTO de Entrada:** `CrearParqueSolarDto`
  - Propiedades: `Nombre`, `Ubicacion`, `CapacidadEstimadaMW`.
  - **Validación:** usar **DataAnnotations** (`[Required]`, `[StringLength]`) sobre el DTO, que MVC valida automáticamente vía `ModelState`.
- **Servicio:** `ParqueSolarService` (implementa la interfaz `IParqueSolarService`)
  - Dependencias: `IParqueSolarRepository` (inyectada por constructor).
  - Método: `CrearParqueSolarAsync(CrearParqueSolarDto dto)`
  - Lógica:
    1. Validar que no exista ya un `ParqueSolar` con ese mismo nombre (consultando `IParqueSolarRepository`); si existe, lanzar la excepción de dominio `NombreParqueDuplicadoException`.
    2. Instanciar la entidad `ParqueSolar` (sus validaciones de constructor se aplican aquí).
    3. Persistir a través del repositorio y hacer `SaveChangesAsync()`.
    4. Retornar el `Id` generado.

### 3. Capa Infrastructure (`SolarUY.Infrastructure`)
- **Repositorio:** `ParqueSolarRepository : IParqueSolarRepository`
  - Expone los métodos de acceso a datos (`ExisteNombreAsync`, `AgregarAsync`, `SaveChangesAsync`).
  - La interfaz `IParqueSolarRepository` se define en `Application`/`Domain`; la implementación concreta vive en `Infrastructure` (DIP).
- **Configuración de Entity Framework:** `ParqueSolarConfiguration : IEntityTypeConfiguration<ParqueSolar>`
  - Definir la tabla y la longitud máxima de columnas.
  - Crear un **Índice Único** para la columna `Nombre` para garantizar a nivel de BD la regla de negocio.
- **DbContext:** `SolarUYDbContext` debe exponer `DbSet<ParqueSolar> ParquesSolares { get; set; }`.
- **Migraciones:** Se deberá generar y aplicar una migración para reflejar estos cambios en la base de datos (`dotnet ef migrations add ...`).

### 4. Capa Presentation (`SolarUY.Controllers` + `Views`)
- **Controlador:** `ParquesSolaresController : Controller`
  - **Seguridad:** decorar el controlador (o la acción) con `[Authorize(Roles = "Administrador")]`.
  - Acción `Create()` (GET): retorna la vista con un `CrearParqueSolarDto` vacío.
  - Acción `Create(CrearParqueSolarDto dto)` (POST): valida `ModelState`; si es válido, inyecta `IParqueSolarService` (por constructor) y ejecuta `CrearParqueSolarAsync`; ante éxito, redirige a la lista de parques con un mensaje de confirmación (`TempData`).
- **Vista (Bootstrap + Tag Helpers):** `Views/ParquesSolares/Create.cshtml`
  - Formulario con `asp-for` / `asp-validation-for` y clases de **Bootstrap** (`form-control`, `btn btn-primary`, `text-danger`).
  - Campos: nombre (text), ubicación (text) y capacidad estimada (number).
  - Incluir `_ValidationScriptsPartial` para la validación en cliente de jQuery.
- **Regla:** el controlador y la vista NO contienen lógica de negocio; toda la decisión y el cálculo viven en `Application`/`Domain`.
