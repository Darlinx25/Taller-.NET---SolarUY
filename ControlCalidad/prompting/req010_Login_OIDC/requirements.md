# Requerimientos: Login OIDC

## Descripción General

Como usuario de SolarUY, quiero autenticarme y cerrar sesión contra un Identity Provider (IdP) externo que cumpla con OpenID Connect (OIDC), para acceder al sistema con una identidad centralizada y con permisos diferenciados. Solo los usuarios con rol "Administrador" pueden realizar las operaciones de gestión (alta, edición y baja de parques, nodos y paneles).

La solución debe incluir un IdP propio basado en **Keycloak** que se levante junto con el resto de los servicios locales mediante `docker compose`, de modo que el flujo OIDC completo (login, callback, roles y logout) sea demostrable de punta a punta sin depender de un proveedor en la nube. En producción, la misma aplicación debe poder apuntar a un IdP externo cambiando únicamente la configuración.

## Criterios de Aceptación

1. El sistema debe delegar la autenticación a un Identity Provider externo mediante OpenID Connect.
2. Cuando un usuario no autenticado intente acceder a una ruta protegida y OIDC esté configurado, el sistema debe redirigirlo directamente al IdP (en desarrollo, el Keycloak local) para autenticarse.
3. Tras la autenticación exitosa en el IdP, el sistema debe recibir el callback OIDC, validar la respuesta y crear una sesión local con cookies.
4. El sistema debe obtener los claims del usuario desde el token/identity provisto por el IdP, incluyendo la información necesaria para determinar el rol.
5. Debe existir distinción de roles (Administrador vs Operador) basada en los claims provenientes del IdP.
6. Solo los usuarios autenticados con rol "Administrador" pueden realizar las operaciones de gestión (alta/edición/baja de parques, nodos y paneles).
7. Las operaciones de gestión deben permanecer protegidas con `[Authorize(Roles = "Administrador")]`.
8. Si un usuario autenticado no tiene rol Administrador e intenta acceder a una operación de gestión, debe mostrarse la pantalla de acceso denegado (`/dev/denegado` o ruta configurada).
9. Debe mantenerse el esquema `ApiKey` existente para los endpoints internos del simulador (`/api/simulacion/*`), sin interferir con el flujo OIDC.
10. Debe mantenerse la posibilidad de autenticación en entorno de desarrollo para facilitar pruebas cuando OIDC no esté configurado (fallback a `/dev/login`).
11. `docker compose up` debe levantar un servicio Keycloak con un realm `solaruy` ya importado, un cliente confidencial `solaruy-app`, los roles `Administrador` y `Operador`, y dos usuarios de prueba: `admin@solaruy.uy` (rol Administrador) y `operador@solaruy.uy` (rol Operador). El administrador de Keycloak debe quedar disponible para inspeccionar el realm.
12. Un usuario con rol Administrador debe poder iniciar sesión y acceder a la gestión; un usuario con rol Operador debe poder iniciar sesión pero recibir acceso denegado en las operaciones de gestión.
13. El logout debe cerrar tanto la sesión local de SolarUY como la sesión del IdP (RP-initiated logout) y regresar al usuario a SolarUY.
14. El flujo de autorización `authorization code` debe usar PKCE.
15. Los roles provistos por el IdP (roles de realm y, si aplica, roles de cliente) deben reconocerse como roles de la aplicación.
16. El sistema debe validar el emisor y la audiencia del token; en producción debe exigir HTTPS para la metadata OIDC.
17. La sesión debe sobrevivir al reinicio de la aplicación y ser válida entre réplicas de la aplicación cuando se compartan las claves de protección de datos (preparación para alta disponibilidad).
18. Las credenciales del IdP y los secretos deben configurarse por variables de entorno o configuración externa, nunca fijarse en el código fuente.

## Reglas de Negocio

- La autenticación debe ser externa mediante OpenID Connect (delegada al IdP).
- La sesión de la aplicación debe gestionarse con cookies ASP.NET Core tras el callback OIDC.
- La autorización para operaciones de gestión es exclusiva de usuarios con rol "Administrador".
- Los roles deben obtenerse a partir de los claims provistos por el IdP (mapeo de claims/roles).
- Keycloak es el IdP local de desarrollo y demostración. No reemplaza a un IdP productivo: el mismo código debe funcionar contra un proveedor externo mediante configuración.
- El logout debe terminar la sesión en SolarUY y en el IdP.
- Las rutas de login, logout y acceso denegado deben estar correctamente configuradas.
- No se debe relajar la protección existente de los módulos de gestión.
- El stub de desarrollo (`DevAuthController`) se mantiene solo como mecanismo alternativo cuando OIDC no está configurado; nunca debe reemplazar al flujo OIDC cuando este sí está configurado.
- La aplicación no administra usuarios ni contraseñas: el alta, la baja y la asignación de roles se realizan en Keycloak (o en el IdP productivo).
- La alta disponibilidad (balanceador, réplicas y estado compartido) está cubierta por el req012. Este requerimiento solo garantiza que el login y el logout funcionen correctamente a través del balanceador.
- Las credenciales de desarrollo de Keycloak son de uso local y no deben reutilizarse en producción.

## Fuera de Alcance

- Implementar un login propio con usuarios y contraseñas dentro de SolarUY: la autenticación se delega siempre a un IdP OIDC.
- Desplegar un IdP productivo en la nube o una infraestructura de alta disponibilidad real con balanceador y múltiples réplicas.
- Rediseñar la infraestructura de alta disponibilidad ya incorporada en req012.
- Registro/autogestión de usuarios, recuperación de contraseña y administración de identidades desde SolarUY.
- Migración de usuarios o roles desde otros sistemas.
- Cambios en lógica de negocio distinta a autenticación/autorización por roles.
- Modificación de `CodeQuality.Tests` sin autorización explícita.
