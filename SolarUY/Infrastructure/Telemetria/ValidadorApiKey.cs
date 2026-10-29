using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using SolarUY.Application.Services;

namespace SolarUY.Infrastructure.Telemetria;

/// <summary>
/// Compara la API key recibida con la configurada en tiempo constante y sin registrarla.
/// </summary>
public class ValidadorApiKey : IValidadorApiKey
{
    private readonly byte[] _apiKeyConfigurada;

    public ValidadorApiKey(IOptions<ConfiguracionApiSimulacion> opciones)
    {
        ArgumentNullException.ThrowIfNull(opciones);

        _apiKeyConfigurada = Encoding.UTF8.GetBytes(opciones.Value.ApiKey ?? string.Empty);
    }

    public bool EsValida(string? apiKey)
    {
        if (_apiKeyConfigurada.Length == 0 || string.IsNullOrEmpty(apiKey))
            return false;

        var recibida = Encoding.UTF8.GetBytes(apiKey);
        return CryptographicOperations.FixedTimeEquals(_apiKeyConfigurada, recibida);
    }
}
