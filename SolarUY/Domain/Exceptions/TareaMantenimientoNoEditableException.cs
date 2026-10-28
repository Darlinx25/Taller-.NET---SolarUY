using System;

namespace SolarUY.Domain.Exceptions;

public class TareaMantenimientoNoEditableException : Exception
{
    public Guid Id { get; }

    public TareaMantenimientoNoEditableException(Guid id)
        : base($"No se puede editar la tarea de mantenimiento con id '{id}' porque se encuentra en estado Completada o Cancelada.")
    {
        Id = id;
    }
}
