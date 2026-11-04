# Tareas: req010 Login OIDC

> Cada tarea debe ser verificable, indicar el archivo o la ruta cuando aplique y comenzar con `[ ]`. Marcar `[x]` solo cuando esté terminada y verificada.

## 0. Preparación

- [x] 0.1 Confirmar que `requirements.md`, `specs.md` y este plan están aprobados (gates R, S y T).
- [x] 0.2 Revisar `SolarUY/SolarUY.csproj`, `SolarUY/Program.cs`, controladores de gestión existentes y `DevAuthController` para respetar patrones actuales.
- [x] 0.3 Revisar `docker-compose.yml`, `docker/Dockerfile.app`, `.env.example`, `SolarUY/appsettings*.json` y `SolarUY/Views/Shared/_Layout.cshtml` antes de modificar.

## 1. Dominio (`SolarUY.Domain`)

- [x] 1.1 Verificar que no se requieren cambios en entidades, excepciones o contratos de repositorio. Sin cambios.

## 2. Tests del Dominio (`SolarUY.Tests/Domain`)

- [x] 2.1 Verificar que no hay lógica de dominio nueva. No agregar tests de dominio nuevos.
- [x] 2.2 Confirmar que los tests existentes de `SolarUY.Tests` siguen pasando después de los cambios de autenticación.

## 3. Aplicación (`SolarUY.Application`)

- [x] 3.1 Verificar que no se requieren cambios en DTOs o servicios de aplicación. Mantener separación de capas.
- [x] 3.2 Crear `SolarUY/Application/Seguridad/OpcionesOidc.cs` con la constante `Seccion = "Authentication:Oidc"`, las propiedades `Authority`, `MetadataAddress`, `BackchannelBaseUrl`, `ClientId`, `ClientSecret`, `CallbackPath`, `SignedOutCallbackPath`, `RequireHttpsMetadata` y `PostLogoutRedirectUri`, y la propiedad calculada `EsConfigurado`. Sin dependencias de ASP.NET Core.

## 4. Infraestructura (`SolarUY.Infrastructure`)

### Base ya implementada

- [x] 4.1 Verificar esquema de base de datos: no hay cambios en entidades/configuraciones; **no se genera migración**.
- [x] 4.2 Agregar paquete NuGet `Microsoft.AspNetCore.Authentication.OpenIdConnect` al proyecto `SolarUY/SolarUY.csproj`.
- [x] 4.3 Configurar autenticación externa en `SolarUY/Program.cs`: mantener `AddAuthentication` con esquema Cookies como predeterminado, agregar `AddOpenIdConnect("oidc", ...)` con `Authority`, `ClientId`, `ClientSecret`, `CallbackPath`, `ResponseType`, `SaveTokens` y `GetClaimsFromUserInfoEndpoint` leídos desde configuración. Mantener `AddScheme` para `ApiKeyAuthenticationHandler` (`AutenticacionComponentes.EsquemaApiKey`).
- [x] 4.4 Implementar el mapeo base de claims/roles desde el IdP a `ClaimTypes.Role`.
- [x] 4.5 Mantener orden de middleware: `UseRouting()`, `UseAuthentication()`, `UseAuthorization()`. Mantener rutas `LoginPath`, `LogoutPath`, `AccessDeniedPath`.

### IdP local Keycloak

- [x] 4.6 Agregar el servicio `keycloak` a `docker-compose.yml` con imagen `quay.io/keycloak/keycloak:26.0`, comando `start-dev --import-realm`, variables `KC_BOOTSTRAP_ADMIN_USERNAME`, `KC_BOOTSTRAP_ADMIN_PASSWORD`, `KC_HOSTNAME=http://localhost:${KEYCLOAK_PORT:-8081}`, `KC_HTTP_ENABLED=true`, `KC_PROXY_HEADERS=xforwarded`, puerto `127.0.0.1:${KEYCLOAK_PORT:-8081}:8080`, volumen del archivo `./docker/keycloak/realm-solaruy.json:/opt/keycloak/data/import/realm-solaruy.json:ro` y red `solaruy`.
- [x] 4.7 Crear `docker/keycloak/realm-solaruy.json` con el realm `solaruy`, el cliente confidencial `solaruy-app` (secret `solaruy-app-secret`, redirect URI `http://localhost:5080/signin-oidc`, post logout URI `http://localhost:5080/*`, PKCE `S256`), el mapper de roles de realm a claim `roles` incluido en ID token/userinfo, los roles `Administrador` y `Operador`, y los usuarios `admin@solaruy.uy` (Administrador) y `operador@solaruy.uy` (Operador).
- [x] 4.8 Agregar a `.env.example` las variables `KEYCLOAK_PORT=8081`, `KEYCLOAK_ADMIN=admin`, `KEYCLOAK_ADMIN_PASSWORD`, `OIDC_CLIENT_SECRET` y `APP_PORT`, con los valores de desarrollo documentados.
- [x] 4.9 Agregar al servicio `app` de `docker-compose.yml` las variables `Authentication__Oidc__Authority`, `Authentication__Oidc__MetadataAddress`, `Authentication__Oidc__BackchannelBaseUrl`, `Authentication__Oidc__ClientId`, `Authentication__Oidc__ClientSecret`, `Authentication__Oidc__CallbackPath`, `Authentication__Oidc__SignedOutCallbackPath`, `Authentication__Oidc__PostLogoutRedirectUri` y `Authentication__Oidc__RequireHttpsMetadata=false`, más `depends_on` de `keycloak` con `condition: service_started`.

