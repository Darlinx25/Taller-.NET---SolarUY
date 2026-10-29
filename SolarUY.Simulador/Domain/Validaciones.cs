namespace SolarUY.Simulador.Domain;

/// <summary>
/// Validaciones comunes del Dominio del simulador.
/// </summary>
internal static class Validaciones
{
    public static void ExigirIdentificador(Guid id, string parametro, string mensaje)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(mensaje, parametro);
    }

    public static string NormalizarTextoObligatorio(string? texto, string parametro, string mensaje)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new ArgumentException(mensaje, parametro);

        return texto.Trim();
    }

    public static void ExigirFechaHora(DateTime fechaHora, string parametro)
    {
        if (fechaHora == default)
            throw new ArgumentException("La fecha y hora de la lectura son obligatorias.", parametro);
    }

    public static void ExigirNoNegativo(decimal valor, string parametro, string mensaje)
    {
        if (valor < 0)
            throw new ArgumentOutOfRangeException(parametro, mensaje);
    }
}
