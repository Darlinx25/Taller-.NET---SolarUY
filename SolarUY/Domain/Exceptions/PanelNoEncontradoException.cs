using System;

namespace SolarUY.Domain.Exceptions;

public class PanelNoEncontradoException : Exception
{
    public PanelNoEncontradoException(Guid id)
        : base($"No existe un panel registrado con el identificador '{id}'.")
    {
        Id = id;
    }

    public Guid Id { get; }
}
