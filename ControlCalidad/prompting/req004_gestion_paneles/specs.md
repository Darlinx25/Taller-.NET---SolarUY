# Especificación Técnica: Gestión de Paneles

## Objetivo
Implementar el CRUD de Paneles fotovoltaicos asociados a un Nodo, respetando la jerarquía Parque Solar -> Nodo -> Panel, siguiendo el patrón utilizado en req003 (gestión desde la entidad padre).

## Modelo de Dominio

### Entidad Panel
Campos propuestos:

| Campo | Tipo | Requerido | Observaciones |
|---|---|---|---|
| Id | Guid/Int (según convención del proyecto) | Sí | Clave primaria |
| NodoId | FK | Sí | Relación con Nodo. Obligatoria. |
| CodigoSerie | string | Sí | Único dentro del mismo Nodo. |
| Marca | string | Sí | Obligatorio |
| Modelo | string | Sí | Obligatorio |
| PotenciaWp | decimal/int | Sí | > 0 |
| FechaInstalacion | DateTime? | No | Opcional. Si se agrega, debe ser válida. |
| Observaciones | string? | No | Opcional, texto breve. |
| FechaCreacion | DateTime | Sí | Seteado en alta, inmutable en edición. |
| FechaModificacion | DateTime? | No | Seteado solo en actualización. |

### Relación
- `Nodo` 1 -- * `Panel` (Un Nodo puede tener múltiples Paneles. Un Panel pertenece a un único Nodo)

## Reglas de Validación
- `CodigoSerie`: requerido, máx. longitud acorde al proyecto, único (NodoId + CodigoSerie).
- `Marca`, `Modelo`: requeridos.
- `PotenciaWp`: requerido, mayor a 0.
- `NodoId`: debe existir y ser válido.
- No permitir duplicados de `CodigoSerie` para el mismo `NodoId` al crear/editar.
- `FechaCreacion` no editable desde formularios.

## Interfaz / Navegación
- Acceso: desde la vista Detalle/Editar Nodo agregar un enlace/botón "Administrar Paneles".
- Listado: `/Paneles/Index?NodoId={id}` filtrado por Nodo. Debe mostrar: Código/Serie, Marca, Modelo, Potencia (Wp), Fecha de alta.
- Alta: `/Paneles/Create?NodoId={id}` (Nodo precargado y no editable).
- Detalle: `/Paneles/Details/{id}`
- Edición: `/Paneles/Edit/{id}`
- Eliminación: `/Paneles/Delete/{id}` con modal/confirmación "¿Está seguro que desea eliminar este panel? Esta acción es definitiva."

## Casos de Uso
| Acción | Descripción |
|---|---|
| Listar | Ver todos los paneles del Nodo. |
| Crear | Dar de alta panel asociado al Nodo con validaciones. |
| Ver | Mostrar ficha con datos del panel + datos del Nodo + Parque Solar. |
| Editar | Modificar Marca/Modelo/Potencia/FechaInstalacion/Observaciones/CodigoSerie respetando unicidad por Nodo. |
| Eliminar | Eliminar físicamente con confirmación. |

## Consideraciones
- Seguir **exactamente** la arquitectura, convenciones, naming y estructura de carpetas que ya existan en el proyecto.
- Respetar autorización por rol `Administrador` tal como se hace en req002/req003.
- Mostrar mensajes de éxito/error tras cada operación.
- Mantener trazabilidad con la jerarquía (mostrar "Parque Solar", "Nodo" en las vistas de Paneles).
- No agregar monitoreo, lecturas en tiempo real ni historial en este requerimiento. Eso corresponde a iteraciones futuras.
