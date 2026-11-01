using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SolarUY.Domain.Entities;
using SolarUY.Domain.Repositories;
using SolarUY.Infrastructure.Persistence;

namespace SolarUY.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de alertas de paneles sobre Entity Framework Core.
/// </summary>
public class AlertaPanelRepository : IAlertaPanelRepository
{
    private readonly SolarUYDbContext _contexto;

    public AlertaPanelRepository(SolarUYDbContext contexto)
    {
        _contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
    }

    public async Task<IReadOnlyList<AlertaPanel>> ObtenerActivasAsync(CancellationToken cancellationToken = default)
    {
        return await _contexto.AlertasPanel
            .Where(alerta => alerta.FechaFinUtc == null)
            .OrderByDescending(alerta => alerta.FechaInicioUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<AlertaPanel>> ObtenerCerradasRecientesAsync(
        int cantidad,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cantidad);

        return await _contexto.AlertasPanel
            .AsNoTracking()
            .Where(alerta => alerta.FechaFinUtc != null)
            .OrderByDescending(alerta => alerta.FechaFinUtc)
            .Take(cantidad)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<AlertaPanel?> ObtenerUltimaPorPanelAsync(
        Guid panelId,
        CancellationToken cancellationToken = default)
    {
        return _contexto.AlertasPanel
            .AsNoTracking()
            .Where(alerta => alerta.PanelId == panelId)
            .OrderByDescending(alerta => alerta.FechaInicioUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<int> ContarActivasAsync(CancellationToken cancellationToken = default)
    {
        return _contexto.AlertasPanel.CountAsync(alerta => alerta.FechaFinUtc == null, cancellationToken);
    }

    public async Task AgregarAsync(AlertaPanel alerta, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(alerta);
        await _contexto.AlertasPanel.AddAsync(alerta, cancellationToken).ConfigureAwait(false);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _contexto.SaveChangesAsync(cancellationToken);
    }
}
