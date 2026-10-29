# Requerimientos: Simulación de telemetría y sensores ARM (mock del inversor)

## Descripción General

Como Administrador del sistema SolarUY, quiero que un mock de inversores y sensores ARM, ejecutado como componente separado de la aplicación monolítica, obtenga la topología registrada desde SolarUY, genere la telemetría de los parques solares y la envíe a la aplicación, para poder operar y verificar la prueba de concepto sin depender del hardware real ni de la API local del inversor, manteniendo el simulador desacoplado, reemplazable y preparado para convivir con un futuro balanceador de carga.

## Criterios de Aceptación

1. El mock se ejecuta como componente separado de la aplicación monolítica, en su propio proceso y contenedor (`simulador`), dentro del mismo stack Docker. La aplicación no genera telemetría.
2. La aplicación expone `GET /api/simulacion/topologia`, un endpoint interno autenticado con API key, que devuelve la topología registrada en modo solo lectura: parques, nodos con su estado, potencia nominal del inversor y paneles. La aplicación es la única que accede a MySQL.
3. El simulador obtiene la topología exclusivamente mediante `GET /api/simulacion/topologia`. No tiene cadena de conexión, no incluye el código de acceso a datos de la aplicación (`DbContext`, repositorios) y no accede a MySQL.
4. El simulador genera automáticamente una lectura de telemetría cada 5 segundos para cada parque solar registrado y para cada nodo en estado `Activo` con sus paneles.
5. Cada lectura de panel informa la potencia generada en watts (W) y su estado de salud: `Operativo` o `EnFalla`.
6. Cada lectura de nodo informa la potencia generada en W y la energía acumulada en kWh; cada lectura de parque consolida la potencia y la energía de sus nodos activos.
7. La generación sigue una curva solar diaria: 0 W fuera de la ventana de 07:00 a 19:00 (hora de Uruguay, UTC-3) y un pico en la mitad de la ventana (13:00), con una variación aleatoria acotada.
8. La potencia generada por un panel no puede ser negativa ni superar su potencia registrada (`PotenciaWp`); la potencia de un nodo no puede superar la potencia nominal de su inversor cuando ese dato está informado.
9. Un panel en estado `EnFalla` genera 0 W y vuelve automáticamente al estado `Operativo` después de la cantidad configurada de lecturas.
10. El simulador arranca con el stack y sigue funcionando aunque no haya parques, nodos o paneles registrados; en ese caso envía una lectura vacía, sin errores. Si la topología no se puede obtener, el simulador omite el tick, registra el error y conserva la última lectura almacenada en la aplicación.
11. El simulador envía cada lectura a la aplicación mediante `POST /api/simulacion/telemetria`, autenticado como componente interno con la misma API key compartida. La aplicación recibe y valida la lectura.
12. La aplicación conserva únicamente la última lectura recibida, de forma efímera: no se persiste la telemetría, no se agregan tablas y no se generan migraciones. La lectura se pierde al reiniciar la aplicación.
13. El Administrador puede consultar la última lectura consolidada (parques, nodos y paneles) mediante `GET /api/telemetria`, que responde `204 No Content` antes de la primera lectura y `200 OK` después.
14. Solo los usuarios autenticados con el rol `Administrador` pueden consultar la lectura. La topología y la ingesta solo aceptan pedidos autenticados del simulador mediante la API key.
15. Los endpoints internos rechazan con `401` los pedidos sin API key o con una key inválida, sin modificar la última lectura almacenada. Un payload de telemetría inválido se rechaza con `400`.
16. Un error al obtener la topología, generar, enviar o ingerir telemetría queda registrado en el log y no detiene ni a la aplicación ni al simulador. El simulador reintenta el envío de cada lectura antes de descartarla.
17. El sistema deja trazabilidad en el log de cada lectura generada y de cada envío, para permitir su verificación.
18. Esta iteración despliega una instancia de la aplicación y una del simulador. No incorpora balanceador de carga ni réplicas.
19. El diseño queda preparado para incorporar luego el balanceador y las réplicas de la aplicación (requerimiento de alta disponibilidad de la letra) sin modificar el simulador, los controladores ni los servicios de aplicación.

## Reglas de Negocio

