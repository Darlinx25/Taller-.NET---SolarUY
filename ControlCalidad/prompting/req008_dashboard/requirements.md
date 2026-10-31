# Requerimientos: Dashboard de telemetría en vivo

## Descripción General

Como usuario del sistema SolarUY, quiero visualizar en un dashboard en tiempo real la generación de energía y el estado de salud de parques, nodos y paneles mediante telemetría recibida vía SignalR, para poder monitorear el funcionamiento del parque y reaccionar rápidamente ante la existencia de un fallo.

## Criterios de Aceptación

1. El dashboard debe ser accesible por ruta (sin depender obligatoriamente de un enlace en el menú).
2. El dashboard debe mostrar, en tiempo real, la generación de energía del parque, de cada nodo y de cada panel conectado.
3. El dashboard debe mostrar el estado de salud de cada panel para identificar rápidamente paneles con fallo. Se debe mostrar tanto un indicador visual como su estado textual.
4. Las actualizaciones deben recibirse en tiempo real mediante SignalR, sin necesidad de recargar la página.
5. El dashboard debe aprovechar la última lectura de telemetría ya existente en el sistema (`TelemetriaDto` / `ITelemetriaService` / almacén de telemetría).
6. El acceso al dashboard **NO** debe restringirse por rol. Debe ser accesible para usuarios autenticados según lo que determine la configuración, respetando las decisiones de arquitectura del proyecto.
7. Si no existe telemetría disponible, el sistema debe mostrar un estado claro ("Sin telemetría disponible").
8. El dashboard debe reflejar la topología actual (Parques / Nodos / Paneles) tal como se encuentra registrada en el sistema.

## Reglas de Negocio

- El dashboard es estrictamente de **solo lectura**.
- Se debe reutilizar el contrato de telemetría ya existente (`Application.Dtos.TelemetriaDto` y sus DTOs relacionados). No se debe modificar el formato de ingesta del simulador.
- La visualización en tiempo real se debe implementar con **SignalR**. El Hub debe ubicarse en la capa de Presentación (`Hubs/`), siguiendo las reglas de arquitectura del proyecto y las convenciones de nomenclatura.
- La lógica de agregación/selección de datos para la vista debe residir en la capa de **Application** (servicio). Los controladores de **Presentation** deben ser "vistas tontas" (solo invocan y renderizan).
- Tanto la vista MVC como la actualización en tiempo real deben convivir: la vista debe cargar el estado inicial desde el servicio de Application y actualizarse vía SignalR.
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
