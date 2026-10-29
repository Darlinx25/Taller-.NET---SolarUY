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
| Parques | Paneles | Registro de paneles fotovoltaicos | Admin | Listo |
| Mantenimiento | Tareas | Registro y seguimiento de tareas | Admin/Personal | Pendiente |
| Monitoreo | Dashboard | Telemetria de energia en vivo via SignalR | Todos | Pendiente |
| Monitoreo | Alertas | Deteccion de fallos en paneles | Todos | Pendiente |
| Historicos | Reportes | Metricas de energia por dia/mes/ano | Todos | Pendiente |
| Historicos | Consulta IA | Estado del sistema en lenguaje natural | Todos | Pendiente |
| Simulacion | Mock Inversor | Simulacion de telemetria y sensores ARM | Admin | Listo |

## Arquitectura

El proyecto adopta una arquitectura monolitica estructurada en cuatro niveles:

1. Capa de Presentacion: Interfaz web construida con Bootstrap y cliente SignalR para recibir actualizaciones en vivo.
2. Aplicacion Monolitica: Controladores MVC, endpoints de API, SignalR Hub y logica de negocio en ASP.NET Core.
3. Servicios Integrados y Simulacion: Integracion con proveedor externo de identidades (OIDC), motor de IA para consultas en lenguaje natural y Mock de Inversores/Sensores ARM. El mock es el componente independiente `simulador` (proyecto `SolarUY.Simulador`, su propio contenedor): obtiene la topologia registrada de la aplicacion y le envia la telemetria por HTTP; no corre dentro del monolito, no comparte codigo con el y no accede a MySQL.
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
docker compose up -d           # MySQL + migraciones + aplicación + simulador (mock del inversor)
docker compose logs -f app
```

- Aplicacion: <http://localhost:5080>
- MySQL: `127.0.0.1:3307` (usuario `solaruy`)

El simulador no requiere configuracion adicional: levanta junto con el resto y usa una API key de desarrollo por defecto (se puede cambiar con `SIMULACION_API_KEY` en el `.env`).

Comandos utiles:

```bash
docker compose run --rm tools dotnet test SolarUY.sln        # pruebas + guardarrailes
docker compose run --rm tools dotnet build SolarUY.sln       # compilacion
docker compose exec mysql mysql -usolaruy -pSolarUY.Local2026 SolarUY   # consola MySQL
docker compose down                                        # apagar (conserva los datos)
docker compose down -v                                     # apagar y borrar los datos
```

La guia completa de instalacion para cada equipo esta en [DESARROLLO.md](DESARROLLO.md).

### Verificacion del mock del inversor

El simulador genera una lectura cada 5 segundos para cada parque registrado y la envia a la aplicacion
(`POST /api/simulacion/telemetria`, autenticado con API key). La aplicacion conserva solo la ultima lectura,
en memoria (sin persistencia ni migraciones), y la expone al Administrador:

```bash
docker compose logs -f simulador                  # lectura generada y envio de cada tick
curl -c cookies.txt http://localhost:5080/dev/login   # sesion de desarrollo (rol Administrador)
curl -b cookies.txt http://localhost:5080/api/telemetria   # 204 antes de la primera lectura, 200 despues
```

La generacion sigue una curva solar (0 W fuera de 07:00-19:00, hora de Uruguay). Un balanceador de carga con
replicas de la aplicacion y Redis para compartir la ultima lectura es una evolucion prevista que no se incorpora
todavia; el detalle esta en `ControlCalidad/prompting/req006_mock_inversor/specs.md`.

## Metodologia de trabajo: SDD + control de calidad

El proyecto aplica **Spec-Driven Development** con artefactos de prompting por requerimiento en
`ControlCalidad/prompting/<requerimiento>/` (`requirements.md`, `specs.md`, `tasks.md`) y dos
guardarrailes automaticos:

1. **ArchUnitNET** (`ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests`): verifica las reglas de Clean Architecture (dependencias hacia adentro, repositorios en Infraestructura, naming, dominio sin EF).
2. **SonarQube** (`ControlCalidad/SonarQube`): analisis estatico de codigo (code smells, duplicacion, cobertura).

Detalle de uso: [ControlCalidad/README.md](ControlCalidad/README.md).
