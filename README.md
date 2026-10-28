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
- Autenticacion y Autorizacion: OpenID Connect (Proveedor externo) - stub de desarrollo en iteraciones previas
- Consultas inteligentes: Servicio de Procesamiento de Lenguaje Natural (NLP)
- Interfaz de Usuario: HTML5, CSS3, Bootstrap
- Control de calidad: ArchUnitNET (guardarrailes de arquitectura) + SonarQube (analisis estatico)

## Funcionalidades

| Modulo | Funcionalidad | Descripcion | Rol | Estado |
| --- | --- | --- | --- | --- |
| Seguridad | Login OIDC | Autenticacion externa OpenID Connect | Publico | Pendiente (stub de desarrollo activo) |
| Seguridad | Permisos | Control de roles (Admin / Operador) | Sistema | En curso (stub de desarrollo activo) |
| Parques | Parques | Gestion CRUD de parques solares | Admin | Listo |
| Parques | Nodos | Configuracion de nodos e inversores | Admin | Listo |
| Parques | Paneles | Registro de paneles fotovoltaicos | Admin | Pendiente |
| Mantenimiento | Tareas | Registro y seguimiento de tareas | Admin/Personal | Pendiente |
| Monitoreo | Dashboard | Telemetria de energia en vivo via SignalR | Todos | Pendiente |
| Monitoreo | Alertas | Deteccion de fallos en paneles | Todos | Pendiente |
| Historicos | Reportes | Metricas de energia por dia/mes/ano | Todos | Pendiente |
| Historicos | Consulta IA | Estado del sistema en lenguaje natural | Todos | Pendiente |
| Simulacion | Mock Inversor | Simulacion de telemetria y sensores ARM | Admin | Pendiente |

## Arquitectura

El proyecto adopta una arquitectura monolitica estructurada en cuatro niveles:

1. Capa de Presentacion: Interfaz web construida con Bootstrap y cliente SignalR para recibir actualizaciones en vivo.
2. Aplicacion Monolitica: Controladores MVC, endpoints de API, SignalR Hub y logica de negocio en ASP.NET Core.
3. Servicios Integrados y Simulacion: Integracion con proveedor externo de identidades (OIDC), motor de IA para consultas en lenguaje natural y Mock de Inversores/Sensores ARM.
4. Capa de Datos: Base de datos MySQL 8.4 gestionada mediante el ORM Entity Framework Core.

Internamente el monolito se organiza en capas (carpetas + namespaces) dentro del proyecto `SolarUY`:

| Capa | Carpeta | Namespace |
| --- | --- | --- |
| Dominio | `Domain/` | `SolarUY.Domain` |
| Aplicacion | `Application/` | `SolarUY.Application` |
| Infraestructura | `Infrastructure/` | `SolarUY.Infrastructure` |
| Presentacion | `Controllers/`, `Views/`, `Models/` | `SolarUY.Controllers`, `SolarUY.Models` |

<img width="719" height="979" alt="image" src="https://github.com/user-attachments/assets/68e71eac-7042-4a7a-923f-ecd11c6b23c2" />

## Ejecucion con Docker (entorno recomendado)

Todo el stack corre en contenedores: no hace falta instalar el SDK de .NET ni MySQL en la maquina.

```bash
cp .env.example .env          # credenciales locales (MySQL 8.4 en el puerto 3307)
docker compose up -d           # MySQL + migraciones + aplicación
docker compose logs -f app
```

- Aplicacion: <http://localhost:5080>
- MySQL: `127.0.0.1:3307` (usuario `solaruy`)

Comandos utiles:

```bash
docker compose run --rm tools dotnet test SolarUY.sln        # pruebas + guardarrailes
docker compose run --rm tools dotnet build SolarUY.sln       # compilacion
docker compose exec mysql mysql -usolaruy -pSolarUY.Local2026 SolarUY   # consola MySQL
docker compose down                                        # apagar (conserva los datos)
docker compose down -v                                     # apagar y borrar los datos
```

La guia completa de instalacion para cada equipo esta en [DESARROLLO.md](DESARROLLO.md).

## Metodologia de trabajo: SDD + control de calidad

El proyecto aplica **Spec-Driven Development** con artefactos de prompting por requerimiento en
`ControlCalidad/prompting/<requerimiento>/` (`requirements.md`, `specs.md`, `tasks.md`) y dos
guardarrailes automaticos:

1. **ArchUnitNET** (`ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests`): verifica las reglas de Clean Architecture (dependencias hacia adentro, repositorios en Infraestructura, naming, dominio sin EF).
2. **SonarQube** (`ControlCalidad/SonarQube`): analisis estatico de codigo (code smells, duplicacion, cobertura).

Detalle de uso: [ControlCalidad/README.md](ControlCalidad/README.md).
