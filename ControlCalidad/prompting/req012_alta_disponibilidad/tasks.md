# Tareas: req012 Alta Disponibilidad (Balanceador + Múltiples Instancias)

## 0. Planificación
- [x] 0.1 Crear carpeta `ControlCalidad/prompting/req012_alta_disponibilidad/` con `requirements.md`.

## 1. Infraestructura (Docker Compose + Nginx)
- [ ] 1.1 Agregar servicio `redis` a `docker-compose.yml` con imagen `redis:7-alpine`, healthcheck (`redis-cli ping`) y red `solaruy`.
- [ ] 1.2 Crear directorio `docker/nginx/` y archivo `docker/nginx/nginx.conf` con upstream a `app:8080`, soporte WebSocket/SignalR (`Upgrade`, `Connection`) y timeouts adecuados.
- [ ] 1.3 Agregar servicio `nginx` a `docker-compose.yml`: expone `127.0.0.1:${APP_PORT:-5080}:80`, monta el config, depende de `app`.
- [ ] 1.4 Modificar servicio `app`: quitar `ports` expuestos al host, agregar variables `ConnectionStrings__Redis` y configuraciones necesarias, agregar `deploy.replicas: ${APP_REPLICAS:-2}`, agregar dependencia de `redis` (healthy).
- [ ] 1.5 Modificar servicio `simulador`: cambiar `SimulacionApi__UrlBase` para apuntar a `http://nginx:80`.
- [ ] 1.6 Actualizar `.env.example` agregando `APP_REPLICAS=2`.

## 2. Aplicación (.NET) - Paquetes y configuración
- [ ] 2.1 Agregar paquetes NuGet al proyecto `SolarUY/SolarUY.csproj`: `Microsoft.AspNetCore.SignalR.StackExchangeRedis`, `Microsoft.Extensions.Caching.StackExchangeRedis`, `Microsoft.AspNetCore.DataProtection.StackExchangeRedis` (versiones compatibles con .NET 10).
- [ ] 2.2 Configurar `DataProtection` con Redis en `Program.cs` (compartir claves entre réplicas).
- [ ] 2.3 Configurar `AddStackExchangeRedisCache` en `Program.cs`.
- [ ] 2.4 Refactorizar `IAlmacenTelemetria`/`AlmacenTelemetriaCache` para usar `IDistributedCache` (mantener interfaz). Serializar/deserializar con `System.Text.Json`. Mantener semántica existente.
- [ ] 2.5 Configurar SignalR con backplane Redis: `AddSignalR().AddStackExchangeRedis(...)`.
- [ ] 2.6 Asegurar que configuración Redis se lee desde `ConnectionStrings:Redis` o variable de entorno.

## 3. Integración y validación
- [ ] 3.1 `docker compose build` sin errores.
- [ ] 3.2 Levantar con `docker compose up -d --build`. Verificar que hay 2 réplicas de `app` corriendo.
- [ ] 3.3 Probar acceso por `http://localhost:5080`. Verificar `/health` responde.
- [ ] 3.4 Detener una réplica de `app` (`docker compose stop` sobre una instancia) y verificar que las requests siguen respondiendo vía balanceador.
- [ ] 3.5 Verificar telemetría compartida: `GET /api/telemetria` funciona correctamente.
- [ ] 3.6 Verificar SignalR/Dashboard recibe actualizaciones.
- [ ] 3.7 Verificar login/sesión funciona tras balanceo (sin sticky sessions).
- [ ] 3.8 Verificar que el simulador sigue enviando telemetría al balanceador correctamente.
- [ ] 3.9 Ejecutar build/tests si aplica.

## 4. Cierre
- [ ] 4.1 Verificar que `docker compose up -d` levanta todo el stack correctamente con réplicas configurables.
