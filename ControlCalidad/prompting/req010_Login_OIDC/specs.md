# Especificación Técnica: Login OIDC

## Objetivo

Completar la autenticación y autorización externa mediante OpenID Connect (OIDC) en el monolito ASP.NET Core MVC, incorporando un Identity Provider (IdP) local basado en **Keycloak** que se ejecute con `docker compose`. La aplicación delega el login al IdP, recibe el callback OIDC, crea una sesión local con cookies, mapea los roles del IdP y mantiene el control de acceso por `[Authorize(Roles = "Administrador")]` en los módulos de gestión.

Además del flujo base ya implementado, este requerimiento cubre el cierre de sesión en el IdP (RP-initiated logout), PKCE, mapeo de roles específico de Keycloak, validación estricta de emisor/audiencia y la preparación para múltiples instancias mediante claves de protección de datos compartidas.

## Decisiones y supuestos

- Arquitectura: monolito ASP.NET Core MVC organizado en capas (`Domain`, `Application`, `Infrastructure` y Presentación MVC), sin nuevos proyectos `.csproj`.
- Autenticación externa: OpenID Connect con `Microsoft.AspNetCore.Authentication.OpenIdConnect` y sesión local con `CookieAuthentication`.
- IdP local: contenedor **Keycloak 26** (`quay.io/keycloak/keycloak:26.0`) en modo `start-dev`, con un realm importado desde archivo. Es un IdP de desarrollo/demostración, no de producción.
- En producción se reemplaza la configuración del IdP local por la de un proveedor externo; no hay cambios de código.
- Alta disponibilidad: el req012 ya incorpora `nginx` como balanceador, réplicas de `app` y `redis` para estado compartido (incluida la protección de datos). Este requerimiento **no duplica** esa infraestructura: se apoya en ella y solo ajusta lo necesario para que OIDC funcione a través del balanceador.
- Autorización basada en roles: solo `Administrador` puede realizar operaciones de gestión. `Operador` puede iniciar sesión y consultar las pantallas generales, pero no gestionar.
- Se mantiene el esquema `ApiKeyAuthenticationHandler` para los endpoints del simulador (`/api/simulacion/*`).
- Se mantiene `DevAuthController` como fallback de desarrollo únicamente cuando OIDC no está configurado.
- Los controladores de gestión existentes mantienen `[Authorize(Roles = "Administrador")]`.
- No hay cambios en entidades, DbSets ni configuraciones de EF Core: **no se genera migración**.
- No se agregan tests de dominio porque no hay comportamiento nuevo en `SolarUY.Domain`.

## 1) Dominio (`SolarUY.Domain`)

### Entidades
- Sin cambios en entidades existentes.

### Validaciones
- Sin cambios.

### Excepciones
- Sin nuevas excepciones de dominio.

### Repositorios (contratos)
- Sin cambios.

## 2) Aplicación (`SolarUY.Application`)

### DTOs
- Sin nuevos DTOs de negocio.

### Servicios y casos de uso
- Sin cambios en servicios de aplicación. La autenticación es una preocupación de infraestructura/presentación y no se modela como caso de uso de negocio.

### Configuración de OIDC
- Crear `SolarUY/Application/Seguridad/OpcionesOidc.cs` como POCO de configuración (no es un servicio de negocio):
  - Constante `Seccion = "Authentication:Oidc"`.
  - Propiedades: `Authority`, `MetadataAddress`, `BackchannelBaseUrl`, `ClientId`, `ClientSecret`, `CallbackPath` (por defecto `/signin-oidc`), `SignedOutCallbackPath` (por defecto `/signout-callback-oidc`), `RequireHttpsMetadata` y `PostLogoutRedirectUri`.
  - Propiedad calculada `EsConfigurado` que devuelve `true` solo si `Authority` y `ClientId` no están vacíos.
- No se agregan dependencias de ASP.NET Core en `Application`: el POCO solo contiene datos de configuración.

## 3) Infraestructura (`SolarUY.Infrastructure`)

### IdP local Keycloak (`docker compose` y archivo de realm)

