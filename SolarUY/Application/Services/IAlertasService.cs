using System.Threading;
using System.Threading.Tasks;
using SolarUY.Application.Dtos;

namespace SolarUY.Application.Services;

public interface IAlertasService
{
    Task<ListadoAlertasDto> ObtenerListadoAsync(CancellationToken cancellationToken = default);

    Task<int> ContarActivasAsync(CancellationToken cancellationToken = default);
}
