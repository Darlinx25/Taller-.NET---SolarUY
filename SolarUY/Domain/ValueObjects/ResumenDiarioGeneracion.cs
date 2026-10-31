using System;
using SolarUY.Domain.Enums;

namespace SolarUY.Domain.ValueObjects;

/// <summary>
/// Proyección inmutable de un resumen diario de generación.
/// </summary>
public sealed class ResumenDiarioGeneracion
{
    public ResumenDiarioGeneracion(
        Guid entidadId,
        string nombreEntidad,
        DateOnly fechaLocal,
        decimal potenciaAcumuladaW,
        int cantidadLecturas,
        decimal energiaKWh,
        decimal potenciaMaximaW,
        int fallas,
        EstadoPanel ultimoEstado)
    {
        if (entidadId == Guid.Empty)
            throw new ArgumentException("El identificador de la entidad es obligatorio.", nameof(entidadId));

        if (string.IsNullOrWhiteSpace(nombreEntidad))
            throw new ArgumentException("El nombre de la entidad es obligatorio.", nameof(nombreEntidad));

        if (fechaLocal == default)
            throw new ArgumentException("La fecha local es obligatoria.", nameof(fechaLocal));

        if (potenciaAcumuladaW < 0)
            throw new ArgumentOutOfRangeException(
                nameof(potenciaAcumuladaW),
                "La potencia acumulada no puede ser negativa.");

        if (cantidadLecturas < 0)
            throw new ArgumentOutOfRangeException(
                nameof(cantidadLecturas),
                "La cantidad de lecturas no puede ser negativa.");

        if (energiaKWh < 0)
            throw new ArgumentOutOfRangeException(nameof(energiaKWh), "La energía no puede ser negativa.");

        if (potenciaMaximaW < 0)
            throw new ArgumentOutOfRangeException(
                nameof(potenciaMaximaW),
                "La potencia máxima no puede ser negativa.");

        if (fallas < 0)
            throw new ArgumentOutOfRangeException(nameof(fallas), "La cantidad de fallas no puede ser negativa.");

        if (cantidadLecturas > 0 && potenciaMaximaW > potenciaAcumuladaW)
            throw new ArgumentOutOfRangeException(
                nameof(potenciaMaximaW),
                "La potencia máxima no puede superar la potencia acumulada.");

        if (!Enum.IsDefined(ultimoEstado))
            throw new ArgumentOutOfRangeException(nameof(ultimoEstado), "El último estado no es válido.");

        EntidadId = entidadId;
        NombreEntidad = nombreEntidad.Trim();
        FechaLocal = fechaLocal;
        PotenciaAcumuladaW = potenciaAcumuladaW;
        CantidadLecturas = cantidadLecturas;
        EnergiaKWh = energiaKWh;
        PotenciaMaximaW = potenciaMaximaW;
        Fallas = fallas;
        UltimoEstado = ultimoEstado;
    }

    public Guid EntidadId { get; }

    public string NombreEntidad { get; }

    public DateOnly FechaLocal { get; }

    public decimal PotenciaAcumuladaW { get; }

    public int CantidadLecturas { get; }

    public decimal EnergiaKWh { get; }

    public decimal PotenciaMaximaW { get; }

    public int Fallas { get; }

    public EstadoPanel UltimoEstado { get; }

    public decimal PotenciaPromedioW => CantidadLecturas == 0
        ? 0m
        : PotenciaAcumuladaW / CantidadLecturas;
}
