using System.Threading;
using System.Threading.Tasks;

namespace SolarUY.Application.Services;

public interface ITelemetriaNotifier
{
    Task NotificarLecturaActualizadaAsync(CancellationToken cancellationToken = default);
}
