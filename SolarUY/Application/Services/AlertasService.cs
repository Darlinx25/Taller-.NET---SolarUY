using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SolarUY.Application.Dtos;
using SolarUY.Domain.Entities;
using SolarUY.Domain.Repositories;

namespace SolarUY.Application.Services;

/// <summary>
/// Consulta de solo lectura de las alertas de paneles: activas e historial reciente.
/// </summary>
public class AlertasService : IAlertasService
{
    private const int CantidadHistorial = 50;

    private readonly IAlertaPanelRepository _alertaPanelRepository;
    private readonly TimeProvider _tiempo;

    public AlertasService(IAlertaPanelRepository alertaPanelRepository, TimeProvider tiempo)
    {
        _alertaPanelRepository = alertaPanelRepository
            ?? throw new ArgumentNullException(nameof(alertaPanelRepository));
        _tiempo = tiempo ?? throw new ArgumentNullException(nameof(tiempo));
    }

    public async Task<ListadoAlertasDto> ObtenerListadoAsync(CancellationToken cancellationToken = default)
    {
        var activas = await _alertaPanelRepository
            .ObtenerActivasAsync(cancellationToken)
            .ConfigureAwait(false);
        var cerradas = await _alertaPanelRepository
            .ObtenerCerradasRecientesAsync(CantidadHistorial, cancellationToken)
            .ConfigureAwait(false);

        var ahoraUtc = _tiempo.GetUtcNow().UtcDateTime;

        return new ListadoAlertasDto
        {
            Activas = activas.Select(alerta => Mapear(alerta, ahoraUtc)).ToList(),
            Historial = cerradas.Select(alerta => Mapear(alerta, ahoraUtc)).ToList()
        };
    }

    public Task<int> ContarActivasAsync(CancellationToken cancellationToken = default)
    {
        return _alertaPanelRepository.ContarActivasAsync(cancellationToken);
    }

    private static AlertaPanelDto Mapear(AlertaPanel alerta, DateTime ahoraUtc)
    {
        return new AlertaPanelDto
        {
            Id = alerta.Id,
            NombreParque = alerta.NombreParque,
            CodigoNodo = alerta.CodigoNodo,
            CodigoSerie = alerta.CodigoSerie,
            FechaInicioUtc = alerta.FechaInicioUtc,
            FechaFinUtc = alerta.FechaFinUtc,
            EstaActiva = alerta.EstaActiva,
            Duracion = alerta.CalcularDuracion(ahoraUtc)
        };
    }
}