#### Servicio `keycloak`
- Agregar el servicio `keycloak` a `docker-compose.yml`:
  - Imagen: `quay.io/keycloak/keycloak:26.0`.
  - Comando: `start-dev --import-realm`.
  - Variables de entorno:
    - `KC_BOOTSTRAP_ADMIN_USERNAME=${KEYCLOAK_ADMIN:-admin}`.
    - `KC_BOOTSTRAP_ADMIN_PASSWORD=${KEYCLOAK_ADMIN_PASSWORD:?definí KEYCLOAK_ADMIN_PASSWORD en .env}`.
    - `KC_HOSTNAME=http://localhost:${KEYCLOAK_PORT:-8081}`.
    - `KC_HTTP_ENABLED=true`.
    - `KC_PROXY_HEADERS=xforwarded`.
  - Puerto publicado solo en loopback: `127.0.0.1:${KEYCLOAK_PORT:-8081}:8080`.
  - Volumen de solo lectura para el archivo del realm: `./docker/keycloak/realm-solaruy.json:/opt/keycloak/data/import/realm-solaruy.json:ro`.
  - Red `solaruy`.
  - Sin MySQL: `start-dev` usa el almacenamiento de desarrollo de Keycloak (H2). Es intencional para este entorno local.
- El servicio `app` agrega `depends_on: keycloak: condition: service_started`. La aplicación no consulta la metadata al arrancar; la resuelve recién en el primer login.

#### Realm importado: `docker/keycloak/realm-solaruy.json`
- Realm `solaruy`, habilitado.
- Cliente `solaruy-app`:
  - `protocol = openid-connect`, `publicClient = false`, `clientAuthenticatorType = client-secret`.
  - Secreto de desarrollo: `solaruy-app-secret` (el mismo valor por defecto de `OIDC_CLIENT_SECRET` en `.env.example`). En producción se define por variable de entorno/secret manager.
  - `standardFlowEnabled = true`, `implicitFlowEnabled = false`, `directAccessGrantsEnabled = false`, `serviceAccountsEnabled = false`.
  - `redirectUris = ["http://localhost:5080/signin-oidc"]`.
  - `post.logout.redirect.uris = ["http://localhost:5080/*"]`.
  - `webOrigins = ["http://localhost:5080"]`.
  - Atributo `pkce.code.challenge.method = S256`.
  - Mapper `realm-roles` de tipo `oidc-usermodel-realm-role-mapper` con `claim.name = roles`, `multivalued = true`, `jsonType.label = String`, e inclusión en ID token, access token y userinfo. Esto garantiza que el rol llegue en el token que valida la aplicación.
- Roles de realm: `Administrador` y `Operador`.
- Usuarios de desarrollo (contraseñas solo para desarrollo, documentadas en `DESARROLLO.md`):
  - `admin@solaruy.uy` con rol `Administrador`.
  - `operador@solaruy.uy` con rol `Operador`.
  - Ambos habilitados, con email verificado y credenciales locales.
- El administrador de Keycloak (`KEYCLOAK_ADMIN` / `KEYCLOAK_ADMIN_PASSWORD`) queda disponible en `http://localhost:8081/admin` para inspeccionar el realm.

#### Variables de entorno
- Agregar a `.env.example`:
  - `KEYCLOAK_PORT=8081`.
  - `KEYCLOAK_ADMIN=admin`.
  - `KEYCLOAK_ADMIN_PASSWORD=SolarUY.Local2026`.
  - `OIDC_CLIENT_SECRET=solaruy-app-secret`.
- Agregar al servicio `app` de `docker-compose.yml`:
  - `Authentication__Oidc__Authority=http://localhost:${KEYCLOAK_PORT:-8081}/realms/solaruy` (URL pública; es el issuer que verá el navegador).
  - `Authentication__Oidc__MetadataAddress=http://keycloak:8080/realms/solaruy/.well-known/openid-configuration` (URL interna para que la app resuelva la metadata).
  - `Authentication__Oidc__BackchannelBaseUrl=http://keycloak:8080` (URL interna para token, JWKS y userinfo).
  - `Authentication__Oidc__ClientId=solaruy-app`.
  - `Authentication__Oidc__ClientSecret=${OIDC_CLIENT_SECRET:-solaruy-app-secret}`.
  - `Authentication__Oidc__CallbackPath=/signin-oidc`.
  - `Authentication__Oidc__SignedOutCallbackPath=/signout-callback-oidc`.
  - `Authentication__Oidc__PostLogoutRedirectUri=http://localhost:${APP_PORT:-5080}/`.
  - `Authentication__Oidc__RequireHttpsMetadata=false` (solo desarrollo).