### Código de infraestructura

- [x] 4.10 Crear `SolarUY/Infrastructure/Seguridad/KeycloakBackchannelHandler.cs`: `DelegatingHandler` que reescribe las solicitudes dirigidas a la URL pública de Keycloak (`localhost:8081`) hacia la URL interna (`http://keycloak:8080`), conservando ruta y query. No se registra si `BackchannelBaseUrl` está vacío.
- [x] 4.11 Crear `SolarUY/Infrastructure/Seguridad/MapeadorRolesKeycloak.cs`: método estático `AgregarRoles(ClaimsIdentity identidad, IEnumerable<Claim> claims, string? clientId)` que lea claims directos (`roles`, `role`, `groups`, claim de rol de Microsoft) y los JSON `realm_access.roles` y `resource_access.<clientId>.roles`, agregando los valores a `ClaimTypes.Role` sin duplicados.

### `Program.cs`

- [x] 4.12 Enlazar `OpcionesOidc` desde `Authentication:Oidc` y validar `Authority` y `ClientId` cuando OIDC esté configurado; si `BackchannelBaseUrl` está definido (Keycloak local), exigir además `ClientSecret` y `MetadataAddress`.
- [x] 4.13 Configurar `DefaultScheme`, `DefaultSignInScheme`, `DefaultAuthenticateScheme` y `DefaultForbidScheme` como Cookies; `DefaultChallengeScheme` como OIDC cuando esté configurado y como Cookies cuando no.
- [x] 4.14 Configurar `AddCookie` con `LoginPath = "/login"`, `LogoutPath = "/logout"`, `AccessDeniedPath = "/dev/denegado"`, expiración de 8 horas, expiración deslizante y `SecurePolicy = CookieSecurePolicy.Always` fuera de desarrollo.
- [x] 4.15 Configurar `AddOpenIdConnect` con `MetadataAddress`, `CallbackPath`, `SignedOutCallbackPath`, `SignedOutRedirectUri = PostLogoutRedirectUri`, `ResponseType = "code"`, `UsePkce = true`, `SaveTokens = true`, `GetClaimsFromUserInfoEndpoint = true`, scopes `openid profile email roles`, `RequireHttpsMetadata` según configuración y `Backchannel = new HttpClient(KeycloakBackchannelHandler)` con `InnerHandler = HttpClientHandler` cuando corresponda.
- [x] 4.16 Configurar `TokenValidationParameters` con `ValidateAudience = true`, `ValidAudience = ClientId`, `ValidateIssuer = true`, `NameClaimType = "preferred_username"` y `RoleClaimType = ClaimTypes.Role`.
- [x] 4.17 En `OnTokenValidated`, invocar `MapeadorRolesKeycloak.AgregarRoles(...)` para volcar los roles del IdP a `ClaimTypes.Role`.
- [x] 4.18 Configurar `UseForwardedHeaders()` al inicio del pipeline (antes de `UseHttpsRedirection` y `UseAuthentication`) con `XForwardedFor | XForwardedProto | XForwardedHost`.

### Integración con alta disponibilidad (req012)

