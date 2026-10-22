using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SolarUY.Infrastructure.Persistence;

/// <summary>
/// Fábrica de contexto de datos para las herramientas de Entity Framework Core
/// (migraciones). Permite ejecutar <c>dotnet ef</c> sin levantar la aplicación web.
/// </summary>
public class SolarUYDbContextFactory : IDesignTimeDbContextFactory<SolarUYDbContext>
{
    /// <inheritdoc />
    public SolarUYDbContext CreateDbContext(string[] args)
    {
        var directorioProyecto = BuscarDirectorioProyecto();
        var configuracion = new ConfigurationBuilder()
            .SetBasePath(directorioProyecto)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cadenaConexion = configuracion.GetConnectionString("SolarUYDb");
        if (string.IsNullOrWhiteSpace(cadenaConexion))
        {
            throw new InvalidOperationException(
                "No se encontró la cadena de conexión 'SolarUYDb' para las herramientas de Entity Framework Core.");
        }

        var opciones = new DbContextOptionsBuilder<SolarUYDbContext>()
            .UseMySQL(cadenaConexion)
            .Options;

        return new SolarUYDbContext(opciones);
    }

    private static string BuscarDirectorioProyecto()
    {
        var directorio = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directorio is not null)
        {
            if (File.Exists(Path.Combine(directorio.FullName, "SolarUY.csproj")))
            {
                return directorio.FullName;
            }

            directorio = directorio.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
