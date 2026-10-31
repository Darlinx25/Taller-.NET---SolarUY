# Requerimientos: Históricos de generación

## Descripción General

Como usuario autenticado de SolarUY, quiero consultar históricos de generación de parques, nodos y paneles, agrupados por día, mes o año, para analizar el desempeño del sistema sin depender de la última lectura efímera de telemetría.

## Criterios de Aceptación

1. La aplicación registra un único resumen diario por parque, nodo y panel con telemetría válida, identificado por entidad y fecha local UTC-3, actualizando potencia acumulada, lecturas, energía, potencia máxima, fallas y último estado.
2. La potencia promedio se calcula al consultar como potencia acumulada dividida por lecturas; la energía se integra con el intervalo real desde la lectura anterior, con tope de 15 segundos y 0 en la primera del día. Las lecturas duplicadas o fuera de orden se ignoran.
3. Los paneles acumulan lecturas en `EnFalla` y su último estado; nodos y parques conservan los valores consolidados. Los resúmenes persisten reinicios y se conservan indefinidamente, sin retención, desde la primera lectura posterior al despliegue.
4. Cualquier usuario autenticado, sin importar su rol, puede consultar los históricos en modo solo lectura. La interfaz filtra por tipo de entidad, entidad y rango de fechas, y agrupa por día, mes o año.
5. La pantalla muestra tarjetas con energía, potencia promedio, máxima y fallas; una tabla con período, entidad, energía, potencias, fallas y último estado; y un gráfico de evolución de la energía.
6. La baja de una entidad no elimina su histórico, aunque deja de aparecer en los selectores vigentes.
7. No se modifica el simulador ni el contrato de telemetría.

## Reglas de Negocio

- Solo se persisten resúmenes diarios; no se almacenan lecturas crudas ni promedios redondeados. La energía se calcula en kWh en la aplicación porque el simulador reinicia su contador y la telemetría de panel no informa energía.
- La potencia promedio de un período se calcula desde los acumulados de sus resúmenes diarios.
- Los estados válidos son `Operativo` y `EnFalla`. Un panel acumula una falla por cada lectura en `EnFalla`; un nodo o parque, por cada lectura con al menos un panel en falla. Su último estado es `EnFalla` si la última lectura tenía al menos un panel en ese estado.
- La agrupación usa la fecha local UTC-3. El acceso requiere autenticación, sin rol específico.

## Fuera de Alcance

- Lecturas crudas, backfill, retención y purga de históricos.
- Dashboard en tiempo real, SignalR, alertas y comandos al inversor.
- Cambios en CRUD, autenticación, roles, exportaciones y pronósticos.
