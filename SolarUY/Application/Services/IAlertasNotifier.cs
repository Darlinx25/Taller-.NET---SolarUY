using System.Threading;
using System.Threading.Tasks;

namespace SolarUY.Application.Services;

/// <summary>
/// Avisa a los clientes en vivo que se abrió o se cerró alguna alerta.
/// La implementación concreta vive en la capa de Presentación.
/// </summary>
public interface IAlertasNotifier
{
    Task NotificarAlertasActualizadasAsync(CancellationToken cancellationToken = default);
}