- El mock sustituye a la API local del inversor: no se realizan llamadas a hardware real ni a servicios externos.
- La aplicación no simula: expone la topología registrada, recibe, valida, conserva y publica la telemetría que le envía el simulador.
- El simulador no accede a MySQL ni escribe en la base de datos. Toda la topología llega por el endpoint interno.
- El simulador es independiente de la aplicación: no comparte código con ella y su único acoplamiento es el contrato de los dos endpoints internos, de modo que pueda reemplazarse sin modificar la aplicación.
- La topología incluye todos los parques y todos sus nodos con el estado registrado. El simulador decide qué simular: solo los nodos con estado `Activo` (sin distinguir mayúsculas de minúsculas).
- Los paneles se obtienen de los nodos activos; no se inventan paneles, nodos, inversores ni parques que no estén registrados. Si los datos registrados de un panel cambian, la simulación usa los valores nuevos desde la siguiente lectura.
- Los parques registrados se incluyen aunque no tengan nodos activos; en ese caso informan 0 W, la energía acumulada que resulte de sus nodos activos (0 si no hay ninguno) y una lista de nodos vacía.
- Los estados válidos de un panel son `Operativo` y `EnFalla`. No existen otros estados en esta simulación.
- Todas las lecturas de un mismo tick comparten la misma fecha y hora UTC.
- Potencia del nodo = suma de la potencia de sus paneles, limitada a la potencia nominal del inversor cuando está informada (una potencia nominal de 0 kW limita el nodo a 0 W).
- Potencia del parque = suma de la potencia de sus nodos activos.
- Energía acumulada del nodo = energía acumulada anterior + (potencia del nodo en kW × duración del intervalo en horas).
- Energía acumulada del parque = suma de la energía acumulada de sus nodos activos. Si un nodo deja de estar `Activo`, deja de sumar y la energía del parque puede disminuir.
- La energía acumulada se cuenta desde el arranque del simulador y se reinicia al reiniciarlo. La última lectura se pierde cuando expira o cuando se reinicia la aplicación.
- Un panel en `EnFalla` genera 0 W durante todas las lecturas en las que permanece en ese estado.
- La falla es aleatoria, con probabilidad configurable, y se recupera automáticamente después de la cantidad configurada de lecturas. Un panel que se recupera no puede volver a fallar en esa misma lectura.
- La telemetría es efímera: no se persiste en MySQL y no produce cambios de esquema ni migraciones.
- Parámetros por defecto del simulador: intervalo de 5 segundos, probabilidad de falla del 5 % por panel en cada lectura, duración de falla de 6 lecturas, variación aleatoria de ±10 % y ventana de generación de 07:00 a 19:00 hora de Uruguay (UTC-3).
- La API key autentica al componente simulador, no a usuarios finales. No se expone en la interfaz web ni se registra en los logs.
- La misma API key protege `GET /api/simulacion/topologia` y `POST /api/simulacion/telemetria`. Si está vacía, la aplicación arranca, registra una advertencia y rechaza ambos endpoints con `401`.
- La consulta es de solo lectura: no permite enviar comandos al inversor.
- El simulador no modifica parques, nodos, paneles ni tareas de mantenimiento.
- No hay balanceador de carga ni réplicas en esta iteración: la aplicación y el simulador se comunican de forma directa dentro de la red del stack.
- Con un futuro balanceador se mantiene un único simulador y se escalan únicamente las instancias de la aplicación; la última lectura deberá compartirse entre réplicas para que cualquiera de ellas responda la misma telemetría.

## Fuera de Alcance

- Balanceador de carga, réplicas, caché compartida entre réplicas y sesiones compartidas en esta iteración; solo se documenta y se deja preparado el diseño.
- Dashboard en vivo y actualización de la interfaz mediante SignalR.
- Alertas y notificaciones automáticas por fallos de paneles.
- Persistencia de telemetría, históricos y reportes por día, mes o año.
- Consultas en lenguaje natural.
- Comandos al inversor: activar/desactivar paneles y activar/desactivar el volcado de energía a la red.
- Integración real con sensores ARM o con la API local del inversor.
- Pantalla de administración o control manual de la simulación.
- Cambios en el alta y edición de parques, nodos, inversores o paneles.
- Cambios en el proveedor de autenticación OpenID Connect o en el stub de desarrollo.
- Un usuario de MySQL dedicado y con permisos solo de lectura para el simulador; el simulador no accede a la base de datos.
