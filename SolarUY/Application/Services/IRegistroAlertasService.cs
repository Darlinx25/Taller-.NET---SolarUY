using System.Threading;
using System.Threading.Tasks;
using SolarUY.Application.Dtos;

namespace SolarUY.Application.Services;

public interface IRegistroAlertasService
{
    /// <summary>
    /// Abre y cierra alertas de paneles a partir de una lectura de telemetría ya validada.
    /// </summary>
    Task RegistrarLecturaAsync(TelemetriaDto lectura, CancellationToken cancellationToken = default);
}
