# Tareas: Gestión de Nodos e Inversores

Este plan sigue el flujo SDD. Se marca `[x]` al completar. Las tareas están organizadas por capas.

## 1. Preparación y documentación
- [x] 1.1 Crear carpeta `ControlCalidad/prompting/req003_nodos_inversores/` con `requirements.md`, `specs.md` y `tasks.md`.
- [x] 1.2 Validar alcance con el usuario (aprobado: req003_nodos_inversores, nodos e inversores juntos, 1:1, código único por parque, desde detalle de ParqueSolar, baja lógica).

## 2. Dominio (`SolarUY/Domain`)
- [x] 2.1 Crear `SolarUY/Domain/Entities/Inversor.cs` con validaciones propias, constructor EF Core + constructor público, `ActualizarDatos(...)`, constantes de longitud.
- [x] 2.2 Crear `SolarUY/Domain/Entities/Nodo.cs` con `ParqueSolarId`, relación con `Inversor` (1:1), `Codigo` único por parque (validaciones base), `ActualizarDatos(...)`, constructor para crear Nodo+Inversor.
- [x] 2.3 Crear `SolarUY/Domain/Exceptions/NodoNoEncontradoException.cs`.
- [x] 2.4 Crear `SolarUY/Domain/Exceptions/CodigoNodoDuplicadoException.cs`.
- [x] 2.5 Crear `SolarUY/Domain/Repositories/INodoRepository.cs` con métodos: `AgregarAsync`, `ObtenerPorIdAsync`, `ObtenerPorParqueAsync`, `ExisteCodigoAsync(string codigo, Guid parqueSolarId, Guid? nodoIdExcluido = null, ...)`, `Eliminar`, `SaveChangesAsync`.
- [x] 2.6 Agregar `Eliminado` y `FechaEliminacion` a `Nodo` e `Inversor`, con `Eliminar()` idempotente; `Nodo.Eliminar()` marca también su inversor.

## 3. Pruebas unitarias del Dominio (`SolarUY.Tests/Domain`)
- [x] 3.1 Crear `SolarUY.Tests/Domain/InversorTests.cs` con tests de creación/validaciones/trims/actualización.
- [x] 3.2 Crear `SolarUY.Tests/Domain/NodoTests.cs` con tests de creación con inversor, validación de código, unicidad lógica si aplica, actualización y restricciones.
- [x] 3.3 Agregar tests de baja lógica de `Nodo` e `Inversor`: estado `Eliminado`, fecha y cascada de `Nodo` sobre su inversor.

## 4. Aplicación (`SolarUY/Application`)
- [x] 4.1 Crear DTOs: `SolarUY/Application/Dtos/InversorDto.cs`, `SolarUY/Application/Dtos/NodoDto.cs`, `SolarUY/Application/Dtos/NodoDetalleDto.cs`, `SolarUY/Application/Dtos/CrearNodoInversorDto.cs`, `SolarUY/Application/Dtos/EditarNodoInversorDto.cs` con DataAnnotations.
- [x] 4.2 Crear `SolarUY/Application/Services/INodoService.cs` con métodos de listado/obtención/creación/edición/eliminación.
- [x] 4.3 Crear `SolarUY/Application/Services/NodoService.cs` con lógica: validar parque existente, validar unicidad de código por parque, crear Nodo+Inversor, editar ambos, eliminar, mapeo a DTOs.

