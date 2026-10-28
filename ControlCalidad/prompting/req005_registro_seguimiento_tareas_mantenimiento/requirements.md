# Requerimientos: Registro y seguimiento de tareas de mantenimiento

## Descripción General

Como Administrador del sistema SolarUY, quiero registrar y dar seguimiento a las tareas de mantenimiento de los paneles fotovoltaicos (limpieza, revisión, reparación), asignando un responsable (personal especializado), con su tipo, descripción, fechas obligatorias, estado y observaciones opcionales, para garantizar el ininterrumpido funcionamiento del parque.

## Criterios de Aceptación

1. El Administrador debe poder acceder a la gestión de tareas de mantenimiento desde la ficha/detalle de un Panel existente perteneciente a un Nodo y a un Parque Solar.
2. El Administrador debe poder listar las tareas registradas para un Panel determinado, mostrando siempre el contexto (Parque Solar -> Nodo -> Panel).
3. El Administrador debe poder dar de alta una tarea de mantenimiento asociada a un Panel, indicando tipo de tarea, descripción, asignado a (personal especializado), fechas obligatorias, estado y observaciones opcionales.
4. El Administrador debe poder ver el detalle de una tarea de mantenimiento, incluyendo quién la asigna, quién la completa, todas las fechas y el contexto jerárquico.
5. El Administrador debe poder editar los datos de una tarea mientras su estado sea distinto a `Completada` o `Cancelada`. No se permite editar tareas en estado `Completada` o `Cancelada`.
6. El Administrador debe poder cambiar el estado de una tarea (Pendiente/EnProceso/Completada/Cancelada), registrando quién asigna (en alta/cambio relevante) y quién la completa al pasar a `Completada`.
7. El Administrador debe poder dar de baja (eliminar) una tarea, solicitando confirmación antes de la eliminación definitiva.
8. Solo los usuarios autenticados con el rol "Administrador" están autorizados a realizar estas operaciones (crear/asignar/editar/cancelar/cambiar estado).
9. El sistema debe informar el resultado de cada operación (éxito o error).

## Reglas de Negocio

- Una tarea de mantenimiento se registra a nivel de Panel. Se aplica una tarea por Panel (no se asigna a múltiples paneles en una misma tarea).
- La tarea pertenece obligatoriamente a un Panel. El Panel pertenece obligatoriamente a un Nodo. El Nodo pertenece obligatoriamente a un Parque Solar. Se debe mantener y mostrar siempre el contexto jerárquico.
- Tipo de tarea y descripción son obligatorias.
- AsignadoA (personal especializado) es obligatorio (se guarda como se indica).
- Estados válidos: `Pendiente`, `EnProceso`, `Completada`, `Cancelada`. No hay otros estados.
- Todas las fechas siguientes son obligatorias: fecha de alta/creación, fecha de asignación, fecha de vencimiento, fecha de inicio, fecha de finalización. 
- Se debe registrar quién asigna la tarea y quién la completa. Al pasar a estado `Completada`, se debe registrar quién la completa y la fecha de finalización debe estar presente/conforme a lo obligatorio.
- Observaciones son opcionales.
- No se permite editar una tarea si está en estado `Completada` o `Cancelada`.
- La baja de la tarea es física (eliminación definitiva). Se debe solicitar confirmación al Administrador antes de ejecutar la eliminación.
- Solo se pueden crear/editar/cambiar estado sobre Paneles existentes.
- Las validaciones deben aplicarse tanto a nivel de UI como a nivel de Dominio.
- Únicamente el rol "Administrador" puede acceder a estas operaciones.

## Fuera de Alcance

- Monitoreo en tiempo real, telemetría, alertas, historial agregado de generación y estado de falla del panel.
- Asignación masiva a múltiples paneles.
- Flujos distintos a los estados definidos.