- [x] 4.19 Verificar que la DataProtection de req012 (Redis + `SetApplicationName("SolarUY")`) siga activa cuando `ConnectionStrings:Redis` está configurado; no agregar volumen `dataprotection`, ni cambios en `docker/Dockerfile.app`, ni `DataProtection:ClavesPath`.
- [x] 4.20 Ajustar `docker/nginx/nginx.conf` para preservar el puerto del host: `proxy_set_header Host $http_host;`, `X-Forwarded-Host $http_host` y `X-Forwarded-Proto $scheme`, de modo que el `redirect_uri` sea `http://localhost:5080/signin-oidc`.
- [x] 4.21 Verificar que Keycloak no se exponga a través de nginx y que su puerto `127.0.0.1:${KEYCLOAK_PORT:-8081}` quede separado del balanceador.

### Compatibilidad

- [x] 4.22 Verificar que el esquema `ApiKey` siga funcionando para `/api/simulacion/*` sin interferir con OIDC.

## 5. Presentación (`SolarUY.Controllers` + `Views`)

- [x] 5.1 Verificar que los controladores de gestión mantienen `[Authorize(Roles = "Administrador")]`: `SolarUY/Controllers/ParquesSolaresController.cs`, `SolarUY/Controllers/NodosController.cs`, `SolarUY/Controllers/PanelesController.cs`, `SolarUY/Controllers/TareasMantenimientoController.cs`, `SolarUY/Controllers/TelemetriaController.cs`.
- [x] 5.2 Asegurar redirección al IdP cuando corresponda (challenge OIDC) y mantener fallback `/dev/login` para desarrollo si no hay IdP configurado.
- [x] 5.3 Mantener `SolarUY/Controllers/DevAuthController.cs` operativo en entorno de desarrollo.
- [x] 5.4 Verificar `SolarUY/Views/DevAuth/AccesoDenegado.cshtml` accesible para usuarios autenticados sin rol Administrador.
- [x] 5.5 Asegurar que el esquema `ApiKey` sigue funcionando para `/api/simulacion/*` sin interferir con el flujo OIDC.
- [x] 5.6 Crear `SolarUY/Controllers/AuthController.cs` con `[AllowAnonymous]`: `GET /login` que desafíe a OIDC cuando esté configurado o redirija a `/dev/login` cuando no, y `POST /logout` con `[ValidateAntiForgeryToken]` que cierre la cookie y, si OIDC está configurado, cierre también la sesión del IdP con `AuthenticationProperties { RedirectUri = OpcionesOidc.PostLogoutRedirectUri }` (RP-initiated logout).
- [x] 5.7 Actualizar `SolarUY/Views/Shared/_Layout.cshtml`: enlace "Iniciar sesión" a `Auth/Login` y formulario `POST` a `Auth/Logout` con token antiforgery para "Salir".
- [x] 5.8 Verificar que `DevAuthController` siga funcionando como fallback y que `/dev/login` no se use desde la UI cuando OIDC está configurado.
- [x] 5.9 Verificar que `AccessDeniedPath` (`/dev/denegado`) se muestre para usuarios autenticados sin rol Administrador.

## 6. Documentación

- [x] 6.1 Actualizar `ControlCalidad/README.md` con el estado del requerimiento `req010_Login_OIDC`, mencionando Keycloak local, RP-initiated logout, PKCE y protección de datos compartida.
- [x] 6.2 Actualizar `README.md`: tecnologías, tabla de funcionalidades, instrucciones de ejecución con Keycloak y usuarios de prueba.
- [x] 6.3 Actualizar `DESARROLLO.md`: agregar `keycloak` a la tabla de servicios, URLs (`http://localhost:8081`, admin console), credenciales de desarrollo, usuarios de prueba, verificación del realm y problemas frecuentes.
- [x] 6.4 Documentar en `DESARROLLO.md` que, si se cambia `APP_PORT`, deben actualizarse las redirect URIs del realm (`signin-oidc` y `signout-callback-oidc`).

## 7. Verificación

### Verificación ya realizada para el flujo base

- [x] 7.1 `docker compose run --rm tools dotnet build SolarUY.sln`.
- [x] 7.2 `docker compose run --rm tools dotnet test SolarUY.sln`.
- [x] 7.3 Ejecutar `./ControlCalidad/SonarQube/analizar.sh` y registrar hallazgos.
- [x] 7.4 Prueba manual del flujo base: sin sesión → redirección al login/fallback dev; autenticación exitosa → sesión con cookies y claims; con rol Administrador → acceso a gestión; sin rol Administrador → acceso denegado; `/api/simulacion/*` sigue funcionando con ApiKey.
- [x] 7.5 Confirmar que no se modificó `CodeQuality.Tests` sin autorización explícita.

