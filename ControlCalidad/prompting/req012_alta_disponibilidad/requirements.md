# Requerimientos: Alta Disponibilidad (Balanceador + Múltiples Instancias)

## Descripcion General

Como operador del sistema SolarUY, quiero que la aplicación esté detrás de un balanceador de carga con múltiples réplicas de la app, para que ante la caída o reinicio de una instancia el servicio siga disponible sin interrupción.

## Criterios de Aceptacion

1. Existe un servicio `nginx` actuando como balanceador de carga frente a las instancias de `app`.
2. Se levantan múltiples réplicas de `SolarUY` (configurables) detrás del balanceador. Por defecto `APP_REPLICAS=2`.
3. El puerto 5080 se mantiene expuesto hacia el balanceador (`nginx`).
4. El estado de "última telemetría" es compartido entre instancias (Redis) para que cualquier réplica pueda responder `GET /api/telemetria` correctamente.
5. SignalR usa un backplane Redis para que `ActualizarTelemetria` y `ActualizarAlertas` lleguen a todos los clientes conectados sin importar a qué instancia estén conectados.
6. Las claves de `DataProtection` se comparten entre réplicas para que las cookies de autenticación sean válidas en todas las instancias (sin forzar sticky sessions).
7. Las 2+ réplicas comparten la misma base de datos MySQL, manteniendo consistencia de datos.
8. El simulador envía la telemetría al balanceador (`nginx`), no directamente a una instancia fija de `app`.
9. `/health` sigue funcionando. El balanceador enruta solo a instancias disponibles.
10. No se rompe el flujo OIDC, el esquema `ApiKey` para `/api/simulacion/*`, ni el servidor MCP.
11. Funciona con `docker compose up -d --build` en el entorno local del taller.

## Reglas de Negocio

- La "última lectura" sigue siendo efímera (no se persiste en MySQL). Alertas e históricos siguen persistiendo en MySQL (compartido).
- Redis es utilizado únicamente para estado compartido: cache distribuido (última telemetría), backplane de SignalR y claves de DataProtection.
- Todas las réplicas acceden a la misma instancia de MySQL.
- El acceso externo sigue por `http://localhost:5080`.

## Fuera de Alcance

- Alta disponibilidad de MySQL/Redis (replicación/clustering).
- HTTPS externo, certificados.
- Modificación de lógica de negocio.
- Cambios en autenticación/autorización distintos a compartir DataProtection.
- Cambios en la API o contratos existentes.
