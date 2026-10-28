using System;

namespace SolarUY.Domain.Exceptions;

/// <summary>
/// Excepción de dominio que se lanza cuando no se encuentra un nodo.
/// </summary>
public class NodoNoEncontradoException : Exception
{
    public NodoNoEncontradoException()
        : base("No se encontró el nodo solicitado.")
    {
    }

    public NodoNoEncontradoException(string message)
        : base(message)
    {
    }

    public NodoNoEncontradoException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
