using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SolarUY.Application.Dtos;

namespace SolarUY.Application.Services;

/// <summary>
/// Guarda la orden vigente para el simulador y avisa al dashboard para que se actualice.
/// El simulador la consulta en cada ciclo; la aplicación nunca lo llama.
/// </summary>
public class ControlSimulacionService : IControlSimulacionService
{
    private readonly IAlmacenControlSimulacion _almacen;
    private readonly ITelemetriaNotifier? _notificador;
    private readonly TimeProvider _tiempo;
    private readonly ILogger<ControlSimulacionService> _logger;

    public ControlSimulacionService(
        IAlmacenControlSimulacion almacen,
        TimeProvider tiempo,
        ILogger<ControlSimulacionService> logger,
        ITelemetriaNotifier? notificador = null)
    {
        _almacen = almacen ?? throw new ArgumentNullException(nameof(almacen));
        _tiempo = tiempo ?? throw new ArgumentNullException(nameof(tiempo));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificador = notificador;
    }

    public Task<ControlSimulacionDto> ObtenerAsync(CancellationToken cancellationToken = default)
    {
        return _almacen.ObtenerAsync(cancellationToken);
    }

    public async Task<ControlSimulacionDto> EstablecerSimulacionForzadaAsync(
        bool activa,
        CancellationToken cancellationToken = default)
    {
        var control = await _almacen.ObtenerAsync(cancellationToken).ConfigureAwait(false);
        control.SimulacionForzada = activa;

        await _almacen.GuardarAsync(control, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Día forzado {Estado} desde el dashboard.", activa ? "activado" : "desactivado");

        await NotificarAsync(cancellationToken).ConfigureAwait(false);
        return control;
    }

    public async Task<ControlSimulacionDto> SolicitarFallaAsync(CancellationToken cancellationToken = default)
    {
        var control = await _almacen.ObtenerAsync(cancellationToken).ConfigureAwait(false);

        // Los ticks de tiempo mantienen la secuencia creciente aunque la caché se pierda al reiniciar.
        control.SecuenciaFalla = Math.Max(control.SecuenciaFalla + 1, _tiempo.GetUtcNow().UtcTicks);

        await _almacen.GuardarAsync(control, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Falla de paneles solicitada desde el dashboard (secuencia {Secuencia}).", control.SecuenciaFalla);

        await NotificarAsync(cancellationToken).ConfigureAwait(false);
        return control;
    }

    private async Task NotificarAsync(CancellationToken cancellationToken)
    {
        if (_notificador is null)
            return;

        try
        {
            await _notificador.NotificarLecturaActualizadaAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // La orden ya quedó guardada: un fallo de SignalR no debe hacer fallar el pedido.
            _logger.LogWarning(ex, "No se pudo notificar al dashboard el cambio de la orden de simulación.");
        }
    }
}
