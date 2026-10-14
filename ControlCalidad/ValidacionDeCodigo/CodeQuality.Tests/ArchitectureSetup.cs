using ArchUnitNET.Domain;
using ArchUnitNET.Loader;

namespace CodeQuality.Tests;

public static class ArchitectureSetup
{
    // =========================================================================
    // ¿CÓMO SE CONECTAN LOS PROYECTOS A ESTOS TESTS?
    // =========================================================================
    // ArchUnitNET no lee carpetas: lee ensamblados compilados (.dll).
    // SolarUY es un monolito: hay UN solo proyecto (SolarUY.csproj) que contiene
    // TODAS las capas como carpetas + namespaces (Domain, Application,
    // Infrastructure y la Presentación MVC en Controllers/Models/Hubs).
    //
    // Por eso cargamos un único ensamblado: el de SolarUY. Esa única referencia
    // ya nos entrega las cuatro capas, porque las capas son namespaces dentro
    // del mismo ensamblado.
    //
    // Para que el ensamblado esté disponible, este proyecto de tests incluye una
    // "Referencia de Proyecto" hacia SolarUY.csproj (ver CodeQuality.Tests.csproj).
    // Al pasarle el tipo (typeof) de una clase cualquiera de SolarUY, ArchUnitNET
    // recibe el ensamblado entero y puede analizar todas sus capas.

    public static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(SolarUY.Controllers.HomeController).Assembly
        )
        .Build();

    // Constantes para identificar las capas por Namespace.
    // En el monolito en capas los namespaces siguen la convención:
    //   SolarUY.Domain, SolarUY.Application, SolarUY.Infrastructure
    //   y la Presentación en SolarUY.Controllers / SolarUY.Models / SolarUY.Hubs.
    public const string DomainNamespace = ".*Domain.*";
    public const string ApplicationNamespace = ".*Application.*";
    public const string InfrastructureNamespace = ".*Infrastructure.*";
    public const string PresentationNamespace = ".*Presentation.*|.*Controllers.*|.*Models.*|.*Hubs.*|.*Views.*";
}
