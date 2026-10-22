using System;

namespace SolarUY.Domain.Exceptions;

/// <summary>
/// Excepción de dominio que se lanza cuando se intenta registrar un parque solar
/// con un nombre que ya existe en el sistema.
/// </summary>
public class NombreParqueDuplicadoException : Exception
{
    /// <summary>
    /// Crea la excepción para el nombre duplicado indicado.
    /// </summary>
    /// <param name="nombre">Nombre del parque solar que ya existe.</param>
    public NombreParqueDuplicadoException(string nombre)
        : base($"Ya existe un parque solar registrado con el nombre '{nombre}'.")
    {
        Nombre = nombre;
    }

    /// <summary>
    /// Nombre del parque solar que ya está registrado.
    /// </summary>
    public string Nombre { get; }
}
