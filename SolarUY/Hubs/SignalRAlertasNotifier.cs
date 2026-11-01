using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SolarUY.Application.Services;

namespace SolarUY.Hubs;

/// <summary>
/// Emite por SignalR el evento <c>ActualizarAlertas</c> cuando se abre o se cierra alguna alerta.
/// </summary>
public class SignalRAlertasNotifier : IAlertasNotifier
{
    private readonly IHubContext<TelemetriaHub> _hubContext;

    public SignalRAlertasNotifier(IHubContext<TelemetriaHub> hubContext)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
    }

    public Task NotificarAlertasActualizadasAsync(CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.All.SendAsync("ActualizarAlertas", cancellationToken);
    }
}
