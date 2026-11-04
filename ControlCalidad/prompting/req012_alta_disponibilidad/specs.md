# Especificación Técnica: Alta Disponibilidad (Balanceador + Múltiples Instancias)

## Objetivo

Hacer que la aplicación sea tolerante a caídas mediante un balanceador de carga (nginx) con múltiples réplicas de `app`, con estado compartido vía Redis (cache distribuido, backplane de SignalR, DataProtection) y simulador apuntando al balanceador.

## Decisiones y supuestos

- Se agrega `nginx` como balanceador de carga en el stack local (Docker Compose).
- Se usan múltiples réplicas de `app` configurables con `APP_REPLICAS` (por defecto 2).
- Puerto 5080 expuesto hacia `nginx` (LB). Las réplicas de `app` no exponen puertos al host.
- Redis para estado compartido: última telemetría, claves DataProtection, backplane de SignalR.
- Las réplicas comparten la misma base de datos MySQL.
- Sin sticky sessions (se comparte DataProtection entre réplicas).
- El simulador envía la telemetría a `http://nginx:80` (balanceador).
- `/health` mantiene el comportamiento actual.
- No se rompen el flujo OIDC, el esquema `ApiKey` para `/api/simulacion/*`, ni el servidor MCP.

## 1) Infraestructura (`docker-compose.yml`, `docker/nginx/`)

### Redis
- Servicio `redis:7-alpine` con healthcheck (`redis-cli ping`), red `solaruy`.

### Nginx (balanceador)
- Servicio `nginx:alpine` con configuración en `docker/nginx/nginx.conf`.
- Upstream hacia `app:8080` (resolución DNS de Docker para las réplicas).
- Soporte para WebSocket/SignalR: headers `Upgrade` y `Connection`, `proxy_http_version 1.1`, timeouts extendidos.
- Expone `127.0.0.1:${APP_PORT:-5080}:80` hacia el host.
- Depende de `app`.

### App (réplicas)
- Sin `ports` expuestos al host.
- `deploy.replicas: ${APP_REPLICAS:-2}` configurable.
- Variables de entorno: `ConnectionStrings__SolarUYDb`, `ConnectionStrings__Redis: redis:6379`, `SimulacionApi__ApiKey`.
- Depende de `mysql` (healthy), `migrate` (completed successfully), `redis` (healthy).
- Healthcheck existente mantenido.

### Simulador
- `SimulacionApi__UrlBase: http://nginx:80` (apunta al balanceador).
- Depende de `nginx`.

### Configuración
- `.env.example` agrega `APP_REPLICAS=2`.

## 2) Aplicación (.NET)

### Paquetes NuGet (`SolarUY.csproj`)
Se agregan:
- `Microsoft.AspNetCore.SignalR.StackExchangeRedis` (9.0.9)
- `Microsoft.Extensions.Caching.StackExchangeRedis` (9.0.9)
- `Microsoft.AspNetCore.DataProtection.StackExchangeRedis` (9.0.9)

### Configuración (`Program.cs`)

#### DataProtection
Si `ConnectionStrings:Redis` está configurado:
- Se registra `ConnectionMultiplexer` como singleton.
- Se configura `AddDataProtection()` con `PersistKeysToStackExchangeRedis(multiplexer, "SolarUY-DataProtection-Keys")` y `SetApplicationName("SolarUY")`.
- Las claves se comparten entre todas las réplicas.

#### Cache distribuido
Si hay Redis configurado:
- Se registra `AddStackExchangeRedisCache` con `Configuration = redisConnectionString` e `InstanceName = "SolarUY:"`.
- Si no hay Redis, se mantiene `AddDistributedMemoryCache()` para compatibilidad.

#### Almacenamiento de telemetría
- `AlmacenTelemetriaCache` ya utiliza `IDistributedCache` para guardar/obtener la última lectura. Con Redis el estado queda compartido entre réplicas sin cambiar la interfaz `IAlmacenTelemetria`.

#### SignalR Backplane
Si hay Redis configurado:
- `AddSignalR().AddStackExchangeRedis(redisConnectionString, options => { options.Configuration.ChannelPrefix = RedisChannel.Literal("SolarUY"); })`
- Los eventos `ActualizarTelemetria` y `ActualizarAlertas` se propagan a todas las réplicas/clientes conectados.

## 3) Comportamiento esperado

- Todas las réplicas comparten la misma base de datos MySQL.
- La "última lectura" es compartida vía Redis (efímera, no se persiste en MySQL).
- Alertas e históricos siguen persistiendo en MySQL.
- Las sesiones de autenticación (cookies) son válidas en cualquier réplica gracias a DataProtection compartido.
- Al caer una réplica, el balanceador deja de enrutarle tráfico y las demás siguen atendiendo.
- El simulador sigue funcionando enviando telemetría al balanceador.

## Cambios realizados

- `docker-compose.yml` - Agregado `redis`, `nginx`, réplicas en `app`, simulador apunta a nginx
- `.env.example` - Agregado `APP_REPLICAS`
- `docker/nginx/nginx.conf` (nuevo) - Configuración del balanceador
- `SolarUY/SolarUY.csproj` - Paquetes Redis/SignalR/DataProtection
- `SolarUY/Program.cs` - Configuración de Redis, DataProtection, cache distribuido y SignalR backplane
- `ControlCalidad/prompting/req012_alta_disponibilidad/requirements.md`, `tasks.md`, `specs.md` (nuevos)