#### Resolución de URLs pública/interna
- El navegador solo resuelve `localhost:8081`; el contenedor `app` solo resuelve `keycloak:8080`.
- Por eso la metadata se obtiene de `MetadataAddress` (interna), mientras que los endpoints de autorización/public logout que genera Keycloak quedan con la URL pública (`localhost:8081`) configurada en `KC_HOSTNAME`.
- Para las llamadas servidor-a-servidor (token, JWKS, userinfo) se usa `KeycloakBackchannelHandler`, que reescribe el host/puerto público por `BackchannelBaseUrl` interno conservando ruta y query. Si `BackchannelBaseUrl` está vacío (IdP productivo externo), no se reescribe nada.

### Componentes de seguridad

- Crear `SolarUY/Infrastructure/Seguridad/KeycloakBackchannelHandler.cs`:
  - `DelegatingHandler` que recibe la URL interna y reescribe únicamente las solicitudes dirigidas al host/puerto público de Keycloak.
  - No conoce controladores ni vistas; se registra como `OpenIdConnectOptions.Backchannel` usando `new HttpClient(handler)` con un `HttpClientHandler` como `InnerHandler`.
- Crear `SolarUY/Infrastructure/Seguridad/MapeadorRolesKeycloak.cs`:
  - Método estático `AgregarRoles(ClaimsIdentity identidad, IEnumerable<Claim> claims, string clientId)`.
  - Agrega a `ClaimTypes.Role` los valores encontrados en:
    - Claims directos: `roles`, `role`, `groups` y `http://schemas.microsoft.com/ws/2008/06/identity/claims/role`.
    - JSON `realm_access` → propiedad `roles`.
    - JSON `resource_access` → propiedad `<clientId>` → `roles`.
  - Evita duplicados antes de agregar cada rol.
  - Usa `System.Text.Json` y no depende de ASP.NET Core ni de EF Core.

### Integración con la alta disponibilidad de req012

- El req012 ya configura `AddDataProtection()` con `PersistKeysToStackExchangeRedis(...)` y `SetApplicationName("SolarUY")` cuando `ConnectionStrings:Redis` está presente, además del backplane de SignalR y la caché distribuida.
- req010 **no agrega volúmenes, cambios en `Dockerfile.app` ni configuración propia de DataProtection**: reutiliza lo existente. Esto garantiza que la cookie de sesión y el estado/correlación de OIDC sean válidos en cualquiera de las réplicas de `app`.
- En `docker-compose.yml`, `app` ya no expone puertos al host: el acceso externo es `nginx` en `APP_PORT` (5080 por defecto), que balancea hacia las réplicas. Keycloak se publica aparte en `127.0.0.1:8081`.
- Ajuste necesario en `docker/nginx/nginx.conf`: preservar el puerto del host con `proxy_set_header Host $http_host;` y enviar `X-Forwarded-Host $http_host` y `X-Forwarded-Proto $scheme`. Sin esto, la app vería `Host: localhost` (sin `:5080`) y generaría un `redirect_uri` (`http://localhost/signin-oidc`) que no coincide con el registrado en Keycloak.
- nginx debe resolver `app` dinámicamente (`resolver 127.0.0.11 valid=10s ipv6=off;` y `server app:8080 resolve;` dentro de un `upstream` con `zone`) para no conservar la IP de una réplica que se reinició. Además usa `proxy_buffer_size 32k`, `proxy_buffers 8 32k`, `proxy_busy_buffers_size 64k` y `large_client_header_buffers 4 32k`, porque las cookies y los encabezados del callback OIDC superan los buffers por defecto de nginx y provocan `502 Bad Gateway`.
- Alternativa descartada: fijar el `redirect_uri` público por configuración en la aplicación. Se prefiere corregir el encabezado en nginx para no duplicar configuración.