### Verificación de Keycloak y del flujo completo

- [x] 7.6 `docker compose up -d --build` y verificar que `keycloak` esté disponible y que el realm `solaruy` se haya importado (`http://localhost:8081/realms/solaruy/.well-known/openid-configuration`).
- [x] 7.7 Iniciar sesión con `admin@solaruy.uy` desde el navegador: redirección a Keycloak, callback en `/signin-oidc` y acceso a las pantallas de gestión.
- [x] 7.8 Iniciar sesión con `operador@solaruy.uy`: sesión válida, pero acceso denegado en las operaciones de gestión.
- [x] 7.9 Ejecutar logout: se cierra la sesión local y la de Keycloak (RP-initiated logout); una ruta protegida vuelve a desafiar al IdP.
- [x] 7.10 Verificar PKCE: el cliente de Keycloak exige `S256` y el flujo completó; con PAR el `code_challenge` viaja en el POST al endpoint PAR, no en la URL visible.
- [x] 7.11 Verificar el mapeo de roles: el claim `roles` de Keycloak se refleja en `ClaimTypes.Role` y `[Authorize(Roles = "Administrador")]` funciona.
- [x] 7.12 Verificar validación de emisor y audiencia; con `RequireHttpsMetadata=true` en producción la metadata debe ser HTTPS.
- [x] 7.13 Verificar la sesión entre réplicas y tras reiniciar `app` reutilizando la DataProtection sobre Redis de req012: el callback OIDC puede caer en cualquier réplica sin perder la sesión.
- [x] 7.14 Verificar que el `redirect_uri` emitido por la app detrás de nginx sea `http://localhost:5080/signin-oidc` y que Keycloak lo acepte.
- [x] 7.15 Verificar que `/api/simulacion/*` sigue respondiendo con `X-Api-Key` y que sin la key responde `401`.
- [x] 7.16 Re-ejecutar `docker compose run --rm tools dotnet build SolarUY.sln` (equivalente local: `dotnet build SolarUY.sln`, en verde).
- [x] 7.17 Re-ejecutar `docker compose run --rm tools dotnet test SolarUY.sln` (equivalente local: `dotnet test SolarUY.sln`, en verde).
- [ ] 7.18 Re-ejecutar `./ControlCalidad/SonarQube/analizar.sh` y registrar hallazgos.
- [x] 7.19 Confirmar que no se modificó `CodeQuality.Tests` sin autorización explícita.

## Notas

- Sin migración de base de datos.
- Sin tests unitarios en Application/Infrastructure/Presentation.
- Keycloak usa `start-dev` y almacenamiento de desarrollo (H2): es solo para desarrollo/demostración.
- La alta disponibilidad (nginx, réplicas de `app`, Redis) proviene de req012; req010 no crea volúmenes ni modifica `docker/Dockerfile.app` para DataProtection.
- nginx debe preservar el puerto en el encabezado `Host` (`$http_host`) para que OIDC genere el `redirect_uri` correcto.
- nginx resuelve `app` dinámicamente y usa buffers de 32k para que el callback OIDC con cookies/headers grandes no devuelva `502`.
- .NET usa PAR (`request_uri`) cuando el IdP lo soporta; Keycloak 26 lo soporta. El `code_challenge` de PKCE se envía en el POST al endpoint PAR.
- Verificación manual completada: admin accede a `/ParquesSolares` (`200`), operador recibe `302` a `/dev/denegado`, logout RP-initiated vuelve a desafiar y la sesión sigue válida en `app-1` y `app-2` y tras `docker compose restart app`.
- La tarea 7.18 queda pendiente porque el `.env` local no tiene `SONAR_TOKEN`; el análisis no se puede ejecutar sin ese token.
- Los secretos de desarrollo (`KEYCLOAK_ADMIN_PASSWORD`, `OIDC_CLIENT_SECRET`) viven en `.env`; `.env.example` solo contiene valores locales de ejemplo.
- Las redirect URIs del realm asumen `APP_PORT=5080`; si se cambia el puerto, hay que actualizar el realm.
- El fallback `/dev/login` sigue disponible solo cuando OIDC no está configurado.
- `AccessDeniedPath` se mantiene en `/dev/denegado` para no cambiar el contrato actual.
- La Presentación solo orquesta desafíos/cierres de sesión; no contiene lógica de negocio ni accede a Infrastructure.
