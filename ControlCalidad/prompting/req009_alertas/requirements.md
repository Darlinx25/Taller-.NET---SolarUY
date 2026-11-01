# Requerimientos: Alertas por fallos en paneles

## Descripción General

Como usuario del sistema SolarUY (rol: Todos), quiero que el sistema detecte y registre los fallos de los paneles y me los muestre, con un aviso en vivo, para reaccionar rápidamente ante la existencia de un fallo.

## Criterios de Aceptación

1. El sistema detecta un fallo cuando un panel informa el estado `EnFalla` en la telemetría que recibe, y registra una alerta.
2. Una alerta registra el parque solar, el nodo y el panel afectados, y la fecha y hora de inicio. Mientras el panel siga en falla, la alerta permanece activa y no se genera otra alerta para ese mismo panel.
3. La alerta se cierra, registrando la fecha y hora de fin, cuando el panel informa el estado `Operativo` o cuando el panel deja de aparecer en la telemetría porque su nodo dejó de estar `Activo` o fue dado de baja.
4. Las alertas se conservan en la base de datos con su historial: no se eliminan al cerrarse ni al dar de baja el panel, el nodo o el parque.
5. Existe una pantalla de alertas que muestra las alertas activas (parque, nodo, panel, inicio y duración) y las 50 alertas cerradas más recientes (parque, nodo, panel, inicio, fin y duración).
6. Todas las pantallas muestran en el menú un indicador con la cantidad de alertas activas, que se actualiza en vivo, sin recargar la página, cuando se abre o se cierra una alerta.
7. Cualquier visitante puede ver la pantalla de alertas y el indicador, sin iniciar sesión ni tener un rol específico. El acceso es de solo lectura.
8. Si no hay alertas activas, la pantalla lo informa claramente.
9. Una lectura repetida o anterior a la ya registrada no duplica ni reabre alertas.
10. El registro de alertas no modifica el simulador ni el contrato de telemetría.

## Reglas de Negocio

- Los estados válidos de un panel son `Operativo` y `EnFalla`. Una alerta se genera solo por paneles; no hay alertas de nodos, parques ni inversores.
- Hay a lo sumo una alerta activa por panel. Si un panel se recupera y vuelve a fallar, se registra una alerta nueva; la anterior queda en el historial.
- Las alertas se evalúan con cada lectura válida de telemetría. Una lectura sin parques no abre ni cierra alertas.
- Cuando una lectura hace que se abra o se cierre una alerta, el registro de la alerta forma parte de la recepción de la lectura: si no se puede registrar, la lectura se rechaza y el simulador reintenta el envío, igual que con los históricos.
- Los nombres y códigos del parque, el nodo y el panel se guardan en la alerta tal como estaban al abrirse, para conservar el historial aunque luego cambien o se den de baja.
- La duración de una alerta cerrada es la diferencia entre su fin y su inicio; la de una alerta activa se calcula hasta el momento de la consulta.
- Las fechas y horas se guardan en UTC y la pantalla las muestra en UTC, indicándolo.
- El historial se conserva indefinidamente, sin retención ni purga, desde la primera lectura posterior al despliegue.
- La pantalla y el indicador son de solo lectura: no permiten atender, reconocer ni eliminar alertas.

## Fuera de Alcance

- Acuse de recibo o atención manual de alertas, y cualquier acción de escritura sobre ellas.
- Notificaciones por correo electrónico, SMS u otros canales.
- Alertas de nodos, parques o inversores, y alertas por umbrales de potencia o de tiempo.
- Filtros, paginación, búsqueda y exportación del historial; retención y purga.
- Actualización en vivo del listado de la pantalla de alertas (el indicador del menú sí se actualiza).
- Mostrar las fechas en hora de Uruguay.
- Cambios en el simulador, en el contrato de telemetría, en el CRUD, en la autenticación y en los roles.
- Comandos al inversor.
