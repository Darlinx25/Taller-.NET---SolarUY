# Requerimientos: Registro y seguimiento de tareas de mantenimiento

## Descripción General

Como Administrador del sistema SolarUY, quiero registrar y dar seguimiento a las tareas de mantenimiento de los paneles fotovoltaicos (limpieza, revisión, reparación), asignando un responsable (personal especializado), con su tipo, descripción, fechas obligatorias, estado y observaciones opcionales, para garantizar el ininterrumpido funcionamiento del parque.

## Criterios de Aceptación

1. El Administrador debe poder acceder a la gestión de tareas de mantenimiento desde la ficha/detalle de un Panel existente perteneciente a un Nodo y a un Parque Solar.
2. El Administrador debe poder listar las tareas registradas para un Panel determinado, mostrando siempre el contexto (Parque Solar -> Nodo -> Panel).
3. El Administrador debe poder dar de alta una tarea de mantenimiento asociada a un Panel, indicando tipo de tarea, descripción, asignado a (personal especializado), fecha de vencimiento y observaciones opcionales. Las fechas de creación, asignación, inicio y finalización se registran automáticamente según el ciclo de vida de la tarea.
4. El Administrador debe poder ver el detalle de una tarea de mantenimiento, incluyendo quién la asigna, quién la completa, todas las fechas y el contexto jerárquico.
5. El Administrador debe poder editar los datos de una tarea mientras su estado sea distinto a `Completada` o `Cancelada`. No se permite editar tareas en estado `Completada` o `Cancelada`.
6. El Administrador debe poder cambiar el estado de una tarea respetando las transiciones permitidas (`Pendiente -> EnProceso`, `Pendiente -> Cancelada`, `EnProceso -> Completada`, `EnProceso -> Cancelada`). El sistema registra automáticamente la fecha de inicio al pasar a `EnProceso` y la fecha de finalización al pasar a `Completada` o `Cancelada`. Al completar, se registra quién completa.
7. El Administrador debe poder dar de baja (eliminar) una tarea, solicitando confirmación antes de la eliminación definitiva.
8. Solo los usuarios autenticados con el rol "Administrador" están autorizados a realizar estas operaciones (crear/asignar/editar/cancelar/cambiar estado).
9. El sistema debe informar el resultado de cada operación (éxito o error).

## Reglas de Negocio

- Una tarea de mantenimiento se registra a nivel de Panel. Se aplica una tarea por Panel (no se asigna a múltiples paneles en una misma tarea).
- La tarea pertenece obligatoriamente a un Panel. El Panel pertenece obligatoriamente a un Nodo. El Nodo pertenece obligatoriamente a un Parque Solar. Se debe mantener y mostrar siempre el contexto jerárquico.
- `TipoTarea`, `Descripcion`, `AsignadoA` y `FechaVencimiento` son obligatorios.
- `Observaciones` es opcional, se normaliza con trim y tiene un máximo de 1000 caracteres.
- `FechaCreacion` y `FechaAsignacion` se registran automáticamente al dar de alta la tarea.
- `FechaVencimiento` es ingresada por el Administrador.
- `FechaInicio` se registra automáticamente al pasar a `EnProceso`. Si la tarea se cancela desde `Pendiente`, no tiene fecha de inicio.
- `FechaFinalizacion` se registra automáticamente al pasar a `Completada` o `Cancelada`.
- `QuienAsigna` se toma del usuario autenticado que crea la tarea y no se edita desde el formulario.
- `QuienCompleta` es obligatorio al pasar a `Completada`. Si el formulario no lo informa, se toma del usuario autenticado.
- Estados válidos: `Pendiente`, `EnProceso`, `Completada`, `Cancelada`. No hay otros estados.
- Transiciones permitidas:
  - `Pendiente -> EnProceso`
  - `Pendiente -> Cancelada`
  - `EnProceso -> Completada`
  - `EnProceso -> Cancelada`
- No se permite `Pendiente -> Completada` ni retroceder desde `EnProceso`, `Completada` o `Cancelada`.
- No se permite editar una tarea si está en estado `Completada` o `Cancelada`.
- La baja de la tarea es física (eliminación definitiva). Se debe solicitar confirmación al Administrador antes de ejecutar la eliminación.
- Solo se pueden crear/editar/cambiar estado sobre Paneles existentes.
- Las validaciones deben aplicarse tanto a nivel de UI como a nivel de Dominio.
- Únicamente el rol "Administrador" puede acceder a estas operaciones.

## Fuera de Alcance

- Monitoreo en tiempo real, telemetría, alertas, historial agregado de generación y estado de falla del panel.
- Asignación masiva a múltiples paneles.
- Flujos distintos a los estados definidos.

