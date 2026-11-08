using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarUY.Application.Dtos;
using SolarUY.Application.Services;

namespace SolarUY.Controllers;

/// <summary>
/// Órdenes del administrador al simulador desde el dashboard. Se autentica con la sesión del usuario
/// (rol Administrador); el simulador no usa estos endpoints.
/// </summary>
[ApiController]
[Route("api/control-simulacion")]
[Authorize(Roles = "Administrador")]
public class ControlSimulacionController : ControllerBase
{
    private readonly IControlSimulacionService _controlSimulacionService;

    public ControlSimulacionController(IControlSimulacionService controlSimulacionService)
    {
        _controlSimulacionService = controlSimulacionService ?? throw new ArgumentNullException(nameof(controlSimulacionService));
    }

    [HttpPost("simulacion-forzada")]
    [Consumes("application/json")]
    public async Task<IActionResult> EstablecerSimulacionForzada(
        [FromBody] SolicitudSimulacionForzadaDto solicitud,
        CancellationToken cancellationToken)
    {
        var control = await _controlSimulacionService.EstablecerSimulacionForzadaAsync(solicitud.Activa, cancellationToken);
        return Ok(control);
    }

    [HttpPost("falla")]
    [Consumes("application/json")]
    public async Task<IActionResult> SolicitarFalla(CancellationToken cancellationToken)
    {
        var control = await _controlSimulacionService.SolicitarFallaAsync(cancellationToken);
        return Ok(control);
    }
}
