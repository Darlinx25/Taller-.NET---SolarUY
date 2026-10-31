using System.Threading;
using System.Threading.Tasks;
using SolarUY.Application.Dtos;

namespace SolarUY.Application.Services;

public interface IDashboardService
{
    Task<DashboardTelemetriaViewDto> ObtenerDashboardAsync(CancellationToken cancellationToken = default);
}
