# Requerimientos: Dashboard de telemetría en vivo

## Descripción General

Como usuario del sistema SolarUY, quiero visualizar en un dashboard en tiempo real la generación de energía y el estado de salud de parques, nodos y paneles mediante telemetría recibida vía SignalR, para poder monitorear el funcionamiento del parque y reaccionar rápidamente ante la existencia de un fallo.

## Criterios de Aceptación

1. El dashboard debe ser accesible por ruta (sin depender obligatoriamente de un enlace en el menú).
2. El dashboard debe mostrar, en tiempo real, la generación de energía del parque, de cada nodo y de cada panel conectado.
3. El dashboard debe mostrar el estado de salud de cada panel para identificar rápidamente paneles con fallo. Se debe mostrar tanto un indicador visual como su estado textual.
4. Las actualizaciones deben recibirse mediante SignalR y reflejarse en la vista sin recargar la página. La vista puede solicitar el estado consolidado al recibir el evento de actualización.
5. El dashboard debe aprovechar la última lectura de telemetría ya existente en el sistema (`TelemetriaDto` / `ITelemetriaService` / almacén de telemetría).
6. El acceso al dashboard **NO** debe restringirse por rol. Debe ser accesible para usuarios autenticados según lo que determine la configuración, respetando las decisiones de arquitectura del proyecto.
7. Si no existe telemetría disponible, el sistema debe mostrar un aviso claro ("Sin telemetría disponible") y mantener visible la topología registrada, indicando "Sin telemetría" en los paneles que no tengan lectura.
8. El dashboard debe reflejar toda la topología actual (Parques / Nodos / Paneles) tal como está registrada en el sistema, aunque un parque, nodo o panel todavía no tenga telemetría.

## Reglas de Negocio

- El dashboard es estrictamente de **solo lectura**.
- Se debe reutilizar el contrato de telemetría ya existente (`Application.Dtos.TelemetriaDto` y sus DTOs relacionados). No se debe modificar el formato de ingesta del simulador.
- La visualización en tiempo real se debe implementar con **SignalR**. El Hub debe ubicarse en la capa de Presentación (`Hubs/`), siguiendo las reglas de arquitectura del proyecto y las convenciones de nomenclatura.
- La lógica de agregación/selección de datos para la vista debe residir en la capa de **Application** (servicio). Los controladores de **Presentation** deben ser "vistas tontas" (solo invocan y renderizan).
- Tanto la vista MVC como la actualización en tiempo real deben convivir: la vista debe cargar el estado inicial desde el servicio de Application y actualizarse vía SignalR.
- La topología registrada es la fuente de verdad para parques, nodos y paneles. La telemetría solo aporta generación y estado de salud.
- `Marca`, `Modelo` y `PotenciaWp` se toman de la topología del panel.
- La generación y el estado de salud se toman de la última lectura de telemetría cuando existen.
- Si un panel no tiene telemetría, se muestra generación `0` y estado `Sin telemetría`.
- No se muestran parques, nodos o paneles que existan solo en la telemetría y no en la topología registrada.
- La actualización en vivo no debe hacer una recarga completa de la página.
- El Dominio no debe depender de SignalR ni de ASP.NET Core.
- Debe respetarse Clean Architecture por capas: Domain / Application / Infrastructure / Presentation.
- Debe integrarse con la inyección de dependencias en `Program.cs`.
- Debe respetarse lo indicado en `Iteracion 1 Grupo 2 .NET.pdf` en cuanto a decisiones de arquitectura tomadas para esta iteración.

## Fuera de Alcance

- Modificar el endpoint/api del simulador (`/api/simulacion/*`).
- Cambiar el modelo de ingesta o validación de telemetría.
- Implementar notificaciones por correo/SMS.
- Implementar histórico/gráficas históricas (ya existe funcionalidad de históricos).
- Crear nuevos sensores/inversores físicos.
- Restringir el acceso por roles.

## Adenda: gráficas del dashboard (agregado posterior)

### Contexto

A partir de una revisión con el profesor se solicita ampliar el dashboard con
gráficas que permitan interpretar más rápido el estado del sistema. Esta adenda
se suma a los criterios y reglas anteriores; no los reemplaza.

### Criterios de aceptación adicionales

9. El dashboard debe mostrar una gráfica de **generación actual por parque**.
10. El dashboard debe mostrar una gráfica de **distribución del estado de los
    paneles**, diferenciando al menos `Operativo`, `EnFalla` y `Sin telemetría`.
11. El dashboard debe mostrar una gráfica de **generación por nodo**, identificando
    el parque y el nodo de cada valor.
12. El dashboard debe mostrar una **tendencia compacta de energía diaria de los
    últimos 7 días**, construida a partir de los resúmenes históricos existentes.
13. Cada gráfica debe mostrar un estado vacío claro cuando no haya datos
    suficientes, sin impedir que se sigan mostrando la topología y las tablas
    existentes.

### Reglas de negocio adicionales

- Las tres primeras gráficas son de **estado en vivo** y deben actualizarse con
  la misma lectura de telemetría que alimenta el resto del dashboard.
- La tendencia de 7 días es una vista **compacta y de solo lectura**; no
  reemplaza la pantalla de Histórico ni incorpora filtros avanzados.
- La tendencia debe reutilizar los resúmenes históricos ya existentes, sin crear
  una nueva fuente de persistencia.
- La lógica de agregación de las gráficas debe residir en la capa de
  **Application**; las vistas no deben calcular totales, conteos ni series.
- Las gráficas no deben agregar restricciones de acceso por rol.
- No se debe consultar el histórico en cada evento de SignalR: la tendencia se
  carga con la vista y, si se refresca, debe hacerlo con una frecuencia baja
  (por ejemplo, cada 5 minutos), no con cada lectura de telemetría.

### Aclaración sobre el fuera de alcance original

La exclusión original de "histórico/gráficas históricas" se mantiene para la
pantalla completa de históricos, sus filtros avanzados y sus reportes. Esta
adenda incorpora únicamente una tendencia compacta de 7 días en el dashboard,
reutilizando los resúmenes diarios existentes.

### Fuera de alcance de esta adenda

- Crear tablas, entidades, migraciones o una nueva fuente de persistencia para
  las gráficas.
- Implementar filtros avanzados de histórico dentro del dashboard.
- Agregar series intradía o almacenamiento de lecturas históricas cada 5
  segundos.
- Modificar el contrato de ingesta de telemetría o el simulador.
- Restringir el dashboard por rol.
