using System;

namespace SolarUY.Domain.Exceptions;

public class TareaMantenimientoEstadoInvalidoException : Exception
{
    public TareaMantenimientoEstadoInvalidoException(string estado)
        : base($"El estado '{estado}' no es válido para una tarea de mantenimiento.")
    {
    }
}
