# Requerimientos: Login OIDC

## Descripción General

Como usuario del sistema SolarUY, quiero autenticarme mediante un sistema de autenticación/autorización externo que cumpla con el estándar OpenID Connect (OIDC), para acceder al sistema con mi identidad delegada a un Identity Provider (IdP) externo. El sistema debe diferenciar los permisos entre usuarios administradores y demás usuarios, de modo que solo los usuarios con rol "Administrador" puedan realizar las operaciones de gestión (alta, edición y baja de parques, nodos, paneles).

## Criterios de Aceptación

1. El sistema debe delegar la autenticación a un Identity Provider externo mediante OpenID Connect.
2. Cuando un usuario no autenticado intente acceder a una ruta protegida y OIDC esté configurado, el sistema debe redirigirlo directamente al IdP externo (flujo de desafío OIDC) para autenticarse, tal como indica el diagrama.
3. Tras la autenticación exitosa en el IdP, el sistema debe recibir el callback OIDC, validar la respuesta y crear una sesión local (cookies) para el usuario.
4. El sistema debe obtener los claims del usuario desde el token/identity provisto por el IdP, incluyendo la información necesaria para determinar el rol.
5. Debe existir distinción de roles (Administrador vs otros usuarios) basada en los claims provenientes del IdP.
6. Solo los usuarios autenticados con rol "Administrador" pueden realizar las operaciones de gestión (alta/edición/baja de parques, nodos, paneles).
7. Las operaciones de gestión deben permanecer protegidas con `[Authorize(Roles = "Administrador")]`.
8. Si un usuario autenticado no tiene rol Administrador e intenta acceder a una operación de gestión, debe mostrarse la pantalla de acceso denegado (`/dev/denegado` o ruta configurada).
9. Debe mantenerse el esquema `ApiKey` existente para los endpoints internos del simulador (`/api/simulacion/*`), sin interferir con el flujo OIDC.
10. Debe mantenerse la posibilidad de autenticación en entorno de desarrollo para facilitar pruebas hasta contar con un IdP configurado (fallback a `/dev/login` cuando OIDC no esté configurado).

## Reglas de Negocio

- La autenticación debe ser externa mediante OpenID Connect (delegada al IdP).
- La sesión de la aplicación debe gestionarse con cookies ASP.NET Core tras el callback OIDC.
- La autorización para operaciones de gestión es exclusiva de usuarios con rol "Administrador".
- Los roles deben obtenerse a partir de los claims provistos por el IdP (mapeo de claims/roles).
- Las rutas de login/logout/access denied deben estar correctamente configuradas.
- No se debe relajar la protección existente de los módulos de gestión.
- El stub de desarrollo (`DevAuthController`) puede mantenerse como mecanismo alternativo para pruebas locales, sin reemplazar el flujo OIDC cuando esté configurado.

## Fuera de Alcance

- Despliegue/configuración de un Identity Provider propio en producción.
- Implementación de registro/autogestión de usuarios.
- Cambios en lógica de negocio distinta a autenticación/autorización por roles.
- Modificación de `CodeQuality.Tests` sin autorización explícita.
