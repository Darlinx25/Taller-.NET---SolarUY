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
12. La aplicación conserva la última lectura recibida de forma efímera: se pierde al reiniciar la aplicación. El mock no persiste la telemetría cruda, no agrega tablas ni genera migraciones. Los resúmenes diarios que se derivan de cada lectura válida para los históricos los persiste `req007_historicos_generacion`, sin cambiar el contrato ni el simulador.
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
- La lectura es efímera: este requerimiento no persiste telemetría en MySQL ni produce cambios de esquema ni migraciones. La persistencia de resúmenes históricos es responsabilidad de `req007_historicos_generacion`.
- Parámetros por defecto del simulador: intervalo de 5 segundos, probabilidad de falla del 5 % por panel en cada lectura, duración de falla de 6 lecturas, variación aleatoria de ±10 % y ventana de generación de 07:00 a 19:00 hora de Uruguay (UTC-3).
- La API key autentica al componente simulador, no a usuarios finales. No se expone en la interfaz web ni se registra en los logs.
- La misma API key protege `GET /api/simulacion/topologia` y `POST /api/simulacion/telemetria`. Si está vacía, la aplicación arranca, registra una advertencia y rechaza ambos endpoints con `401`.
- La consulta es de solo lectura: no permite enviar comandos al inversor.
- El simulador no modifica parques, nodos, paneles ni tareas de mantenimiento.
- No hay balanceador de carga ni réplicas en esta iteración: la aplicación y el simulador se comunican de forma directa dentro de la red del stack.
- Con un futuro balanceador se mantiene un único simulador y se escalan únicamente las instancias de la aplicación; la última lectura deberá compartirse entre réplicas para que cualquiera de ellas responda la misma telemetría.

## Fuera de Alcance

- Balanceador de carga, réplicas, caché compartida entre réplicas y sesiones compartidas en esta iteración; solo se documenta y se deja preparado el diseño.
- Dashboard en vivo y actualización de la interfaz mediante SignalR (cubierto por `req008_dashboard`).
- Alertas y notificaciones automáticas por fallos de paneles.
- Persistencia de telemetría, históricos y reportes por día, mes o año (los históricos los cubre `req007_historicos_generacion`).
- Consultas en lenguaje natural.
- Comandos al inversor: activar/desactivar paneles y activar/desactivar el volcado de energía a la red.
- Integración real con sensores ARM o con la API local del inversor.
- Pantalla de administración o control manual de la simulación.
- Cambios en el alta y edición de parques, nodos, inversores o paneles.
- Cambios en el proveedor de autenticación OpenID Connect o en el stub de desarrollo.
- Un usuario de MySQL dedicado y con permisos solo de lectura para el simulador; el simulador no accede a la base de datos.

---

# Revisión 1: el mock pasa a simular una placa ARM con WebAssembly

> Estado: **Aprobada (Gate R). La spec de esta revisión está pendiente de aprobación (Gate S).** Todo lo anterior en este documento describe lo que se implementó en la primera versión (worker .NET en contenedor) y se conserva como registro. Esta sección describe el cambio pedido; donde contradice a lo anterior, rige esta revisión.

## Motivo del cambio

El mock debe simular una placa ARM (inversor y sensores) usando WebAssembly. La primera versión es un servicio .NET común, que no representa a la placa. El cambio reemplaza ese servicio por una placa simulada que corre como módulo WebAssembly, manteniendo la funcionalidad del sistema y la forma en que se levanta.

## Descripción General (revisión)

Como Administrador del sistema SolarUY, quiero que el mock sea un módulo sencillo y separado, una placa ARM simulada que corre como WebAssembly y se levanta junto con el resto del stack Docker, que obtenga la topología registrada y envíe la telemetría de los parques únicamente al balanceador de carga, para verificar la prueba de concepto sin hardware real y con una simulación que represente a la placa.

## Criterios de Aceptación (revisión)

