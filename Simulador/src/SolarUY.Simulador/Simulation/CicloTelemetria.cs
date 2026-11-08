using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolarUY.Simulador.Contracts;
using SolarUY.Simulador.Transport;

namespace SolarUY.Simulador.Simulation;

/// <summary>
/// Ejecuta el ciclo del simulador: un tick inmediato y luego uno por intervalo. Cada tick genera la lectura
/// y la envía a la aplicación con reintentos. Ningún fallo detiene el ciclo.
/// </summary>
public class CicloTelemetria
{
    private readonly SimuladorInversor _simulador;
    private readonly ClienteTelemetria _clienteTelemetria;
    private readonly ConfiguracionSimulador _configuracion;
    private readonly ILogger<CicloTelemetria> _logger;

    public CicloTelemetria(
        SimuladorInversor simulador,
        ClienteTelemetria clienteTelemetria,
        IOptions<ConfiguracionSimulador> configuracion,
        ILogger<CicloTelemetria> logger)
    {
        _simulador = simulador ?? throw new ArgumentNullException(nameof(simulador));
        _clienteTelemetria = clienteTelemetria ?? throw new ArgumentNullException(nameof(clienteTelemetria));
        ArgumentNullException.ThrowIfNull(configuracion);
        _configuracion = configuracion.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Ejecuta ticks hasta que se cancele el token. La cancelación termina el ciclo sin lanzar.
    /// </summary>
    public async Task EjecutarAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Simulador iniciado: un tick cada {IntervaloSegundos} s.",
            _configuracion.IntervaloSegundos);

        try
        {
            await EjecutarTickSeguroAsync(cancellationToken).ConfigureAwait(false);

            // Task.Delay y no PeriodicTimer: en WebAssembly (un solo hilo) es la espera con soporte probado.
            var intervalo = TimeSpan.FromSeconds(_configuracion.IntervaloSegundos);
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(intervalo, cancellationToken).ConfigureAwait(false);
                await EjecutarTickSeguroAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // La cancelación es el modo normal de detener el ciclo.
        }

        _logger.LogInformation("Simulador detenido.");
    }

    private async Task EjecutarTickSeguroAsync(CancellationToken cancellationToken)
    {
        try
        {
            await EjecutarTickAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Error inesperado en el tick de simulación; se continúa con el siguiente.");
        }
    }

    private async Task EjecutarTickAsync(CancellationToken cancellationToken)
    {
        var lecturas = await _simulador.GenerarLecturaAsync(cancellationToken).ConfigureAwait(false);
        if (lecturas is null)
        {
            _logger.LogWarning("Topología no disponible: se omite el tick y se conserva la última lectura.");
            return;
        }

        var telemetria = TelemetriaMapper.Mapear(lecturas);
        RegistrarLecturaGenerada(telemetria);

        await EnviarConReintentosAsync(telemetria, cancellationToken).ConfigureAwait(false);
    }

    private void RegistrarLecturaGenerada(TelemetriaDto telemetria)
    {
        var nodos = telemetria.Parques.Sum(parque => parque.Nodos.Count);
        var paneles = telemetria.Parques.Sum(parque => parque.Nodos.Sum(nodo => nodo.Paneles.Count));
        var potenciaW = telemetria.Parques.Sum(parque => parque.PotenciaGeneradaW);

        _logger.LogInformation(
            "Lectura generada {FechaHora:o}: {Parques} parque(s), {Nodos} nodo(s), {Paneles} panel(es), {PotenciaW} W.",
            telemetria.FechaHora,
            telemetria.Parques.Count,
            nodos,
            paneles,
            potenciaW);
    }

    private async Task EnviarConReintentosAsync(TelemetriaDto telemetria, CancellationToken cancellationToken)
    {
        for (var intento = 1; intento <= _configuracion.IntentosEnvio; intento++)
        {
            if (await _clienteTelemetria.EnviarAsync(telemetria, cancellationToken).ConfigureAwait(false))
            {
                _logger.LogInformation("Lectura {FechaHora:o} enviada a la aplicación.", telemetria.FechaHora);
                return;
            }

            if (intento < _configuracion.IntentosEnvio)
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(_configuracion.EsperaEntreIntentosSegundos),
                    cancellationToken).ConfigureAwait(false);
            }
        }

        _logger.LogError(
            "No se pudo enviar la lectura {FechaHora:o} tras {Intentos} intento(s); se descarta.",
            telemetria.FechaHora,
            _configuracion.IntentosEnvio);
    }
}
