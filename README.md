# SolarUY - Sistema de Gestion de Parques Solares
## Integrantes

- Facundo Salaberry
- Kevin Jaffe
- Alexis Menchaca
- Ignacio Ortega

## Tecnologias

- Framework Web: ASP.NET Core MVC
- Persistencia: Entity Framework Core / SQL Server
- Comunicacion en tiempo real: SignalR
- Autenticacion y Autorizacion: OpenID Connect (Proveedor externo)
- Consultas inteligentes: Servicio de Procesamiento de Lenguaje Natural (NLP)
- Interfaz de Usuario: HTML5, CSS3, Bootstrap

## Funcionalidades

| Modulo | Funcionalidad | Descripcion | Rol |
| --- | --- | --- | --- |
| Seguridad | Login OIDC | Autenticacion externa OpenID Connect | Publico |
| Seguridad | Permisos | Control de roles (Admin / Operador) | Sistema |
| Parques | Parques | Gestion CRUD de parques solares | Admin |
| Parques | Nodos | Configuracion de nodos e inversores | Admin |
| Parques | Paneles | Registro de paneles fotovoltaicos | Admin |
| Mantenimiento | Tareas | Registro y seguimiento de tareas | Admin/Personal |
| Monitoreo | Dashboard | Telemetria de energia en vivo via SignalR | Todos |
| Monitoreo | Alertas | Deteccion de fallos en paneles | Todos |
| Historicos | Reportes | Metricas de energia por dia/mes/ano | Todos |
| Historicos | Consulta IA | Estado del sistema en lenguaje natural | Todos |
| Simulacion | Mock Inversor | Simulacion de telemetria y sensores ARM | Admin |

## Arquitectura

El proyecto adopta una arquitectura monolitica estructurada en cuatro niveles:

1. Capa de Presentacion: Interfaz web construida con Bootstrap y cliente SignalR para recibir actualizaciones en vivo.
2. Aplicacion Monolitica: Controladores MVC, endpoints de API, SignalR Hub y logica de negocio en ASP.NET Core.
3. Servicios Integrados y Simulacion: Integracion con proveedor externo de identidades (OIDC), motor de IA para consultas en lenguaje natural y Mock de Inversores/Sensores ARM.
4. Capa de Datos: Base de datos SQL Server gestionada mediante el ORM Entity Framework Core.

<img width="719" height="979" alt="image" src="https://github.com/user-attachments/assets/68e71eac-7042-4a7a-923f-ecd11c6b23c2" />