R1. La placa simulada es un módulo separado de la aplicación monolítica: no comparte código con ella ni accede a MySQL.
R2. La lógica de la placa se compila a WebAssembly y se ejecuta en su propio contenedor (`simulador`), sin navegador ni intervención manual.
R3. El contenedor `simulador` se levanta junto con el resto del stack con `docker compose up`, igual que en la primera versión, y empieza a enviar telemetría sin que nadie abra una página ni ingrese datos.
R4. La placa se comunica únicamente con el balanceador de carga. No conoce las réplicas de la aplicación ni les envía pedidos directamente.
R5. La placa obtiene la topología con `GET /api/simulacion/topologia` y envía la telemetría con `POST /api/simulacion/telemetria`. El contrato de ambos endpoints no cambia, por lo que la aplicación y sus servicios no se modifican por esta revisión.
R6. La placa mantiene las reglas de simulación de la primera versión: lectura cada 5 segundos, curva solar de 07:00 a 19:00 (UTC-3), variación aleatoria, fallas y recuperación de paneles, límite por potencia del panel y del inversor, y energía acumulada.
R7. La placa mantiene el comportamiento de la primera versión ante errores y ausencia de datos: sigue funcionando sin parques, omite el tick si no obtiene la topología, reintenta el envío y registra cada lectura y cada envío en el log del contenedor.
R8. La clave de la API se configura por variable de entorno, como hoy (`SIMULACION_API_KEY`), y no se registra en los logs.
R9. Deja de existir el worker .NET actual y su ejecución como proceso .NET común: el servicio `simulador` pasa a ejecutar el módulo WebAssembly.
R10. Todo lo relacionado con el mock (código, tests, definición del contenedor y documentación propia) está agrupado en una única carpeta del repositorio.

## Reglas de Negocio (revisión)

- La placa simulada sustituye a la placa ARM real: no se hacen llamadas a hardware.
- La energía acumulada se cuenta desde el arranque de la placa y se reinicia al reiniciarla, igual que antes.
- Las reglas de simulación, de validación de la ingesta y de autenticación por API key de los endpoints internos no cambian.
- El módulo sigue siendo independiente: su único acoplamiento con la aplicación es el contrato de los dos endpoints internos.

## Decisión sobre cómo se ejecuta el WebAssembly

Para que el mock se levante solo con Docker no puede depender de un navegador abierto. Por eso se propone compilar el código .NET de la placa a WebAssembly y ejecutarlo dentro del contenedor con un entorno que no es un navegador (Node.js). Es la misma tecnología WebAssembly que usa Blazor, pero sin interfaz web: la placa corre sola y se verifica por logs, como el mock actual.

Alternativa descartada: una aplicación Blazor WebAssembly con interfaz, que solo corre mientras hay una pestaña abierta y obligaría a iniciarla a mano. Si más adelante se necesita ver la placa en una página, se puede agregar una vista de estado, pero no sería lo que levanta la telemetría.

## Fuera de Alcance (revisión)

- Emular un procesador ARM real o ejecutar firmware ARM.
- Interfaz web de la placa.
- Cambios en los endpoints internos, en la ingesta o en los servicios de la aplicación.
- Mantener en paralelo el worker .NET anterior y la placa en WebAssembly.

---

# Revisión 2: control de la simulación desde el dashboard

> Estado: **Propuesta, pendiente de aprobación (Gate R).** Se suma a la Revisión 1 (mock en WebAssembly). Donde contradice a lo anterior, rige esta revisión.

## Motivo del cambio

Hoy el mock genera según la hora real y solo falla por azar, por lo que fuera del horario solar el dashboard muestra 0 W y no hay forma de provocar un fallo para verificar el comportamiento. Se pide poder controlar la simulación desde el propio dashboard de SolarUY, sin depender de la hora de la computadora ni del azar.

## Descripción General (revisión)

Como Administrador del sistema SolarUY, quiero forzar desde el dashboard que la simulación (que ya corre con el stack) genere como a pleno sol sin importar la hora, y forzar con un botón la falla de los paneles, para verificar la generación, el estado de salud y las alertas en cualquier momento y ver el resultado actualizarse en vivo.

## Criterios de Aceptación (revisión)