## 5. Infraestructura (`SolarUY/Infrastructure`)
- [x] 5.1 Actualizar `SolarUY/Infrastructure/Persistence/SolarUYDbContext.cs` agregando `DbSet<Nodo>` y `DbSet<Inversor>`.
- [x] 5.2 Crear `SolarUY/Infrastructure/Persistence/Configurations/InversorConfiguration.cs` (tabla `Inversores`, claves, relaciones).
- [x] 5.3 Crear `SolarUY/Infrastructure/Persistence/Configurations/NodoConfiguration.cs` (tabla `Nodos`, FK a ParqueSolar, índice único `(ParqueSolarId, Codigo)`, relación 1:1 con Inversor).
- [x] 5.4 Crear `SolarUY/Infrastructure/Repositories/NodoRepository.cs` con `Include` de Inversor/ParqueSolar según corresponda, validación de unicidad por parque y baja lógica en `Eliminar` invocando `nodo.Eliminar()`.
- [x] 5.5 Registrar servicios/repositorios en `Program.cs` (`builder.Services.AddScoped<INodoService, NodoService>()`, `builder.Services.AddScoped<INodoRepository, NodoRepository>()`).
- [x] 5.6 Generar y aplicar migración `AgregarNodosEInversores` (`dotnet ef migrations add AgregarNodosEInversores` / aplicar según flujo del repo).
- [x] 5.7 Configurar columnas `Eliminado`/`FechaEliminacion` y filtros globales encadenados para `Nodo` e `Inversor`; generar la migración `AgregarBorradoLogico`.

## 6. Presentación (`SolarUY/Controllers`, `SolarUY/Views`)
- [x] 6.1 Crear `SolarUY/Controllers/NodosController.cs` con `[Authorize(Roles = "Administrador")]`: `Index(parqueId)`, `Create(parqueId)` GET/POST, `Details(id)`, `Edit(id)` GET/POST, `Delete(id)` GET, `DeleteConfirmed(id)` POST con `[ValidateAntiForgeryToken]`. Manejo de excepciones y `TempData`.
- [x] 6.2 Crear vistas `SolarUY/Views/Nodos/Index.cshtml`, `Create.cshtml`, `Details.cshtml`, `Edit.cshtml`, `Delete.cshtml`.
- [x] 6.3 Actualizar `SolarUY/Views/ParquesSolares/Details.cshtml` agregando botón/enlace "Gestionar nodos" hacia `Nodos/Index` con `parqueId`.
- [x] 6.4 Actualizar `SolarUY/Views/ParquesSolares/Index.cshtml` si se considera necesario (opcional) para facilitar acceso.

## 7. Validación, calidad y cierre
- [x] 7.1 Ejecutar `docker compose --profile tools run --rm tools dotnet build SolarUY.sln` y corregir errores hasta build exitoso.
- [x] 7.2 Ejecutar `docker compose --profile tools run --rm tools dotnet test SolarUY.sln` (incluye `CodeQuality.Tests` ArchUnitNET + `SolarUY.Tests`) y corregir hasta verde.
- [x] 7.3 Verificación funcional: levantar app con docker y smoke test (acceder desde detalle de parque -> crear nodo+inversor -> listar -> detalle -> editar -> eliminar).
- [x] 7.4 Ejecutar SonarQube con `./ControlCalidad/SonarQube/analizar.sh`, revisar resultados en `http://localhost:9000`, corregir hallazgos relevantes y registrar resumen. Resultado: Quality Gate OK, 0 bugs, 0 vulnerabilidades, 0 code smells, 0 hotspots; cobertura no reportada (0.0) y duplicacion 8.7% (por debajo del umbral). Se excluyeron las migraciones generadas del analisis.
- [x] 7.5 Actualizar documentación: `README.md` (módulo Parques/Nodos pasar a "En curso/Implementado" según corresponda), `ControlCalidad/README.md` fila `req003_nodos_inversores`, marcar tareas `[x]` en `tasks.md`.

## Notas
- Código único del Nodo **únicamente por ParqueSolar**.
- Relación **1:1 Nodo-Inversor**.
- Baja **lógica** con confirmación: nodo e inversor quedan con `Eliminado = true` y dejan de mostrarse, pero no se eliminan filas.
- Los filtros globales ocultan los nodos e inversores cuyo parque fue eliminado lógicamente.
- Acceso **desde el detalle del ParqueSolar**.
- Solo Dominio con tests unitarios obligatorios.
- No modificar `CodeQuality.Tests` sin autorización explícita.
- Generar migración por cambio de esquema.
