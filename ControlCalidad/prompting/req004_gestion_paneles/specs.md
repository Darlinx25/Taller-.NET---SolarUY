# Especificación Técnica: Gestión de Paneles

## Objetivo

Implementar el CRUD de Paneles fotovoltaicos asociados a un Nodo, respetando la jerarquía
Parque Solar -> Nodo -> Panel y el patrón utilizado en req003 (gestión desde la entidad padre).

## 1) Dominio (`SolarUY/Domain`)

### 1.1 Entidad `Panel`

| Campo | Tipo | Obligatorio | Regla |
|---|---|---|---|
| `Id` | `Guid` | Sí | Clave primaria, generada en el alta |
| `NodoId` | `Guid` | Sí | FK a `Nodo`, no vacía, inmutable |
| `Nodo` | `Nodo?` | Navegación | Relación con el nodo padre |
| `CodigoSerie` | `string` | Sí | Máx. 50, trim, único por `NodoId` |
| `Marca` | `string` | Sí | Máx. 100, trim |
| `Modelo` | `string` | Sí | Máx. 100, trim |
| `PotenciaWp` | `decimal` | Sí | Mayor a 0 |
| `FechaInstalacion` | `DateTime?` | No | Opcional |
| `Observaciones` | `string?` | No | Máx. 500, trim, vacío se normaliza a `null` |
| `FechaCreacion` | `DateTime` | Sí | UTC, inmutable |

No se agrega `FechaModificacion`.

### 1.2 Comportamiento

- Constructor privado para Entity Framework Core.
- Constructor público con todas las validaciones.
- `ActualizarDatos(codigoSerie, marca, modelo, potenciaWp, fechaInstalacion, observaciones)`.
- `ActualizarDatos` no modifica `Id`, `NodoId` ni `FechaCreacion`.

### 1.3 Excepciones

- `PanelNoEncontradoException(Guid id)` con propiedad `Id`.
- `CodigoPanelDuplicadoException` con el mismo patrón que `CodigoNodoDuplicadoException`.

### 1.4 Repositorio

`IPanelRepository` con:

- `AgregarAsync(Panel panel, CancellationToken)`.
- `ObtenerPorIdAsync(Guid id, CancellationToken)` incluyendo `Nodo` y `Nodo.ParqueSolar`.
- `ObtenerPorNodoAsync(Guid nodoId, CancellationToken)`.
- `ExisteCodigoAsync(string codigo, Guid nodoId, Guid? panelIdExcluido = null, CancellationToken)`.
- `Eliminar(Panel panel)`.
- `SaveChangesAsync(CancellationToken)`.

## 2) Aplicación (`SolarUY/Application`)

### 2.1 DTOs

- `PanelDto`: `Id`, `NodoId`, `CodigoSerie`, `Marca`, `Modelo`, `PotenciaWp`,
  `FechaInstalacion`, `FechaCreacion`.
- `PanelDetalleDto`: agrega `NodoCodigo`, `ParqueSolarId`, `ParqueSolarNombre` y
  `Observaciones`.
- `CrearPanelDto`: `NodoId`, `CodigoSerie`, `Marca`, `Modelo`, `PotenciaWp`,
  `FechaInstalacion`, `Observaciones` con DataAnnotations.
- `EditarPanelDto`: agrega `Id`; `NodoId` se incluye para navegación, pero el servicio no lo aplica.

### 2.2 Servicio

`IPanelService` / `PanelService` con:

- `ObtenerPanelesPorNodoAsync(Guid nodoId, CancellationToken)`.
- `ObtenerPanelAsync(Guid panelId, CancellationToken)`.
- `CrearPanelAsync(CrearPanelDto dto, CancellationToken)`.
- `EditarPanelAsync(EditarPanelDto dto, CancellationToken)`.
- `EliminarPanelAsync(Guid panelId, CancellationToken)`.

Reglas:

- Validar que el `Nodo` exista al crear.
- Validar unicidad de `CodigoSerie` dentro del `NodoId`, excluyendo el panel editado.
- Al editar, ignorar cualquier cambio de `NodoId`.
- Mapear entidades a DTOs.

## 3) Infraestructura (`SolarUY/Infrastructure`)

- Agregar `DbSet<Panel> Paneles` a `SolarUYDbContext`.
- Crear `PanelConfiguration`:
  - Tabla `Paneles`.
  - PK `Id` sin autogeneración.
  - Longitudes y precisión `decimal(18,2)`.
  - FK a `Nodos` con borrado en cascada.
  - Índice único `(NodoId, CodigoSerie)` llamado `IX_Paneles_NodoId_CodigoSerie`.
- Crear `PanelRepository` en `Infrastructure/Repositories`.
- Registrar `IPanelRepository` e `IPanelService` en `Program.cs`.
- Generar la migración `AgregarPaneles`.

## 4) Presentación (`SolarUY/Controllers`, `SolarUY/Views`)

### 4.1 Controlador

`PanelesController` con `[Authorize(Roles = "Administrador")]` y las acciones:

- `Index(Guid nodoId)`: listado filtrado por nodo.
- `Create(Guid nodoId)` GET/POST.
- `Details(Guid id)`.
- `Edit(Guid id)` GET/POST.
- `Delete(Guid id)` GET y `DeleteConfirmed(Guid id)` POST con anti-forgery.

Manejo de errores con `TempData["Mensaje"]` / `TempData["Error"]`, siguiendo req002/req003.

### 4.2 Vistas

Crear `Views/Paneles/Index.cshtml`, `Create.cshtml`, `Details.cshtml`, `Edit.cshtml` y
`Delete.cshtml`.

- `Index` muestra Código/Serie, Marca, Modelo, Potencia (Wp) y Fecha de alta.
- `Create` recibe `NodoId` oculto y muestra el nodo/parque.
- `Details` muestra el panel y la jerarquía Parque Solar / Nodo.
- `Edit` mantiene `Id` y `NodoId` ocultos; el nodo no es editable.
- `Delete` pide confirmación explícita.

Agregar en `Views/Nodos/Details.cshtml` el enlace "Administrar paneles" hacia
`Paneles/Index` con `nodoId`.

## 5) Tests y calidad

- Crear `SolarUY.Tests/Domain/PanelTests.cs` con tests de creación, validaciones,
  normalización, actualización e inmutabilidad.
- No crear tests unitarios en Application, Infrastructure ni Presentation.
- Ejecutar build, tests de la solución (incluye ArchUnitNET) y SonarQube al cerrar.