### Persistencia
- Sin cambios de esquema, DbSets, configuraciones Fluent API ni repositorios.

### Migración
- **No requiere migración.** No hay cambios en entidades, DbSets o configuraciones de EF Core.

### Registro de dependencias (`Program.cs`)
- Enlazar `OpcionesOidc` desde `Authentication:Oidc`. Cuando OIDC esté configurado, exigir `Authority` y `ClientId`; si `BackchannelBaseUrl` está definido (IdP local Keycloak), exigir además `ClientSecret` y `MetadataAddress`. Para un IdP externo sin backchannel interno, `MetadataAddress` y `BackchannelBaseUrl` pueden quedar vacíos.
- Configurar autenticación:
  - `DefaultScheme` y `DefaultSignInScheme` = `CookieAuthenticationDefaults.AuthenticationScheme`.
  - `DefaultAuthenticateScheme` y `DefaultForbidScheme` = cookies.
  - `DefaultChallengeScheme` = `OpenIdConnectDefaults.AuthenticationScheme` cuando OIDC está configurado; si no, cookies (fallback).
  - `AddCookie` con `LoginPath = "/login"`, `LogoutPath = "/logout"`, `AccessDeniedPath = "/dev/denegado"`, `ExpireTimeSpan = 8 horas` y `SlidingExpiration = true`. En producción, `Cookie.SecurePolicy = CookieSecurePolicy.Always`.
  - `AddOpenIdConnect("oidc", ...)` con `Authority`, `ClientId`, `ClientSecret`, `MetadataAddress`, `CallbackPath`, `SignedOutCallbackPath`, `SignedOutRedirectUri = PostLogoutRedirectUri`, `ResponseType = "code"`, `UsePkce = true`, `SaveTokens = true`, `GetClaimsFromUserInfoEndpoint = true`.
  - `RequireHttpsMetadata` según configuración; en desarrollo `false`, en producción `true`.
  - Scopes: `openid`, `profile`, `email` y `roles`.
  - `TokenValidationParameters` con `ValidateAudience = true`, `ValidAudience = ClientId`, `ValidateIssuer = true`, `NameClaimType = "preferred_username"` y `RoleClaimType = ClaimTypes.Role`.
  - `Backchannel = new HttpClient(new KeycloakBackchannelHandler(...) { InnerHandler = new HttpClientHandler() })` cuando `BackchannelBaseUrl` esté configurado.
  - Evento `OnTokenValidated`: invocar `MapeadorRolesKeycloak.AgregarRoles(...)` para volcar los roles a `ClaimTypes.Role`.
  - El handler de .NET usa Pushed Authorization Requests (PAR) por defecto cuando el IdP lo soporta; Keycloak 26 lo soporta. Por eso la URL visible del navegador lleva `request_uri` y el `code_challenge`/`code_challenge_method=S256` viaja en el POST al endpoint PAR, no en la query de autorización.
- Mantener `AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(AutenticacionComponentes.EsquemaApiKey, null)`.
- Mantener `AddAuthorization()`.
- No modificar la configuración de DataProtection de req012: las claves compartidas por Redis son las que hacen válida la sesión entre réplicas.
- Configurar `UseForwardedHeaders()` al inicio del pipeline (antes de `UseHttpsRedirection` y `UseAuthentication`), con `XForwardedFor | XForwardedProto | XForwardedHost`, para respetar el esquema y host terminados en nginx o en un proxy externo. En desarrollo, el ajuste de `Host` en nginx ya preserva el puerto.
- Mantener el orden de middleware: `UseRouting()`, `UseAuthentication()`, `UseAuthorization()`.

## 4) Presentación (`SolarUY.Controllers` + `Views`)

