using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarUY.Application.Services;

namespace SolarUY.Controllers;

/// <summary>
/// Consulta de solo lectura de la última lectura de telemetría, exclusiva del rol Administrador.
/// </summary>
[ApiController]
[Route("api/telemetria")]
[Authorize(Roles = "Administrador")]
public class TelemetriaController : ControllerBase
{
    private readonly ITelemetriaService _telemetriaService;

    public TelemetriaController(ITelemetriaService telemetriaService)
    {
        _telemetriaService = telemetriaService ?? throw new ArgumentNullException(nameof(telemetriaService));
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerUltimaLectura(CancellationToken cancellationToken)
    {
        var lectura = await _telemetriaService.ObtenerUltimaLecturaAsync(cancellationToken);

        return lectura is null ? NoContent() : Ok(lectura);
    }
}
