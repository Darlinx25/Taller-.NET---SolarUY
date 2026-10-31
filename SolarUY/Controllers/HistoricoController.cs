using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarUY.Application.Dtos;
using SolarUY.Application.Services;
using SolarUY.Domain.Exceptions;
using SolarUY.Models;

namespace SolarUY.Controllers;

/// <summary>
/// Consulta de solo lectura de los históricos de generación.
/// </summary>
[Authorize]
public class HistoricoController : Controller
{
    private readonly IHistoricoService _historicoService;

    public HistoricoController(IHistoricoService historicoService)
    {
        _historicoService = historicoService
            ?? throw new ArgumentNullException(nameof(historicoService));
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery] FiltroHistoricoDto? filtro,
        CancellationToken cancellationToken)
    {
        var usarFiltroPredeterminado = filtro is null || Request.Query.Count == 0;
        var filtroActual = usarFiltroPredeterminado
            ? _historicoService.CrearFiltroPredeterminado()
            : filtro!;

        if (usarFiltroPredeterminado)
            ModelState.Clear();

        var opciones = await _historicoService
            .ObtenerOpcionesAsync(cancellationToken)
            .ConfigureAwait(false);

        var resultado = new HistoricoGeneracionDto();

        if (ModelState.IsValid)
        {
            try
            {
                resultado = await _historicoService
                    .ConsultarAsync(filtroActual, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (FiltroHistoricoInvalidoException excepcion)
            {
                ModelState.AddModelError(string.Empty, excepcion.Message);
            }
        }

        return View(new HistoricoViewModel
        {
            Filtro = filtroActual,
            Opciones = opciones,
            Resultado = resultado
        });
    }
}
