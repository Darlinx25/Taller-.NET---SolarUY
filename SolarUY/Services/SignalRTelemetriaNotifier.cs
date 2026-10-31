using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SolarUY.Application.Services;
using SolarUY.Hubs;

namespace SolarUY.Services;

public class SignalRTelemetriaNotifier : ITelemetriaNotifier
{
    private readonly IHubContext<TelemetriaHub> _hubContext;

    public SignalRTelemetriaNotifier(IHubContext<TelemetriaHub> hubContext)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
    }

    public Task NotificarLecturaActualizadaAsync(CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.All.SendAsync("ActualizarTelemetria", cancellationToken);
    }
}
