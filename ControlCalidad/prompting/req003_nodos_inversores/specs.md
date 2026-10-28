# Especificación Técnica: Gestión de Nodos e Inversores

## Objetivo

Implementar la gestión de Nodos e Inversores asociados a un Parque Solar, siguiendo la arquitectura por capas (Domain / Application / Infrastructure / Presentation), respetando las reglas del proyecto y el flujo SDD.

## 1) Dominio (`SolarUY/Domain`)

### 1.1 Entidades

#### `Inversor`
Representa el inversor asociado a un Nodo. Debe tener identidad propia y validaciones en dominio.

**Propiedades (sugeridas):**
- `Id` (Guid, privado en set)
- `Marca` (string, obligatorio, trim)
- `Modelo` (string, obligatorio, trim)
- `NumeroSerie` (string, obligatorio, trim) - o `Codigo`/identificador del inversor
- `PotenciaNominalkW` (decimal?, opcional o según validación; mantener consistente con reglas)
- `FechaCreacion` (DateTime.UtcNow)
- Constructor sin parámetros para EF Core.
- Constructor público con validaciones.
- Métodos para actualizar datos (mantener fecha de creación inmutable).

**Validaciones:**
- Campos obligatorios no pueden ser nulos/vacíos/espacios.
- Longitudes máximas razonables (definir constantes).
- Potencia no negativa si aplica.

#### `Nodo`
Representa un nodo dentro de un Parque Solar. Pertenece a un Parque Solar y tiene exactamente un Inversor asociado (1:1).

**Propiedades:**
- `Id` (Guid, privado en set)
- `ParqueSolarId` (Guid, obligatorio)
- `ParqueSolar` (navegación, opcional según configuración)
- `Codigo` (string, obligatorio, trim) - **único dentro del mismo ParqueSolar**
- `Descripcion` (string?, opcional)
- `Estado` (string/enum o string con valores válidos; definir si necesario)
- `Inversor` (navegación 1:1, obligatorio)
- `InversorId` (Guid, obligatorio)
- `FechaCreacion` (DateTime.UtcNow)

**Comportamiento:**
- Constructor para crear Nodo + Inversor asociado, validando ambos.
- Método `ActualizarDatos(...)` para editar datos del Nodo y/o permitir actualización coordinada con el Inversor (respetando responsabilidades).
- Validar código único por parque (se valida en Application/Infrastructure, pero reglas básicas en Dominio).

**Relación:**
- `ParqueSolar` 1:N `Nodo`
- `Nodo` 1:1 `Inversor` (con clave/foreign key bien configurada)

### 1.2 Excepciones de Dominio
Crear excepciones necesarias, siguiendo el estilo existente:
- `NodoNoEncontradoException` (similar a `ParqueSolarNoEncontradoException`)
- `CodigoNodoDuplicadoException` / `NodoDuplicadoException` (similar a `NombreParqueDuplicadoException`)
- Otras excepciones si aplican según validaciones

### 1.3 Repositorios (Contratos)
Ampliar o crear contratos:
- `INodoRepository`: `AgregarAsync`, `ObtenerPorIdAsync(Guid id, ...)` (incluir Inversor/ParqueSolar con `Include` cuando corresponda), `ObtenerPorParqueAsync(Guid parqueSolarId, ...)`, `ExisteCodigoAsync(string codigo, Guid parqueSolarId, Guid? nodoIdExcluido = null, ...)`, `Eliminar(Nodo nodo)`, `SaveChangesAsync`
- `IInversorRepository` (opcional si se gestiona vía Navegación/Agregado; evaluar según diseño). Dado que se crea junto con Nodo y relación 1:1, puede gestionarse a través de `Nodo`/`DbContext`, pero mantener coherente con el patrón Repository usado.

## 2) Aplicación (`SolarUY/Application`)

### 2.1 DTOs
Crear DTOs siguiendo convenciones:
- `NodoDto` / `InversorDto` / `NodoDetalleDto` (para listado/detalle)
- `CrearNodoInversorDto` (datos unificados para crear Nodo + Inversor desde un mismo formulario)
- `EditarNodoInversorDto` (datos para editar Nodo + Inversor)
- Validaciones con DataAnnotations donde aplique

### 2.2 Servicios
Crear `INodoService` / `NodoService` con casos de uso:
- `ObtenerNodosPorParqueAsync(Guid parqueSolarId, CancellationToken)`
- `ObtenerNodoAsync(Guid nodoId, CancellationToken)`
- `CrearNodoInversorAsync(CrearNodoInversorDto dto, CancellationToken)`
- `EditarNodoInversorAsync(EditarNodoInversorDto dto, CancellationToken)`
- `EliminarNodoAsync(Guid nodoId, CancellationToken)`

**Lógica:**
- Validar existencia del ParqueSolar
- Validar unicidad de código del Nodo dentro del parque (excluyendo el propio al editar)
- Crear entidades de Dominio, invocar repositorios, `SaveChangesAsync`
- Mapear a DTOs
- Lanzar excepciones de dominio/aplicación según corresponda

### 2.3 Excepciones/Validaciones
Reutilizar patrón de `ParqueSolarService`: capturar excepciones de dominio en controlador para mostrar mensajes amigables.

