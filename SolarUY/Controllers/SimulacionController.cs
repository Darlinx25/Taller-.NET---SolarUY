using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarUY.Application.Dtos;
using SolarUY.Application.Seguridad;
using SolarUY.Application.Services;

namespace SolarUY.Controllers;

/// <summary>
/// Endpoints internos del componente simulador (mock del inversor). Se autentican con API key,
/// no con cookie ni con rol de usuario.
/// </summary>
[ApiController]
[Route("api/simulacion")]
[Authorize(AuthenticationSchemes = AutenticacionComponentes.EsquemaApiKey)]
public class SimulacionController : ControllerBase
{
    private readonly ITopologiaSimulacionService _topologiaSimulacionService;
    private readonly ITelemetriaService _telemetriaService;

    public SimulacionController(
        ITopologiaSimulacionService topologiaSimulacionService,
        ITelemetriaService telemetriaService)
    {
        _topologiaSimulacionService = topologiaSimulacionService ?? throw new ArgumentNullException(nameof(topologiaSimulacionService));
        _telemetriaService = telemetriaService ?? throw new ArgumentNullException(nameof(telemetriaService));
    }

    [HttpGet("topologia")]
    public async Task<IActionResult> ObtenerTopologia(CancellationToken cancellationToken)
    {
        var topologia = await _topologiaSimulacionService.ObtenerAsync(cancellationToken);
        return Ok(topologia);
    }

    [HttpPost("telemetria")]
    public async Task<IActionResult> RecibirTelemetria(
        [FromBody] TelemetriaDto? lectura,
        CancellationToken cancellationToken)
    {
        var resultado = await _telemetriaService.RegistrarLecturaAsync(lectura, cancellationToken);

        return resultado.EsValida
            ? NoContent()
            : BadRequest(new { errores = resultado.Errores });
    }
}
