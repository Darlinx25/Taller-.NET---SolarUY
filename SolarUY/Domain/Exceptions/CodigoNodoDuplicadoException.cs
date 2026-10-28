using System;

namespace SolarUY.Domain.Exceptions;

/// <summary>
/// Excepción de dominio que se lanza cuando ya existe un nodo con el mismo código en el parque solar.
/// </summary>
public class CodigoNodoDuplicadoException : Exception
{
    public CodigoNodoDuplicadoException()
        : base("Ya existe un nodo con ese código en el parque solar.")
    {
    }

    public CodigoNodoDuplicadoException(string message)
        : base(message)
    {
    }

    public CodigoNodoDuplicadoException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