## 3) Infraestructura (`SolarUY/Infrastructure`)

### 3.1 Persistencia
- Añadir `DbSet<Nodo> Nodos` y `DbSet<Inversor> Inversores` en `SolarUYDbContext`
- Crear configuraciones Fluent API:
  - `NodoConfiguration`: tabla `Nodos`, PK, FK a `ParquesSolares`, índice único compuesto `(ParqueSolarId, Codigo)`, relación 1:1 con `Inversor` (FK en `Inversor` o compartida; definir claramente)
  - `InversorConfiguration`: tabla `Inversores`, PK, relación 1:1 con `Nodo`
- Aplicar configuraciones vía `ApplyConfigurationsFromAssembly`

### 3.2 Repositorios
- Implementar `NodoRepository` en `Infrastructure/Repositories`
- Incluir `Include(n => n.Inversor).Include(n => n.ParqueSolar)` cuando se obtenga por Id/listado necesario
- Validaciones de unicidad con filtros por `ParqueSolarId` y exclusión de `nodoIdExcluido`

### 3.3 Migraciones
- Generar migración con `dotnet ef migrations add AgregarNodosEInversores` (o nombre descriptivo)
- Aplicar con `dotnet ef database update` en el flujo de desarrollo/docker según corresponda

## 4) Presentación (`SolarUY/Controllers`, `SolarUY/Views`)

### 4.1 Controlador
Crear `NodosController` (o acciones relacionadas) con `[Authorize(Roles = "Administrador")]`.

**Acciones sugeridas:**
- `Index(Guid parqueId, ...)` - listar nodos del parque
- `Create(Guid parqueId, ...)` GET/POST - formulario unificado Nodo+Inversor
- `Details(Guid id, ...)` GET - detalle nodo + inversor
- `Edit(Guid id, ...)` GET/POST - editar nodo + inversor
- `Delete(Guid id, ...)` GET + `DeleteConfirmed` POST con `[ValidateAntiForgeryToken]`

**Comportamiento:**
- Acceso desde ficha de ParqueSolar (añadir enlace/botón en `ParquesSolares/Details.cshtml` o donde corresponda)
- Manejo de errores: `NodoNoEncontradoException`, `CodigoNodoDuplicadoException`, validar existencia de parque
- Usar `TempData["Mensaje"]` / `TempData["Error"]` igual que en `req002`

### 4.2 Vistas
Crear vistas bajo `Views/Nodos/` o integradas:
- `Index.cshtml` (listado por parque)
- `Create.cshtml` (formulario Nodo + Inversor)
- `Details.cshtml` (detalle)
- `Edit.cshtml` (edición)
- `Delete.cshtml` (confirmación)

Actualizar `Views/ParquesSolares/Details.cshtml` para incluir enlace a gestión de nodos.

## 5) Calidad y Verificación

- **Tests unitarios:** Agregar tests en `SolarUY.Tests/Domain/` para validaciones de `Nodo` e `Inversor` (solo Dominio, obligatorio si hay lógica nueva)
- **ArchUnitNET:** Ejecutar `docker compose --profile tools run --rm tools dotnet test SolarUY.sln` hasta verde
- **Build:** `docker compose --profile tools run --rm tools dotnet build SolarUY.sln` sin errores/warnings críticos
- **Funcional:** Smoke test del flujo crear/listar/detalle/editar/eliminar Nodo+Inversor
- **SonarQube:** Ejecutar `./Controlcalidad/SonarQube/analizar.sh` (o script correspondiente) y registrar hallazgos, marcar tarea 7.4

## 6) Restricciones

- Seguir exactamente el flujo SDD: no codear sin `requirements.md`, `specs.md`, `tasks.md` aprobados
- No modificar `CodeQuality.Tests` sin autorización explícita
- Mantener estilo y patrones existentes (DTOs, servicios, repositorios, controladores MVC)
- No traducir sufijos/prefijos técnicos (Repository, Service, Exception, Controller)
- Validaciones en Dominio donde aplique, presentación "tonta"
- Unicidad de código de Nodo **únicamente por ParqueSolar**
- Relación 1:1 Nodo-Inversor
- Baja física con confirmación
- Acceso desde detalle/listado de ParqueSolar

## 7) Detalles de implementación a definir al codificar

- **Nombres exactos:** seguir estilo español para entidades/reglas, sufijos/prefijos en inglés (Repository/Service/Exception/Controller).
- **EF Core:** relación 1:1 Nodo-Inversor. Configurar FK correctamente para evitar ambigüedad.
- **Unicidad:** índice único `(ParqueSolarId, Codigo)` en `Nodos`.
- **Validaciones:** obligatorias en Dominio; DTOs con DataAnnotations para UX.
- **Errores:** usar excepciones de dominio en Application/Domain y mapear a mensajes en Controller (igual que `req002`).
- **Sin migración si no cambia schema:** **NO** aplicar esta regla aquí; sí se esperan cambios de esquema (nuevas tablas/relaciones), por lo que **sí** debe generarse migración.
- **Tests:** solo Dominio obligatorios si hay nueva lógica.
