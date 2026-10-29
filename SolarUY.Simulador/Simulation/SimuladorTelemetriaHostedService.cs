using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolarUY.Simulador.Contracts;
using SolarUY.Simulador.Transport;

namespace SolarUY.Simulador.Simulation;

/// <summary>
/// Ejecuta el ciclo del simulador: un tick inmediato y luego uno por intervalo. Cada tick genera la lectura
/// y la envía a la aplicación con reintentos. Ningún fallo detiene el proceso.
/// </summary>
public class SimuladorTelemetriaHostedService : BackgroundService
{
    private readonly SimuladorInversor _simulador;
    private readonly ClienteTelemetria _clienteTelemetria;
    private readonly ConfiguracionSimulador _configuracion;
    private readonly ILogger<SimuladorTelemetriaHostedService> _logger;

    public SimuladorTelemetriaHostedService(
        SimuladorInversor simulador,
        ClienteTelemetria clienteTelemetria,
        IOptions<ConfiguracionSimulador> configuracion,
        ILogger<SimuladorTelemetriaHostedService> logger)
    {
        _simulador = simulador ?? throw new ArgumentNullException(nameof(simulador));
        _clienteTelemetria = clienteTelemetria ?? throw new ArgumentNullException(nameof(clienteTelemetria));
        ArgumentNullException.ThrowIfNull(configuracion);
        _configuracion = configuracion.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Simulador iniciado: un tick cada {IntervaloSegundos} s.",
            _configuracion.IntervaloSegundos);

        try
        {
            await EjecutarTickSeguroAsync(stoppingToken).ConfigureAwait(false);

            using var temporizador = new PeriodicTimer(TimeSpan.FromSeconds(_configuracion.IntervaloSegundos));
            while (await temporizador.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                await EjecutarTickSeguroAsync(stoppingToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Simulador detenido.");
        }
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
