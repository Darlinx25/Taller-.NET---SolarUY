using System;

namespace SolarUY.Domain.Exceptions;

/// <summary>
/// Excepción de dominio que se lanza cuando no existe un parque solar con el identificador indicado.
/// </summary>
public class ParqueSolarNoEncontradoException : Exception
{
    /// <summary>
    /// Crea la excepción para el identificador de parque solar no encontrado.
    /// </summary>
    /// <param name="id">Identificador del parque solar buscado.</param>
    public ParqueSolarNoEncontradoException(Guid id)
        : base($"No existe un parque solar registrado con el identificador '{id}'.")
    {
        Id = id;
    }

    /// <summary>
    /// Identificador del parque solar que no fue encontrado.
    /// </summary>
    public Guid Id { get; }
}
