using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SolarUY.Application.Dtos;

namespace SolarUY.Application.Services;

/// <summary>
/// Recibe, valida y conserva la última lectura de telemetría que envía el simulador,
/// y la entrega a quien la consulta.
/// </summary>
public class TelemetriaService : ITelemetriaService
{
    private readonly IAlmacenTelemetria _almacen;
    private readonly ValidadorTelemetria _validador;
    private readonly IRegistroHistoricoService _registroHistoricoService;
    private readonly ILogger<TelemetriaService> _logger;
    private readonly ITelemetriaNotifier? _notificador;

    public TelemetriaService(
        IAlmacenTelemetria almacen,
        ValidadorTelemetria validador,
        IRegistroHistoricoService registroHistoricoService,
        ILogger<TelemetriaService> logger,
        ITelemetriaNotifier? notificador = null)
    {
        _almacen = almacen ?? throw new ArgumentNullException(nameof(almacen));
        _validador = validador ?? throw new ArgumentNullException(nameof(validador));
        _registroHistoricoService = registroHistoricoService
            ?? throw new ArgumentNullException(nameof(registroHistoricoService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificador = notificador;
    }

    public Task<TelemetriaDto?> ObtenerUltimaLecturaAsync(CancellationToken cancellationToken = default)
    {
        return _almacen.ObtenerAsync(cancellationToken);
    }

    public async Task<ResultadoIngestaTelemetria> RegistrarLecturaAsync(
        TelemetriaDto? lectura,
        CancellationToken cancellationToken = default)
    {
        var errores = _validador.Validar(lectura);
        if (errores.Count > 0)
        {
            _logger.LogWarning(
                "Lectura de telemetría rechazada: {CantidadErrores} error(es). Primero: {PrimerError}",
                errores.Count,
                errores[0]);

            return new ResultadoIngestaTelemetria(false, errores);
        }

        await _registroHistoricoService
            .RegistrarLecturaAsync(lectura!, cancellationToken)
            .ConfigureAwait(false);

        await _almacen.GuardarAsync(lectura!, cancellationToken).ConfigureAwait(false);

        if (_notificador is not null)
        {
            try
            {
                await _notificador.NotificarLecturaActualizadaAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "No se pudo notificar la actualización de telemetría.");
            }
        }

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Lectura de telemetría recibida: {FechaHora:o}, {CantidadParques} parque(s).",
                lectura!.FechaHora,
                lectura.Parques.Count);
        }

        return new ResultadoIngestaTelemetria(true, Array.Empty<string>());
    }
}
