# Tareas de Implementación: Alta de Parque Solar

- [ ] **1. Capa Domain (`SolarUY.Domain`)**
  - [ ] 1.1 Crear la entidad `ParqueSolar` en la carpeta de Dominio.
  - [ ] 1.2 Agregar las propiedades (`Id`, `Nombre`, `Ubicacion`, `CapacidadEstimadaMW`, `FechaCreacion`).
  - [ ] 1.3 Agregar constructor con validaciones básicas y asignación de `FechaCreacion`.
  - [ ] 1.4 Crear la excepción de dominio `NombreParqueDuplicadoException`.
  - [ ] 1.5 Escribir los tests unitarios (xUnit) del comportamiento de la entidad `ParqueSolar`.

- [ ] **2. Capa Infrastructure (`SolarUY.Infrastructure`)**
  - [ ] 2.1 Definir la interfaz `IParqueSolarRepository` (en `Application`/`Domain`).
  - [ ] 2.2 Crear la clase `ParqueSolarRepository : IParqueSolarRepository`.
  - [ ] 2.3 Crear la clase `ParqueSolarConfiguration : IEntityTypeConfiguration<ParqueSolar>`.
  - [ ] 2.4 Configurar el índice único para la propiedad `Nombre` y las longitudes máximas.
  - [ ] 2.5 Agregar la propiedad `DbSet<ParqueSolar>` en el `SolarUYDbContext`.
  - [ ] 2.6 Registrar el `DbContext` y el repositorio en `Program.cs`.
  - [ ] 2.7 Generar y aplicar la migración (`dotnet ef migrations add AddParqueSolar` / `dotnet ef database update`).

- [ ] **3. Capa Application (`SolarUY.Application`)**
  - [ ] 3.1 Crear el DTO `CrearParqueSolarDto` con sus validaciones (DataAnnotations).
  - [ ] 3.2 Crear la interfaz `IParqueSolarService` y su implementación `ParqueSolarService`.
  - [ ] 3.3 Implementar el método de creación (inyectar `IParqueSolarRepository`, chequear unicidad de nombre, guardar cambios, retornar Id).
  - [ ] 3.4 Registrar `IParqueSolarService` en `Program.cs`.

- [ ] **4. Capa Presentation (MVC + Bootstrap)**
  - [ ] 4.1 Crear `ParquesSolaresController` con la acción `Create()` (GET).
  - [ ] 4.2 Restringir el acceso con `[Authorize(Roles = "Administrador")]`.
  - [ ] 4.3 Crear la acción `Create(CrearParqueSolarDto dto)` (POST) y validar `ModelState`.
  - [ ] 4.4 Crear la vista `Views/ParquesSolares/Create.cshtml` con el formulario en Bootstrap y tag helpers.
  - [ ] 4.5 Redirigir con mensaje de éxito (`TempData`) tras la creación.

- [ ] **5. Verificación de Calidad (Guardarraíles)**
  - [ ] 5.1 Ejecutar `dotnet test` sobre `CodeQuality.Tests` y confirmar que no existen violaciones de arquitectura (ArchUnitNET).
  - [ ] 5.2 Ejecutar el análisis estático con SonarQube (`ControlCalidad/SonarQube/analizar.ps1`) y asegurar que no hay code smells ni código duplicado.
