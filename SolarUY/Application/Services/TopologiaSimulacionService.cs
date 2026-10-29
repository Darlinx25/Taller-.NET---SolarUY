using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SolarUY.Application.Dtos;
using SolarUY.Domain.Entities;
using SolarUY.Domain.Repositories;

namespace SolarUY.Application.Services;

/// <summary>
/// Entrega al simulador la topología registrada (solo lectura). No filtra por estado:
/// el simulador decide qué nodos simular.
/// </summary>
public class TopologiaSimulacionService : ITopologiaSimulacionService
{
    private readonly IParqueSolarRepository _parqueSolarRepository;
    private readonly INodoRepository _nodoRepository;
    private readonly IPanelRepository _panelRepository;

    public TopologiaSimulacionService(
        IParqueSolarRepository parqueSolarRepository,
        INodoRepository nodoRepository,
        IPanelRepository panelRepository)
    {
        _parqueSolarRepository = parqueSolarRepository ?? throw new ArgumentNullException(nameof(parqueSolarRepository));
        _nodoRepository = nodoRepository ?? throw new ArgumentNullException(nameof(nodoRepository));
        _panelRepository = panelRepository ?? throw new ArgumentNullException(nameof(panelRepository));
    }

    public async Task<TopologiaSimulacionDto> ObtenerAsync(CancellationToken cancellationToken = default)
    {
        var parques = await _parqueSolarRepository
            .ObtenerTodosAsync(cancellationToken)
            .ConfigureAwait(false);

        var parquesDto = new List<ParqueSimulacionDto>(parques.Count);
        foreach (var parque in parques)
        {
            parquesDto.Add(await MapearParqueAsync(parque, cancellationToken).ConfigureAwait(false));
        }

        return new TopologiaSimulacionDto { Parques = parquesDto };
    }

    private async Task<ParqueSimulacionDto> MapearParqueAsync(
        ParqueSolar parque,
        CancellationToken cancellationToken)
    {
        var nodos = await _nodoRepository
            .ObtenerPorParqueAsync(parque.Id, cancellationToken)
            .ConfigureAwait(false);

        var nodosDto = new List<NodoSimulacionDto>(nodos.Count);
        foreach (var nodo in nodos)
        {
            nodosDto.Add(await MapearNodoAsync(nodo, cancellationToken).ConfigureAwait(false));
        }

        return new ParqueSimulacionDto
        {
            ParqueSolarId = parque.Id,
            NombreParque = parque.Nombre,
            Nodos = nodosDto
        };
    }

    private async Task<NodoSimulacionDto> MapearNodoAsync(Nodo nodo, CancellationToken cancellationToken)
    {
        var paneles = await _panelRepository
            .ObtenerPorNodoAsync(nodo.Id, cancellationToken)
            .ConfigureAwait(false);

        return new NodoSimulacionDto
        {
            NodoId = nodo.Id,
            CodigoNodo = nodo.Codigo,
            Estado = nodo.Estado,
            PotenciaNominalInversorKW = nodo.Inversor.PotenciaNominalkW,
            Paneles = paneles
                .Select(panel => new PanelSimulacionDto
                {
                    PanelId = panel.Id,
                    CodigoSerie = panel.CodigoSerie,
                    PotenciaWp = panel.PotenciaWp
                })
                .ToList()
        };
    }
}
