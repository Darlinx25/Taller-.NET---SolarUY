# Especificación Técnica: Login OIDC

## Objetivo

Implementar autenticación y autorización externa mediante OpenID Connect (OIDC) en el monolito ASP.NET Core MVC. La aplicación debe delegar la autenticación a un Identity Provider (IdP) externo, recibir el callback OIDC, crear una sesión local con cookies y mapear roles desde los claims para mantener el control de acceso por `[Authorize(Roles = "Administrador")]` en los módulos de gestión.

## Decisiones y supuestos

- Arquitectura: monolito ASP.NET Core MVC organizado en capas (Domain, Application, Infrastructure, Presentation), sin nuevos proyectos `.csproj`.
- Autenticación externa: OpenID Connect. Usar `Microsoft.AspNetCore.Authentication.OpenIdConnect` junto con `CookieAuthentication` para la sesión local.
- Autorización: basada en roles. Solo usuarios con rol `Administrador` pueden realizar operaciones de gestión (alta/edición/baja de parques, nodos, paneles).
- Se mantiene el esquema `ApiKeyAuthenticationHandler` para endpoints del simulador (`/api/simulacion/*`).
- Se mantiene `DevAuthController` como fallback para desarrollo cuando no hay IdP configurado.
- Los controladores de gestión ya existentes mantienen `[Authorize(Roles = "Administrador")]`.

## 1) Dominio (`SolarUY.Domain`)

### Entidades
- Sin cambios en entidades existentes.

### Validaciones
- Sin cambios.

### Excepciones
- Sin nuevas excepciones de dominio requeridas.

### Repositorios (contratos)
- Sin cambios.

## 2) Aplicación (`SolarUY.Application`)

### DTOs
- Sin nuevos DTOs requeridos.

### Servicios y casos de uso
- Sin cambios en servicios de aplicación.

## 3) Infraestructura (`SolarUY.Infrastructure`)

### DbSet y configuraciones
- Sin cambios de esquema.

### Repositorios
- Sin cambios.

### Migración
- **No requiere migración.** No hay cambios en entidades, DbSets o configuraciones de EF Core.

### Registro de dependencias (`Program.cs`)
- Agregar referencia/uso de `Microsoft.AspNetCore.Authentication.OpenIdConnect`.
- Configurar autenticación híbrida:
  - `AddAuthentication(...)` con esquema por defecto `CookieAuthenticationDefaults.AuthenticationScheme`, esquema de sign-in en cookies y `DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme` cuando OIDC esté configurado (`Authority` y `ClientId` presentes). Si no está configurado, se mantiene challenge a cookies con `LoginPath = "/dev/login"`.
  - `AddCookie(...)` con `LoginPath`, `LogoutPath`, `AccessDeniedPath` configurados.
  - `AddOpenIdConnect("oidc", options => { ... })` con `Authority`, `ClientId`, `ClientSecret`, `CallbackPath`, `ResponseType="code"`, `SaveTokens=true`, `GetClaimsFromUserInfoEndpoint=true` (configurables vía `appsettings`/`Configuration`).
- Configurar mapeo de roles/claims: transformar claims del IdP (ej. `roles`, `role`, `groups` o claim personalizado) a `ClaimTypes.Role` para que funcione con `[Authorize(Roles = "Administrador")]`.
- Mantener `AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(AutenticacionComponentes.EsquemaApiKey, null)`.
- Mantener `AddAuthorization()`.
- Orden de middleware: `UseRouting()`, `UseAuthentication()`, `UseAuthorization()`.
- Permitir fallback a `/dev/login` en desarrollo si OIDC no está completamente configurado (sin romper el flujo).
- El navegador se redirige directamente al IdP externo cuando hay un desafío de autenticación y OIDC está configurado (conforme al diagrama).

## 4) Presentación (`SolarUY.Controllers` + `Views`)

### Controlador y acciones
- Los controladores de gestión mantienen `[Authorize(Roles = "Administrador")]`: `ParquesSolaresController`, `NodosController`, `PanelesController`, `TareasMantenimientoController`, `TelemetriaController`.
- Configurar challenge OIDC: cuando corresponda, usar `Challenge("oidc")` para redirigir al IdP. `DevAuthController` permanece para entorno de desarrollo.
- Mantener `Views/DevAuth/AccesoDenegado.cshtml` accesible.

### Autorización
- Delegación al IdP externo vía OIDC.
- Mapeo de roles desde claims OIDC a `ClaimTypes.Role`.
- Acceso denegado redirige a `AccessDeniedPath`.
- Sin sesión → redirección al IdP (o fallback dev).

### Vistas
- Sin cambios estructurales obligatorios. Patrón MVC con Bootstrap mantenido. Si aplica, se puede ajustar navegación para mostrar login/logout según autenticación.

## 5) Pruebas

- **Tests xUnit del Dominio (obligatorios):** No se agregan tests de dominio nuevos (sin cambios en Domain). No se modifican tests existentes.
- **No se agregan tests unitarios en Application, Infrastructure ni Presentation.**

## 6) Calidad y verificación

- [ ] Agregar paquete `Microsoft.AspNetCore.Authentication.OpenIdConnect` al proyecto `SolarUY`.
- [ ] Configurar `AddOpenIdConnect` en `SolarUY/Program.cs` con opciones desde configuración.
- [ ] Implementar mapeo de claims/roles desde el IdP.
- [ ] Mantener esquema `ApiKey` para `/api/simulacion/*` funcional.
- [ ] Mantener `[Authorize(Roles = "Administrador")]` en controladores de gestión.
- [ ] Build: `docker compose run --rm tools dotnet build SolarUY.sln`
- [ ] Tests: `docker compose run --rm tools dotnet test SolarUY.sln`
- [ ] ArchUnitNET en verde
- [ ] SonarQube ejecutado y hallazgos revisados
- [ ] Prueba manual: sin sesión → redirección al flujo OIDC/fallback; con rol Admin → acceso a gestión; sin rol Admin → acceso denegado.

## 7) Fuera de alcance

- Configuración de un IdP real en producción.
- Registro/autogestión de usuarios.
- Cambios en lógica de negocio distinta a autenticación/autorización.
- Modificación de `CodeQuality.Tests` sin autorización explícita.