R11. La simulación ya está en marcha desde que se levanta el stack: el dashboard no la inicia. Muestra un botón "Forzar día" que hace que el mock genere como a pleno sol sin importar la hora. Mientras está forzado, el mismo botón pasa a "Volver a hora real" y el mock vuelve a generar según la hora real.
R12. Con la simulación forzada activa, el mock genera como si fueran las 13:00 (pico de generación) sin importar la hora de la computadora ni la hora real de Uruguay, con la variación aleatoria y las reglas de potencia de siempre (ningún panel supera su `PotenciaWp` ni ningún nodo la potencia nominal de su inversor).
R12b. Con el día forzado, el mock sigue produciendo fallas aleatorias de paneles con la probabilidad y la duración configuradas, igual que con la hora real. Forzar el día solo cambia la hora que usa la curva solar.
R13. El dashboard indica con claridad cuándo el día está forzado ("Día forzado activo").
R14. El dashboard muestra un botón "Simular falla". Al presionarlo, todos los paneles de los nodos activos pasan a `EnFalla` (0 W) durante la cantidad configurada de lecturas y luego vuelven solos a `Operativo`, como una falla aleatoria.
R15. "Simular falla" funciona con el día forzado o con la hora real.
R16. Los botones los ve y los usa únicamente el rol `Administrador`. Los demás usuarios siguen viendo el dashboard sin botones, y los pedidos de control de quien no sea administrador se rechazan (`401` sin sesión, `403` sin el rol).
R17. Al presionar un botón, el dashboard confirma de inmediato que la orden se registró y el efecto se ve en la siguiente lectura del mock (como máximo un intervalo de 5 segundos después).
R18. El dashboard se actualiza solo, sin recargar la página y por SignalR, cuando llega la lectura con el cambio, tal como ya hace con cualquier otra lectura.
R19. Las órdenes se comparten entre todas las réplicas de la aplicación: da igual en cuál se presione el botón o cuál reciba al mock.
R20. El mock sigue siendo un cliente que solo habla con el balanceador: no abre puertos ni recibe pedidos. La aplicación nunca le envía órdenes directamente; el mock consulta cuál es la orden vigente en cada ciclo.
R21. Si el mock no puede consultar la orden vigente, mantiene la última que conocía y sigue enviando telemetría.
R22. Los endpoints internos que usa el mock siguen protegidos con la API key; el control desde el dashboard usa la sesión del usuario.

## Reglas de Negocio (revisión)

- El día forzado y la falla forzada son órdenes de la aplicación al mock; el mock sigue siendo quien genera la telemetría.
- El estado de la orden vigente es efímero, como la última lectura: se pierde al reiniciar la aplicación y el mock vuelve al comportamiento según la hora real.
- Reiniciar el mock no cambia la orden vigente: al volver a consultar, retoma el modo que tenía la aplicación.
- La simulación forzada solo cambia la hora que usa la curva solar; no modifica parques, nodos, paneles ni tareas de mantenimiento.
- La falla forzada usa el mismo mecanismo de falla y recuperación que la aleatoria: un panel que se recupera no vuelve a fallar en esa misma lectura.
- Las lecturas generadas en modo forzado se tratan como cualquier otra lectura: se almacenan, se muestran en el dashboard, se incluyen en los resúmenes históricos (req007) y las fallas generan alertas (req009).
- Esta revisión amplía la regla de req008 que define el dashboard como estrictamente de solo lectura: sigue siéndolo respecto de los datos de generación, y los botones de control para el rol `Administrador` son la única excepción.

## Fuera de Alcance (revisión)

- Elegir la hora simulada, el parque, el nodo o el panel a fallar.
- Falla sostenida hasta apagarla manualmente, o fallar un solo panel.
- Día solar acelerado.
- Enviar comandos al inversor real o activar o desactivar paneles.
- Cambios en las reglas de ingesta y validación de la telemetría.
- Historial o bitácora de las órdenes dadas desde el dashboard.

---

# Revisión 3: recuperación de fallas probabilística

> Estado: **Aprobada por pedido directo del usuario.** Reemplaza la duración fija de las fallas y las probabilidades por defecto de la primera versión; donde contradice a lo anterior, rige esta revisión.

## Motivo del cambio

Hasta ahora una falla duraba siempre el mismo número de lecturas (6, unos 30 s) y era poco frecuente (5 %). Se pide que las fallas sean más frecuentes y que su duración no sea fija, sino que dependa del azar.

## Criterios de Aceptación (revisión)

R23. Un panel operativo falla en cada lectura con una probabilidad configurable, que por defecto es del 20 %.
R24. Mientras un panel está en falla, en cada lectura tiene una probabilidad configurable de recuperarse, que por defecto es del 40 %. Una falla dura entonces una cantidad variable de lecturas (al menos una, y 2,5 en promedio, unos 12,5 segundos con lecturas cada 5 segundos).
R25. Un panel que se recupera no puede volver a fallar en esa misma lectura.
R26. La falla provocada con el botón "Simular falla" usa la misma regla de recuperación que la aleatoria.
R27. Con el día forzado siguen las fallas aleatorias, con las mismas probabilidades.

## Reglas de Negocio (revisión)

- Se elimina la duración fija de la falla (antes 6 lecturas): reemplaza a "se recupera automáticamente después de la cantidad configurada de lecturas" y a la duración del criterio R14.
- Los parámetros por defecto del mock pasan a ser: intervalo de 5 segundos, probabilidad de falla del 20 % por panel y lectura, probabilidad de recuperación del 40 % por lectura, variación aleatoria de ±10 % y ventana de 07:00 a 19:00 (UTC-3).

## Fuera de Alcance (revisión)

- Probabilidades distintas por parque, nodo o panel.
- Elegir desde el dashboard las probabilidades.
