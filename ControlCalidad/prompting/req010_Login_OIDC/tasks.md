# Tareas: req009 Login OIDC

> Cada tarea debe ser verificable, indicar el archivo o la ruta cuando aplique y comenzar con `[ ]`. Marcar `[x]` solo cuando esté terminada y verificada.

## 0. Preparación

- [ ] 0.1 Confirmar que `requirements.md`, `specs.md` y este plan están aprobados.
- [ ] 0.2 Revisar `SolarUY/SolarUY.csproj`, `SolarUY/Program.cs`, controladores de gestión existentes y `DevAuthController` para respetar patrones actuales.

## 1. Dominio (`SolarUY.Domain`)

- [ ] 1.1 Verificar que no se requieren cambios en entidades, excepciones o contratos de repositorio. Sin cambios.

## 2. Tests del Dominio (`SolarUY.Tests/Domain`)

- [ ] 2.1 Verificar que no hay lógica de dominio nueva. No agregar tests de dominio nuevos; asegurar que los tests existentes siguen pasando.

## 3. Aplicación (`SolarUY.Application`)

- [ ] 3.1 Verificar que no se requieren cambios en DTOs o servicios. Mantener separación de capas.

## 4. Infraestructura (`SolarUY.Infrastructure`)

- [ ] 4.1 Verificar esquema de base de datos: no hay cambios en entidades/configuraciones; **no se genera migración**.
- [x] 4.2 Agregar paquete NuGet `Microsoft.AspNetCore.Authentication.OpenIdConnect` al proyecto `SolarUY/SolarUY.csproj`.
- [x] 4.3 Configurar autenticación externa en `SolarUY/Program.cs`: mantener `AddAuthentication` con esquema Cookies como predeterminado, agregar `AddOpenIdConnect("oidc", ...)` con `Authority`, `ClientId`, `ClientSecret`, `CallbackPath`, `ResponseType`, `SaveTokens`, `GetClaimsFromUserInfoEndpoint` leídos desde configuración. Mantener `AddScheme` para `ApiKeyAuthenticationHandler` (`AutenticacionComponentes.EsquemaApiKey`).
- [x] 4.4 Implementar mapeo de claims/roles desde el IdP a `ClaimTypes.Role` (para que `[Authorize(Roles = "Administrador")]` funcione). Configurar transformación de claims en las opciones de OpenIdConnect.
- [x] 4.5 Mantener orden de middleware: `UseRouting()`, `UseAuthentication()`, `UseAuthorization()`. Mantener rutas `LoginPath`, `LogoutPath`, `AccessDeniedPath`.

## 5. Presentación (`SolarUY.Controllers` + `Views`)

- [x] 5.1 Verificar que los controladores de gestión mantienen `[Authorize(Roles = "Administrador")]`: `SolarUY/Controllers/ParquesSolaresController.cs`, `SolarUY/Controllers/NodosController.cs`, `SolarUY/Controllers/PanelesController.cs`, `SolarUY/Controllers/TareasMantenimientoController.cs`, `SolarUY/Controllers/TelemetriaController.cs`.
- [x] 5.2 Asegurar redirección al IdP cuando corresponda (challenge OIDC) y mantener fallback `/dev/login` para desarrollo si no hay IdP configurado.
- [x] 5.3 Mantener `SolarUY/Controllers/DevAuthController.cs` operativo en entorno de desarrollo.
- [x] 5.4 Verificar `SolarUY/Views/DevAuth/AccesoDenegado.cshtml` accesible para usuarios autenticados sin rol Administrador.
- [x] 5.5 Asegurar que el esquema `ApiKey` sigue funcionando para `/api/simulacion/*` sin interferir con el flujo OIDC.

## 6. Documentación

- [ ] 6.1 Actualizar `ControlCalidad/README.md` con el estado del requerimiento `req009<Login OIDC>`.
- [ ] 6.2 Actualizar `README.md` si cambia el estado de una funcionalidad relacionada con autenticación/autorización externa OpenID Connect.

## 7. Verificación

- [x] 7.1 `docker compose run --rm tools dotnet build SolarUY.sln`
- [x] 7.2 `docker compose run --rm tools dotnet test SolarUY.sln`
- [x] 7.3 Ejecutar `./ControlCalidad/SonarQube/analizar.sh` y registrar hallazgos.
- [x] 7.4 Prueba manual del flujo completo: sin sesión → redirección al login/fallback dev; autenticación exitosa → sesión con cookies y claims; con rol Administrador → acceso a gestión (parques/nodos/paneles); sin rol Administrador → acceso denegado; `/api/simulacion/*` sigue funcionando con ApiKey.
- [x] 7.5 Confirmar que no se modificó `CodeQuality.Tests` sin autorización explícita.

## Notas

- Sin migración de base de datos.
- Sin tests unitarios en Application/Infrastructure/Presentation.
- Debe usarse `Microsoft.AspNetCore.Authentication.OpenIdConnect` para delegación al IdP externo.
- Mantener compatibilidad con esquema ApiKey para `/api/simulacion/*`.
- Los controladores de gestión deben seguir protegidos con `[Authorize(Roles = "Administrador")]`.
- Implementaciones front deben ser MVC con SignalR según lo indicado.
