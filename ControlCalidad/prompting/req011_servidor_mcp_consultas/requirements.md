# Requerimientos: Servidor MCP de consultas en lenguaje natural

## Descripcion General

Como usuario que opera SolarUY, quiero consultar el estado del sistema en lenguaje
natural desde un agente de IA (Antigravity) conectado a un servidor MCP de solo
lectura, para obtener informacion de parques, nodos, paneles, generacion,
historicos, alertas y tareas de mantenimiento sin navegar por las pantallas.

## Criterios de Aceptacion

1. SolarUY expone un servidor MCP accesible por Streamable HTTP en `/mcp`.
2. El agente Antigravity puede conectarse al servidor y descubrir sus herramientas.
3. El servidor ofrece herramientas de solo lectura para listar parques, consultar
   la topologia, el estado actual, los historicos, las alertas y las tareas de
   mantenimiento.
4. Cada herramienta devuelve datos reales de SolarUY y mensajes en espanol.
5. Las consultas historicas aceptan entidad (parque, nodo o panel), rango de fechas
   y agrupacion por dia, mes o anio.
6. Si no hay telemetria, la respuesta lo indica explicitamente.
7. Los errores de validacion o las entidades inexistentes se informan al agente
   con un mensaje claro, sin exponer detalles internos.
8. El servidor no crea, modifica ni elimina datos.
9. El acceso es anonimo y de solo lectura, pensado para el entorno local de la
   prueba de concepto.
10. Se puede agregar una nueva consulta sin modificar las existentes.
11. La aplicacion no requiere una clave de Gemini; el lenguaje natural lo resuelve
    el agente externo.
12. La conexion y el uso quedan documentados.

## Reglas de Negocio

- Solo consultas: no hay comandos de escritura ni control del inversor.
- La topologia y el estado actual reflejan los datos registrados y la ultima
  lectura de telemetria.
- Los historicos usan la agrupacion y la fecha local UTC-3 ya existentes.
- Las alertas muestran las activas y las 50 cerradas mas recientes.
- Las tareas de mantenimiento se pueden listar por panel o por estado.
- Las fechas y horas se informan en UTC, indicandolo.
- No se persisten preguntas ni respuestas.
- No hay migracion de base de datos.
- El endpoint MCP esta pensado para uso local; no se expone a internet.

## Fuera de Alcance

- Interfaz de chat dentro de SolarUY.
- Autenticacion OIDC o API key para el endpoint MCP (acceso anonimo de la POC).
- Escrituras, comandos al inversor o cambios de estado.
- Persistencia de consultas o respuestas.
- Cambios en la ingesta de telemetria, el simulador o el contrato interno.
- Limites de uso, cuotas o alta disponibilidad del servidor MCP.
- Consultas en lenguaje natural sin un agente externo.
