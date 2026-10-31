# Tareas de Implementación: Alta de Parque Solar

- [x] **1. Capa Domain (`SolarUY.Domain`)**
  - [x] 1.1 Crear la entidad `ParqueSolar` en la carpeta de Dominio.
  - [x] 1.2 Agregar las propiedades (`Id`, `Nombre`, `Ubicacion`, `CapacidadEstimadaMW`, `FechaCreacion`).
  - [x] 1.3 Agregar constructor con validaciones básicas y asignación de `FechaCreacion`.
  - [x] 1.4 Crear la excepción de dominio `NombreParqueDuplicadoException`.
  - [x] 1.5 Escribir los tests unitarios (xUnit) del comportamiento de la entidad `ParqueSolar`.

- [x] **2. Capa Infrastructure (`SolarUY.Infrastructure`)**
  - [x] 2.1 Definir la interfaz `IParqueSolarRepository` (en `Application`/`Domain`).
  - [x] 2.2 Crear la clase `ParqueSolarRepository : IParqueSolarRepository`.
  - [x] 2.3 Crear la clase `ParqueSolarConfiguration : IEntityTypeConfiguration<ParqueSolar>`.
  - [x] 2.4 Configurar el índice único para la propiedad `Nombre` y las longitudes máximas.
  - [x] 2.5 Agregar la propiedad `DbSet<ParqueSolar>` en el `SolarUYDbContext`.
  - [x] 2.6 Registrar el `DbContext` y el repositorio en `Program.cs`.
  - [x] 2.7 Generar y aplicar la migración (`dotnet ef migrations add AddParqueSolar` / `dotnet ef database update`).

- [x] **3. Capa Application (`SolarUY.Application`)**
  - [x] 3.1 Crear el DTO `CrearParqueSolarDto` con sus validaciones (DataAnnotations).
  - [x] 3.2 Crear la interfaz `IParqueSolarService` y su implementación `ParqueSolarService`.
  - [x] 3.3 Implementar el método de creación (inyectar `IParqueSolarRepository`, chequear unicidad de nombre, guardar cambios, retornar Id).
  - [x] 3.4 Registrar `IParqueSolarService` en `Program.cs`.

- [x] **4. Capa Presentation (MVC + Bootstrap)**
  - [x] 4.1 Crear `ParquesSolaresController` con la acción `Create()` (GET).
  - [x] 4.2 Restringir el acceso con `[Authorize(Roles = "Administrador")]`.
  - [x] 4.3 Crear la acción `Create(CrearParqueSolarDto dto)` (POST) y validar `ModelState`.
  - [x] 4.4 Crear la vista `Views/ParquesSolares/Create.cshtml` con el formulario en Bootstrap y tag helpers.
  - [x] 4.5 Redirigir con mensaje de éxito (`TempData`) tras la creación.

- [x] **5. Verificación de Calidad (Guardarraíles)**
  - [x] 5.1 Ejecutar `dotnet test` sobre `CodeQuality.Tests` y confirmar que no existen violaciones de arquitectura (ArchUnitNET).
  - [x] 5.2 Ejecutar el análisis estático con SonarQube (`ControlCalidad/SonarQube/analizar.ps1`) y asegurar que no hay code smells ni código duplicado.

- [x] **6. Soporte de baja lógica para ParqueSolar (consumido por req002)**
  - [x] 6.1 Agregar `Eliminado` y `FechaEliminacion` a la entidad `ParqueSolar`.
  - [x] 6.2 Implementar `Eliminar()` como baja lógica idempotente.
  - [x] 6.3 Escribir tests xUnit del nuevo comportamiento de baja lógica.
  - [x] 6.4 Configurar la columna `Eliminado` (default `false`), `FechaEliminacion` y el filtro global de consulta.
  - [x] 6.5 Generar la migración `AgregarBorradoLogico`.

---

## Notas de implementación

- **Base de datos:** MySQL 8.4 con el provider oficial `MySql.EntityFrameworkCore` 10.0.12 (el plan inicial hablaba de SQL Server; se adjusts al requisito de "instalar lo mínimo en las máquinas", ya que MySQL se levanta en un contenedor sin instalación local).
- **Migración:** `20261021234048_AddParqueSolar` crea `ParquesSolares` (`Id char(36)` PK, `Nombre varchar(100)` con índice único, `Ubicacion varchar(200)`, `CapacidadEstimadaMW decimal(18,2)`, `FechaCreacion datetime(6)`) en `utf8mb4`.
- **Autenticación:** la integración con OpenID Connect queda para una iteración posterior; en `Development` se usa un stub (`/dev/login`) que firma un usuario con rol `Administrador`. El controlador ya queda restringido con `[Authorize(Roles = "Administrador")]`.
- **Pruebas:** las pruebas xUnit del dominio viven en `SolarUY.Tests` (14 casos). Los guardarraíles de arquitectura quedan en `ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests`.
- **Baja lógica:** la entidad `ParqueSolar` incorpora `Eliminado`/`FechaEliminacion`, filtro global de consulta y la migración `AgregarBorradoLogico`; la baja se consume desde req002.