### Controlador y acciones
- Crear `SolarUY/Controllers/AuthController.cs` con `[AllowAnonymous]`:
  - `GET /login` (`Login`): valida que `returnUrl` sea local. Si OIDC está configurado, ejecuta `Challenge(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = destino })` para redirigir directamente al IdP. Si no está configurado, redirige a `/dev/login?returnUrl=...` (fallback).
  - `POST /logout` (`Logout`) con `[ValidateAntiForgeryToken]`: ejecuta `SignOut` con los esquemas Cookie y OpenID Connect cuando OIDC está configurado, usando `AuthenticationProperties { RedirectUri = OpcionesOidc.PostLogoutRedirectUri }` (por defecto `/`) para que el handler redirija al `end_session_endpoint` del IdP (RP-initiated logout) y Keycloak vuelva al destino indicado. Si OIDC no está configurado, cierra solo la cookie y redirige a `/`.
  - El controlador no contiene reglas de negocio, no accede a `Infrastructure` y solo orquesta los desafíos/cierres de sesión del framework.
- Mantener `SolarUY/Controllers/DevAuthController.cs` para el fallback de desarrollo (`/dev/login`, `/dev/logout`, `/dev/denegado`).

### Autorización
- Los controladores de gestión mantienen `[Authorize(Roles = "Administrador")]`: `ParquesSolaresController`, `NodosController`, `PanelesController`, `TareasMantenimientoController` y `TelemetriaController`.
- Sin sesión y OIDC configurado: redirección directa al IdP.
- Sin sesión y OIDC no configurado: fallback a `/dev/login`.
- Usuario autenticado sin rol `Administrador`: redirección a `AccessDeniedPath` (`/dev/denegado`).
- El esquema `ApiKey` se mantiene independiente para `/api/simulacion/*`.

### Vistas
- Actualizar `SolarUY/Views/Shared/_Layout.cshtml`:
  - Usuario no autenticado: enlace "Iniciar sesión" a `Auth/Login` (el controlador decide IdP o fallback dev).
  - Usuario autenticado: reemplazar el enlace "Salir" a `DevAuth/Logout` por un formulario `POST` a `Auth/Logout` con token antiforgery.
- Mantener `SolarUY/Views/DevAuth/AccesoDenegado.cshtml` como vista de acceso denegado.

## 5) Pruebas

- **Tests xUnit del Dominio (obligatorios):** no se agregan tests nuevos porque no hay comportamiento nuevo en `Domain`. Se debe verificar que los tests existentes sigan pasando.
- **No se agregan tests unitarios en Application, Infrastructure ni Presentation.**
- La verificación de Keycloak, PKCE, logout y sesión compartida se realiza con pruebas manuales guiadas (ver sección 6).

## 6) Calidad y verificación

- [x] El servicio `keycloak` levanta con `docker compose up -d` y responde en `http://localhost:8081/realms/solaruy/.well-known/openid-configuration`.
- [x] El realm importa correctamente el cliente, los roles y los dos usuarios de prueba.
- [x] `AddOpenIdConnect` usa PKCE, validación de audiencia/emisor, scopes y mapeo de roles de Keycloak.
- [x] El logout cierra la sesión local y la sesión de Keycloak (RP-initiated logout).
- [x] El `redirect_uri` generado detrás de nginx conserva el puerto (`http://localhost:5080/signin-oidc`) y Keycloak lo acepta.
- [x] La sesión es válida entre réplicas y tras reiniciar `app`, reutilizando la DataProtection sobre Redis de req012 (sin volumen propio).
- [x] Se mantiene el esquema `ApiKey` para `/api/simulacion/*`.
- [x] Se mantiene `[Authorize(Roles = "Administrador")]` en los controladores de gestión.
- [x] Build: `docker compose run --rm tools dotnet build SolarUY.sln`.
- [x] Tests: `docker compose run --rm tools dotnet test SolarUY.sln`.
- [x] ArchUnitNET en verde.
- [ ] SonarQube ejecutado y hallazgos revisados.
- [x] Prueba manual: `admin@solaruy.uy` accede a gestión; `operador@solaruy.uy` recibe acceso denegado; logout vuelve a pedir login; `/api/simulacion/*` sigue funcionando con ApiKey.

## 7) Fuera de alcance

- Desplegar un IdP productivo en la nube o una infraestructura de alta disponibilidad real con balanceador.
- Registro/autogestión de usuarios y recuperación de contraseña desde SolarUY.
- Migración de usuarios o roles desde otros sistemas.
- Cambios en lógica de negocio distinta a autenticación/autorización.
- Modificación de `CodeQuality.Tests` sin autorización explícita.
