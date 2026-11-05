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
| Monitoreo | Dashboard | Topologia registrada y telemetria de energia en vivo via SignalR, sin recarga | Todos | Listo |
| Monitoreo | Alertas | Deteccion de fallos en paneles, con historial, pantalla e indicador en vivo | Todos | Listo |
| Historicos | Reportes | Metricas de energia por dia/mes/ano | Todos | Listo |
| Historicos | Consulta IA | Consulta del estado del sistema en lenguaje natural desde Antigravity via servidor MCP de solo lectura (req011) | Todos | Listo |
| Simulacion | Mock Inversor | Simulacion de telemetria y sensores ARM | Admin | Listo |

## Arquitectura

El proyecto adopta una arquitectura monolitica estructurada en cuatro niveles:

1. Capa de Presentacion: Interfaz web construida con Bootstrap y cliente SignalR para recibir actualizaciones en vivo.
2. Aplicacion Monolitica: Controladores MVC, endpoints de API, SignalR Hub y logica de negocio en ASP.NET Core.
3. Servicios Integrados y Simulacion: Integracion con proveedor externo de identidades (OIDC), servidor MCP de solo lectura que expone el estado del sistema a un agente externo (Antigravity + Gemini) y Mock de Inversores/Sensores ARM. El mock es el componente independiente `simulador` (proyecto `SolarUY.Simulador`, su propio contenedor): obtiene la topologia registrada de la aplicacion y le envia la telemetria por HTTP; no corre dentro del monolito, no comparte codigo con el y no accede a MySQL.
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
docker compose up -d           # MySQL + migraciones + Redis + Keycloak + app (réplicas) + nginx + simulador
docker compose logs -f app
```

- Aplicacion (via nginx): <http://localhost:5080>
- Keycloak (IdP local): <http://localhost:8081> (consola en `/admin`)
- Usuarios de desarrollo: `admin@solaruy.uy` (Administrador) y `operador@solaruy.uy` (Operador), password `SolarUY.Local2026`
- MCP (solo lectura): <http://127.0.0.1:5080/mcp>
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
(`POST /api/simulacion/telemetria`, autenticado con API key). La aplicacion conserva la ultima lectura en
memoria (se pierde al reiniciar) y la expone al Administrador; los resumenes diarios para los historicos
(req007) los persiste aparte, y el dashboard (req008) se actualiza por SignalR:

```bash
docker compose logs -f simulador                  # lectura generada y envio de cada tick
# Flujo real: abrir http://localhost:5080, "Iniciar sesión" y entrar con admin@solaruy.uy
# Alternativa de desarrollo (sin pasar por Keycloak): curl -c cookies.txt http://localhost:5080/dev/login
curl -b cookies.txt http://localhost:5080/api/telemetria   # 204 antes de la primera lectura, 200 despues
```

La generacion sigue una curva solar (0 W fuera de 07:00-19:00, hora de Uruguay). El balanceador nginx con
replicas de la aplicacion y Redis para compartir la ultima lectura ya esta incorporado (req012).

### Alertas de paneles

Cuando un panel informa el estado `EnFalla` en la telemetria, el sistema abre una alerta; la cierra cuando el panel
vuelve a `Operativo` o deja de aparecer en la lectura (por ejemplo, si su nodo pasa a mantenimiento). Las alertas se
guardan en MySQL con su historial (tabla `AlertasPanel`) y no se eliminan. Se ven sin iniciar sesion:

- Pantalla de alertas: <http://localhost:5080/Alertas> (activas y las 50 cerradas mas recientes, con fechas en UTC).
- Indicador "Alertas" en el menu de todas las pantallas, con la cantidad de alertas activas: se actualiza sin recargar
  por SignalR cuando se abre o se cierra una alerta.

Para provocar fallos y verlas, se puede levantar un simulador efimero con probabilidad de falla 1 (con el servicio
`simulador` detenido, para que no pise las lecturas):

```bash
docker compose stop simulador
docker run -d --rm --name sim-falla --network solaruy_solaruy -e SimulacionApi__UrlBase=http://app:8080 -e SimulacionApi__ApiKey=SolarUY.Local2026 -e Simulador__IntervaloSegundos=2 -e Simulador__ProbabilidadFalla=1 solaruy-simulador
docker rm -f sim-falla && docker compose start simulador      # volver a lo normal
```

### Consultas con Antigravity (MCP)

La aplicacion expone un servidor **MCP** (Model Context Protocol) de **solo lectura** en
`http://127.0.0.1:5080/mcp` (req011). Antigravity lo usa como herramienta y traduce las preguntas
en lenguaje natural a consultas sobre los datos de SolarUY; la aplicacion no llama a ningun LLM ni
necesita una key de Gemini.

El workspace ya trae la configuracion en [`.agents/mcp_config.json`](.agents/mcp_config.json). Para
usarla en Antigravity, agregala a la configuracion de MCP del workspace o copiala a la global
(`~/.gemini/config/mcp_config.json`):

```json
{
  "mcpServers": {
    "solaruy": {
      "serverUrl": "http://127.0.0.1:5080/mcp"
    }
  }
}
```

Herramientas disponibles (todas de consulta): `listar_parques`, `obtener_topologia`,
`obtener_estado_actual`, `obtener_historico`, `obtener_alertas` y `listar_tareas_mantenimiento`.
El endpoint es anonimo y solo se publica en loopback (`127.0.0.1`); se puede apagar con
`Mcp:Habilitado=false`.

## Metodologia de trabajo: SDD + control de calidad

El proyecto aplica **Spec-Driven Development** con artefactos de prompting por requerimiento en
`ControlCalidad/prompting/<requerimiento>/` (`requirements.md`, `specs.md`, `tasks.md`) y dos
guardarrailes automaticos:

1. **ArchUnitNET** (`ControlCalidad/ValidacionDeCodigo/CodeQuality.Tests`): verifica las reglas de Clean Architecture (dependencias hacia adentro, repositorios en Infraestructura, naming, dominio sin EF).
2. **SonarQube** (`ControlCalidad/SonarQube`): analisis estatico de codigo (code smells, duplicacion, cobertura).

Detalle de uso: [ControlCalidad/README.md](ControlCalidad/README.md).
