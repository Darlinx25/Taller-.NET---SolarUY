using System.Threading;
using System.Threading.Tasks;
using SolarUY.Application.Dtos;

namespace SolarUY.Application.Services;

public interface ITopologiaSimulacionService
{
    Task<TopologiaSimulacionDto> ObtenerAsync(CancellationToken cancellationToken = default);
}
