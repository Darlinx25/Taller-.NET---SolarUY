using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SolarUY.Infrastructure.Persistence.Converters;

/// <summary>
/// Adapta DateOnly al proveedor de MySQL, que materializa las columnas date como DateTime.
/// </summary>
public sealed class DateOnlyToDateTimeConverter : ValueConverter<DateOnly, DateTime>
{
    public DateOnlyToDateTimeConverter()
        : base(
            fecha => fecha.ToDateTime(TimeOnly.MinValue),
            fecha => DateOnly.FromDateTime(fecha))
    {
    }
}
