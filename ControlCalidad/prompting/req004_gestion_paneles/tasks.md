# Tareas: req004 - Gestión de Paneles

## Objetivo
Implementar CRUD de Paneles asociado a un Nodo siguiendo el flujo definido en requirements.md y specs.md.

## Tareas

### 1) Dominio y Datos
- [ ] Analizar entidades existentes (`ParqueSolar`, `Nodo`, `Inversor`) para mantener convenciones.
- [ ] Crear entidad `Panel` con las propiedades definidas en `specs.md` y relaciones con `Nodo`.
- [ ] Configurar relación 1:N (Nodo -> Paneles) en DbContext/mapeo.
- [ ] Agregar restricción/índice para garantizar unicidad `(NodoId, CodigoSerie)`.
- [ ] Crear/actualizar migración correspondiente.
- [ ] Aplicar migración y verificar integridad con datos existentes.

### 2) Validaciones y Reglas
- [ ] Agregar DataAnnotations/Validaciones o validaciones de dominio para campos obligatorios y `PotenciaWp > 0`.
- [ ] Validar unicidad de `CodigoSerie` por `NodoId` (crear y editar).
- [ ] Asegurar que `FechaCreacion` sea inmutable.
- [ ] Validar existencia del `Nodo` al crear.

### 3) Lógica / Servicios (si aplica)
- [ ] Crear/ajustar `IPanelService` / lógica de aplicación si el proyecto utiliza servicios.
- [ ] Centralizar validaciones de negocio (unicidad + existencia de padre).

### 4) Controlador y Autorización
- [ ] Crear `PanelesController` con acciones `Index`, `Details`, `Create` (GET/POST), `Edit` (GET/POST), `Delete` (GET/POST).
- [ ] Filtrar por `NodoId` en `Index/Create`.
- [ ] Aplicar atributo/autorización para rol `Administrador` (igual que req002/req003).
- [ ] Agregar TempData/mensajes de éxito/error.
- [ ] Protección ante acceso a paneles de nodos inexistentes.

### 5) Vistas (UI)
- [ ] Agregar enlace "Administrar Paneles" en la vista de Detalle/Edición del Nodo.
- [ ] Crear vistas `Index`, `Details`, `Create`, `Edit`, `Delete` siguiendo el estilo visual existente.
- [ ] Precargar `NodoId` en el formulario de alta (solo lectura o hidden).
- [ ] Mostrar jerarquía: Parque Solar / Nodo / Panel en encabezados/breadcrumbs si existe esa convención.
- [ ] Agregar confirmación para eliminación.
- [ ] Validaciones client-side/server-side visibles al usuario.

### 6) Integración y Pruebas
- [ ] Probar flujo completo: desde Nodo -> listar paneles -> crear -> editar -> ver detalle -> eliminar con confirmación.
- [ ] Probar regla: dos paneles con mismo `CodigoSerie` en el mismo Nodo -> debe fallar.
- [ ] Probar regla: mismo `CodigoSerie` en distinto Nodo -> debe permitir.
- [ ] Verificar solo `Administrador` puede acceder.
- [ ] Verificar mensajes de éxito/error.
- [ ] Revisar que no se rompe navegación de Parques/Nodos existentes.
- [ ] Compilar solución sin errores.

## Notas
- Mantener nomenclatura en español/inglés consistente con lo ya hecho en el proyecto.
- Seguir el patrón de navegación "desde la ficha del padre" usado en `req003`.
- No implementar monitoreo en tiempo real ni histórico. Fuera de alcance de este requerimiento.
