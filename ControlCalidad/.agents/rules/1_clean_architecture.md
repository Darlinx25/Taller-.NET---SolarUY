# Reglas de Arquitectura y Diseño (SolarUY - ASP.NET Core MVC monolítico en capas)

SolarUY es una aplicación **ASP.NET Core MVC monolítica**, pero organizada internamente **por capas** (carpetas + namespaces). La arquitectura es un monolito desplegable como una sola unidad, con separación lógica de responsabilidades. Al generar, refactorizar o validar código, el agente debe asegurar que se cumplan las siguientes reglas (validadas automáticamente con ArchUnitNET).

## 0. Estructura de carpetas y namespaces del monolito

Un único proyecto web (`SolarUY`), con carpetas y namespaces que representan las capas:

| Capa | Carpeta | Namespace | Contenido |
|---|---|---|---|
| Dominio | `Domain/` | `SolarUY.Domain` | Entidades y reglas de negocio puras |
| Aplicación | `Application/` | `SolarUY.Application` | Servicios / casos de uso, DTOs, validadores |
| Infraestructura | `Infrastructure/` | `SolarUY.Infrastructure` | EF Core (`DbContext`), repositorios, integraciones externas |
| Presentación | `Controllers/`, `Views/`, `Models/`, `Hubs/` | `SolarUY.Controllers`, `SolarUY.Models`, `SolarUY.Hubs` | Controladores MVC, vistas Razor, ViewModels, hubs de SignalR |

> Es un **monolito**: no se crean proyectos `.csproj` separados por capa. La separación es por carpetas y namespaces dentro de `SolarUY`.

## 1. Reglas de Dependencia de Capas (las dependencias apuntan hacia adentro)
- **Aislamiento del Dominio:** La carpeta `Domain` es el núcleo. **NUNCA** debe importar Entity Framework Core, ASP.NET Core, SignalR, ni referenciar `Application`, `Infrastructure` o `Presentation`. Solo usa C# puro.
- **Aislamiento de la Aplicación:** `Application` (servicios / casos de uso) solo puede depender de `Domain`. No puede conocer detalles de ASP.NET MVC ni de acceso a datos (nada de `DbContext`, SQL, `Controller`, etc.).
- **Protección de la Presentación:** Los Controladores MVC, ViewModels y vistas **NO DEBEN** acceder directamente a `Infrastructure` (repositorios concretos o `DbContext`). La Presentación solo se comunica con `Application` (servicios).
- **La Infraestructura es un detalle:** `Infrastructure` (base de datos, integraciones, mock del inversor) implementa las interfaces definidas en `Domain` o `Application`. Ninguna otra capa debe depender de `Infrastructure` directamente (la inyección se configura solo al arrancar la app, en `Program.cs`).

## 2. Reglas de Dominio y Datos
- **Flexibilidad en Entidades:** Se permite el uso de `setters` públicos (`public set`) en las entidades del Dominio para facilitar el aprendizaje con Entity Framework Core. Sin embargo, si una actualización de datos requiere validaciones complejas, el agente debe proponer encapsularla en un método de la entidad en lugar de dispersar la lógica.
- **Uso de Clases Anémicas Permitido Inicialmente:** Está permitido que las entidades funcionen principalmente como contenedores de datos en etapas tempranas.

## 3. Reglas de Repositorios y Abstracciones
- **Inversión de Dependencias (DIP):** Todo acceso a la base de datos se hace mediante interfaces. Las interfaces de los repositorios (ej. `IParqueSolarRepository`) se definen idealmente en `Domain` o `Application`, pero su implementación concreta (ej. `ParqueSolarRepository`) va **obligatoriamente** en `Infrastructure`.
- **Convención de Nombres:** Las clases que implementan acceso a base de datos deben terminar con el sufijo `Repository`. Las interfaces deben comenzar siempre con la letra `I`.

## 4. Inyección de Dependencias
- **Inyección por Constructor:** Las dependencias en los Servicios de Aplicación, Repositorios y Controladores deben inyectarse EXCLUSIVAMENTE a través del constructor. No instanciar clases complejas con `new` si corresponden a servicios, repositorios o componentes de infraestructura.
- **Registro centralizado:** El registro de servicios (`builder.Services.Add...`) se realiza en `Program.cs`, el único lugar que conoce las implementaciones concretas de `Infrastructure`.
