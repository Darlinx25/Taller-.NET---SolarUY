using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SolarUY.Domain.Entities;

namespace SolarUY.Domain.Repositories;

/// <summary>
/// Contrato de persistencia de las alertas de paneles.
/// La implementación concreta vive en la capa de Infraestructura.
/// </summary>
public interface IAlertaPanelRepository
{
    /// <summary>
    /// Alertas activas (sin fecha de fin), trackeadas, de la más reciente a la más antigua por inicio.
    /// </summary>
    Task<IReadOnlyList<AlertaPanel>> ObtenerActivasAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Las <paramref name="cantidad"/> alertas cerradas más recientes por fecha de fin.
    /// </summary>
    Task<IReadOnlyList<AlertaPanel>> ObtenerCerradasRecientesAsync(
        int cantidad,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// La alerta más reciente (activa o cerrada) de un panel, o <c>null</c> si nunca tuvo.
    /// </summary>
    Task<AlertaPanel?> ObtenerUltimaPorPanelAsync(Guid panelId, CancellationToken cancellationToken = default);

    Task<int> ContarActivasAsync(CancellationToken cancellationToken = default);

    Task AgregarAsync(AlertaPanel alerta, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
