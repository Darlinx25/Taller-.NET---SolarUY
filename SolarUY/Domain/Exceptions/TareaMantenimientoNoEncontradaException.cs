using System;

namespace SolarUY.Domain.Exceptions;

public class TareaMantenimientoNoEncontradaException : Exception
{
    public Guid Id { get; }

    public TareaMantenimientoNoEncontradaException(Guid id)
        : base($"No se encontró la tarea de mantenimiento con id '{id}'.")
    {
        Id = id;
    }
}
