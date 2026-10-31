using System;

namespace SolarUY.Domain.ValueObjects;

/// <summary>
/// Calcula la energía generada entre dos lecturas consecutivas de una misma entidad.
/// </summary>
public static class CalculadoraEnergia
{
    public static decimal CalcularEnergiaKWh(
        decimal potenciaW,
        DateTime fechaHoraUtc,
        DateTime? fechaHoraUltimaLecturaUtc,
        int brechaMaximaSegundos)
    {
        if (potenciaW < 0)
            throw new ArgumentOutOfRangeException(nameof(potenciaW), "La potencia no puede ser negativa.");

        if (fechaHoraUtc == default)
            throw new ArgumentException("La fecha y hora de la lectura es obligatoria.", nameof(fechaHoraUtc));

        if (fechaHoraUltimaLecturaUtc.HasValue && fechaHoraUltimaLecturaUtc.Value == default)
            throw new ArgumentException(
                "La fecha y hora de la lectura anterior es inválida.",
                nameof(fechaHoraUltimaLecturaUtc));

        if (brechaMaximaSegundos <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(brechaMaximaSegundos),
                "La brecha máxima debe ser mayor a 0.");

        if (!fechaHoraUltimaLecturaUtc.HasValue)
            return 0m;

        var intervalo = (decimal)(fechaHoraUtc - fechaHoraUltimaLecturaUtc.Value).TotalSeconds;
        if (intervalo <= 0)
            return 0m;

        var segundos = Math.Min(intervalo, brechaMaximaSegundos);
        return potenciaW / 1000m * segundos / 3600m;
    }
}
