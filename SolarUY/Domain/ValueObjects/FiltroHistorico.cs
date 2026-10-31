using System;
using SolarUY.Domain.Enums;
using SolarUY.Domain.Exceptions;

namespace SolarUY.Domain.ValueObjects;

/// <summary>
/// Criterios validados para consultar los resúmenes diarios de generación.
/// </summary>
public sealed class FiltroHistorico
{
    private FiltroHistorico(
        TipoEntidadHistorico tipoEntidad,
        Guid? entidadId,
        DateOnly desde,
        DateOnly hasta,
        AgrupacionHistorico agrupacion)
    {
        TipoEntidad = tipoEntidad;
        EntidadId = entidadId;
        Desde = desde;
        Hasta = hasta;
        Agrupacion = agrupacion;
    }

    public TipoEntidadHistorico TipoEntidad { get; }

    public Guid? EntidadId { get; }

    public DateOnly Desde { get; }

    public DateOnly Hasta { get; }

    public AgrupacionHistorico Agrupacion { get; }

    public static FiltroHistorico Crear(
        TipoEntidadHistorico tipoEntidad,
        Guid? entidadId,
        DateOnly desde,
        DateOnly hasta,
        AgrupacionHistorico agrupacion)
    {
        if (!Enum.IsDefined(tipoEntidad))
            throw new FiltroHistoricoInvalidoException("El tipo de entidad del histórico no es válido.");

        if (!Enum.IsDefined(agrupacion))
            throw new FiltroHistoricoInvalidoException("La agrupación del histórico no es válida.");

        if (desde == default)
            throw new FiltroHistoricoInvalidoException("La fecha desde es obligatoria.");

        if (hasta == default)
            throw new FiltroHistoricoInvalidoException("La fecha hasta es obligatoria.");

        if (desde > hasta)
            throw new FiltroHistoricoInvalidoException("La fecha desde no puede ser posterior a la fecha hasta.");

        if (entidadId == Guid.Empty)
            throw new FiltroHistoricoInvalidoException("La entidad seleccionada no es válida.");

        return new FiltroHistorico(tipoEntidad, entidadId, desde, hasta, agrupacion);
    }
}
