# SolarUY - Sistema de Gestion de Parques Solares
## Integrantes

- Facundo Salaberry
- Kevin Jaffe
- Alexis Menchaca
- Ignacio Ortega

## Tecnologias

- Framework Web: ASP.NET Core MVC (.NET 10)
- Persistencia: Entity Framework Core / MySQL 8.4
- Comunicacion en tiempo real: SignalR
- Autenticacion y Autorizacion: OpenID Connect con Keycloak local (contenedor) y sesion por cookies
- Consultas inteligentes: Servidor MCP de solo lectura que expone el estado del sistema a un agente externo (Antigravity + Gemini)
- Interfaz de Usuario: HTML5, CSS3, Bootstrap
- Control de calidad: ArchUnitNET (guardarrailes de arquitectura) + SonarQube (analisis estatico)

## Funcionalidades

| Modulo | Funcionalidad | Descripcion | Rol | Estado |
| --- | --- | --- | --- | --- |
| Seguridad | Login OIDC | Autenticacion externa OpenID Connect con Keycloak local, PKCE, RP-initiated logout, mapeo de roles y sesión distribuida | Publico | Listo |
| Seguridad | Permisos | Control de roles (Administrador / Operador) mapeados desde Keycloak | Sistema | Listo |
| Parques | Parques | Gestion CRUD de parques solares | Admin | Listo |
| Parques | Nodos | Configuracion de nodos e inversores | Admin | Listo |
| Parques | Paneles | Registro de paneles fotovoltaicos | Admin | Listo |
| Mantenimiento | Tareas | Registro y seguimiento de tareas por panel | Admin | Listo |
| Monitoreo | Dashboard | Topologia registrada, telemetria de energia en vivo via SignalR sin recarga y graficas de generacion, estado de paneles y tendencia de 7 dias | Todos | Listo |
| Monitoreo | Alertas | Deteccion de fallos en paneles, con historial, pantalla e indicador en vivo | Todos | Listo |
| Historicos | Reportes | Metricas de energia por dia/mes/ano | Todos | Listo |
| Historicos | Consulta IA | Consulta del estado del sistema en lenguaje natural desde Antigravity via servidor MCP de solo lectura (req011) | Todos | Listo |
| Simulacion | Mock Inversor | Simulacion de telemetria y sensores ARM | Admin | Listo |

## Arquitectura

El proyecto adopta una arquitectura monolitica estructurada en cuatro niveles:

1. Capa de Presentacion: Interfaz web construida con Bootstrap y cliente SignalR para recibir actualizaciones en vivo.
2. Aplicacion Monolitica: Controladores MVC, endpoints de API, SignalR Hub y logica de negocio en ASP.NET Core.
3. Servicios Integrados y Simulacion: Integracion con proveedor externo de identidades (OIDC), servidor MCP de solo lectura que expone el estado del sistema a un agente externo (Antigravity + Gemini) y Mock de Inversores/Sensores ARM. El mock es el componente independiente `simulador` (carpeta [`Simulador/`](Simulador/README.md), su propio contenedor): una placa ARM simulada compilada a WebAssembly y ejecutada con Node.js que obtiene la topologia registrada y envia la telemetria por HTTP al balanceador de carga; se levanta con el stack, no corre dentro del monolito, no comparte codigo con el y no accede a MySQL.
4. Capa de Datos: Base de datos MySQL 8.4 gestionada mediante el ORM Entity Framework Core.

Internamente el monolito se organiza en capas (carpetas + namespaces) dentro del proyecto `SolarUY`:

| Capa | Carpeta | Namespace |
| --- | --- | --- |
| Dominio | `Domain/` | `SolarUY.Domain` |
| Aplicacion | `Application/` | `SolarUY.Application` |
| Infraestructura | `Infrastructure/` | `SolarUY.Infrastructure` |
| Presentacion | `Controllers/`, `Views/`, `Models/` | `SolarUY.Controllers`, `SolarUY.Models` |

<img width="719" height="979" alt="image" src="https://github.com/user-attachments/assets/68e71eac-7042-4a7a-923f-ecd11c6b23c2" />

## Metodologia de trabajo: SDD + control de calidad

El proyecto aplica **Spec-Driven Development** con artefactos de prompting por requerimiento en
`ControlCalidad/prompting/<requerimiento>/` (`requirements.md`, `specs.md`, `tasks.md`) y dos
guardarrailes automaticos:

1. **ArchUnitNET** (`ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests`): verifica las reglas de Clean Architecture (dependencias hacia adentro, repositorios en Infraestructura, naming, dominio sin EF).
2. **SonarQube** (`ControlCalidad/SonarQube`): analisis estatico de codigo (code smells, duplicacion, cobertura).

Detalle de uso: [ControlCalidad/README.md](ControlCalidad/README.md).

## Desarrollo con IA

Para este proyecto utilizamos OpenCode con el modelo gratuito predeterminado, Big Pickle.\
Algunas conclusiones/aprendizajes al desarrollar con IA:

- Hay que definir bien los requerimientos para que el agente no tome decisiones por si solo, por ejemplo no dijimos que queríamos borrado lógico de parques solares, entonces la IA implementó la que eligió, que fue borrado de la base de datos físico DELETE con sql. Y si no nos fijamos bien como implementa todo, podemos pasar por alto cosas importantes.
- Se nos dificulta a veces verificar exhaustivamente lo que implementa la IA, al ser más sencillo es difícil resistir el "vibecoding" por así llamarlo, aunque parezca que funciona todo hay que verificar y entender lo que implementa, como el ejemplo de borrado lógico de arriba.
- A veces no hace caso, tiende a cortar camino siguiendo el Spec-Driven Development ejemplo le decís que haga el specs.md y que vas a leerlo antes de proseguir y ya te hace el tasks.md e implementa todo ignorando tu prompt anterior, probablemente esto sea error nuestro por no hacer bien las skills y demás.
