using System;

namespace SolarUY.Domain.Exceptions;

/// <summary>
/// Se lanza cuando los criterios de consulta del histórico no son válidos.
/// </summary>
public class FiltroHistoricoInvalidoException : Exception
{
    public FiltroHistoricoInvalidoException()
        : base("El filtro del histórico no es válido.")
    {
    }

    public FiltroHistoricoInvalidoException(string message)
        : base(message)
    {
    }

    public FiltroHistoricoInvalidoException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
