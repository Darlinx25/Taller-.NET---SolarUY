namespace SolarUY.Simulador.Transport;

/// <summary>
/// Utilidades comunes de los clientes HTTP.
/// </summary>
internal static class RespuestaHttp
{
    private const int LongitudMaximaFragmento = 200;

    /// <summary>
    /// Devuelve un fragmento del cuerpo de una respuesta no exitosa, para registrarlo en el log.
    /// </summary>
    public static async Task<string> LeerFragmentoAsync(HttpResponseMessage respuesta, CancellationToken cancellationToken)
    {
        var cuerpo = await respuesta.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        return cuerpo.Length <= LongitudMaximaFragmento
            ? cuerpo
            : cuerpo[..LongitudMaximaFragmento];
    }
}
