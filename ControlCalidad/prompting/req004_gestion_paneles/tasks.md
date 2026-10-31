# Tareas: req004 - Gestión de Paneles

## Objetivo

Implementar el CRUD de Paneles asociado a un Nodo siguiendo el flujo definido en `requirements.md`
y `specs.md`.

## 0. Preparación

- [x] 0.1 Confirmar `requirements.md`, `specs.md` y `tasks.md` aprobados.
- [x] 0.2 Revisar los patrones de `Nodo` e `Inversor` implementados en req003.

## 1. Dominio

- [x] 1.1 Crear `SolarUY/Domain/Entities/Panel.cs` con constantes, constructor privado,
      constructor público, validaciones, `ActualizarDatos` y `FechaCreacion` inmutable.
- [x] 1.2 Crear `SolarUY/Domain/Exceptions/PanelNoEncontradoException.cs` con propiedad `Id`.
- [x] 1.3 Crear `SolarUY/Domain/Exceptions/CodigoPanelDuplicadoException.cs`.
- [x] 1.4 Crear `SolarUY/Domain/Repositories/IPanelRepository.cs` con los métodos de `specs.md`.
- [x] 1.5 Agregar `Eliminado` y `FechaEliminacion` a `Panel` y el método `Eliminar()` para baja lógica idempotente.

## 2. Tests del Dominio

- [x] 2.1 Crear `SolarUY.Tests/Domain/PanelTests.cs` con creación válida, trims, `Id` y
      `FechaCreacion`.
- [x] 2.2 Agregar tests de código/serie, marca, modelo, `NodoId`, potencia y observaciones inválidos.
- [x] 2.3 Agregar tests de `ActualizarDatos` y de inmutabilidad de `Id`, `FechaCreacion` y `NodoId`.
- [x] 2.4 Ejecutar los tests del dominio.
- [x] 2.5 Agregar tests de baja lógica de `Panel`: `Eliminado`, `FechaEliminacion` y comportamiento idempotente.

## 3. Aplicación

- [x] 3.1 Crear `PanelDto`, `PanelDetalleDto`, `CrearPanelDto` y `EditarPanelDto` con
      DataAnnotations y `[JsonRequired]` en los `Guid` requeridos.
- [x] 3.2 Crear `IPanelService` con los cinco casos de uso.
- [x] 3.3 Implementar `PanelService`: validar nodo, unicidad por nodo, crear, editar, eliminar
      y mapear.
- [x] 3.4 Ignorar cambios de `NodoId` al editar.

## 4. Infraestructura

- [x] 4.1 Agregar `DbSet<Panel>` en `SolarUYDbContext`.
- [x] 4.2 Crear `PanelConfiguration` con longitudes, precisión, FK, cascada e índice único
      `(NodoId, CodigoSerie)`.
- [x] 4.3 Crear `PanelRepository`; su `Eliminar` marca `panel.Eliminar()` en lugar de usar `Remove`.
- [x] 4.4 Registrar `IPanelRepository` e `IPanelService` en `Program.cs`.
- [x] 4.5 Generar y aplicar la migración `AgregarPaneles`.
- [x] 4.6 Configurar `Eliminado`/`FechaEliminacion` y filtro global; generar la migración `AgregarBorradoLogico`.

## 5. Presentación

- [x] 5.1 Crear `PanelesController` con autorización, acciones y `TempData`.
- [x] 5.2 Crear `Views/Paneles/Index.cshtml`.
- [x] 5.3 Crear `Views/Paneles/Create.cshtml` con `NodoId` oculto y validaciones.
- [x] 5.4 Crear `Views/Paneles/Details.cshtml` con Panel + Nodo + Parque Solar.
- [x] 5.5 Crear `Views/Paneles/Edit.cshtml` con `Id` y `NodoId` ocultos.
- [x] 5.6 Crear `Views/Paneles/Delete.cshtml` con confirmación.
- [x] 5.7 Agregar "Administrar paneles" en `Views/Nodos/Details.cshtml`.
- [x] 5.8 Probar la navegación completa desde Parque Solar -> Nodo -> Paneles.

## 6. Documentación

- [x] 6.1 Pasar req004 a Implementado en `ControlCalidad/README.md`.
- [x] 6.2 Pasar Paneles a Listo en `README.md`.
- [x] 6.3 Marcar todas las tareas `[x]`.

## 7. Verificación

- [x] 7.1 Build: `docker compose run --rm tools dotnet build SolarUY.sln` (0 errores, 0 warnings).
- [x] 7.2 Tests: `docker compose run --rm tools dotnet test SolarUY.sln`
      (71 tests de dominio + 9 guardarraíles ArchUnitNET, todos en verde).
- [x] 7.3 Ejecutar `./ControlCalidad/SonarQube/analizar.sh` y revisar hallazgos.
      Resultado: 0 bugs, 0 vulnerabilidades, 0 code smells y 0 issues nuevos. El Quality Gate
      queda en ERROR por cobertura no reportada (0.0%) y duplicación (10.1% vs. umbral 3%);
      no hay defectos de código que corregir.
- [x] 7.4 Prueba manual vía HTTP con login dev: crear, listar, ver detalle, editar y eliminar panel
      (flujo completo verificado).

## Notas

- La baja de paneles es lógica: la fila permanece con `Eliminado = true` y `FechaEliminacion`; los filtros globales ocultan también los paneles cuyo nodo o parque fue eliminado.
- Los códigos/serie de paneles eliminados siguen bloqueados por el índice único y por `ExisteCodigoAsync` con `IgnoreQueryFilters`.
- Mantener la nomenclatura y estructura existentes en el proyecto.
- Seguir el patrón de navegación "desde la ficha del padre" usado en req003.
- No implementar monitoreo, telemetría, alertas ni historial: están fuera de alcance.
- No modificar `ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests`.
