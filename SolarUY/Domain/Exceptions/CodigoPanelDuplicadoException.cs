using System;

namespace SolarUY.Domain.Exceptions;

public class CodigoPanelDuplicadoException : Exception
{
    public CodigoPanelDuplicadoException()
        : base("Ya existe un panel con ese código/serie en el nodo.")
    {
    }

    public CodigoPanelDuplicadoException(string message)
        : base(message)
    {
    }

    public CodigoPanelDuplicadoException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
