namespace SolarUY.Application.Services;

/// <summary>
/// Valida la API key con la que se autentica el simulador.
/// </summary>
public interface IValidadorApiKey
{
    bool EsValida(string? apiKey);
}
