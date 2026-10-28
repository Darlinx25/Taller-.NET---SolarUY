using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SolarUY.Application.Dtos;
using SolarUY.Domain.Entities;
using SolarUY.Domain.Exceptions;
using SolarUY.Domain.Repositories;

namespace SolarUY.Application.Services;

public class NodoService : INodoService
{
    private readonly INodoRepository _nodoRepository;
    private readonly IParqueSolarRepository _parqueSolarRepository;

    public NodoService(INodoRepository nodoRepository, IParqueSolarRepository parqueSolarRepository)
    {
        _nodoRepository = nodoRepository ?? throw new ArgumentNullException(nameof(nodoRepository));
        _parqueSolarRepository = parqueSolarRepository ?? throw new ArgumentNullException(nameof(parqueSolarRepository));
    }

    public async Task<IReadOnlyList<NodoDto>> ObtenerNodosPorParqueAsync(Guid parqueSolarId, CancellationToken cancellationToken = default)
    {
        var nodos = await _nodoRepository.ObtenerPorParqueAsync(parqueSolarId, cancellationToken).ConfigureAwait(false);
        return nodos.Select(MapearNodo).ToList();
    }

    public async Task<NodoDetalleDto?> ObtenerNodoAsync(Guid nodoId, CancellationToken cancellationToken = default)
    {
        var nodo = await _nodoRepository.ObtenerPorIdAsync(nodoId, cancellationToken).ConfigureAwait(false);
        return nodo is null ? null : MapearDetalle(nodo);
    }

    public async Task<Guid> CrearNodoInversorAsync(CrearNodoInversorDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var parqueExiste = await _parqueSolarRepository.ObtenerPorIdAsync(dto.ParqueSolarId, cancellationToken).ConfigureAwait(false);
        if (parqueExiste is null)
            throw new ParqueSolarNoEncontradoException(dto.ParqueSolarId);

        var existeCodigo = await _nodoRepository.ExisteCodigoAsync(dto.Codigo, dto.ParqueSolarId, null, cancellationToken).ConfigureAwait(false);
        if (existeCodigo)
            throw new CodigoNodoDuplicadoException();

        var inversor = new Inversor(dto.InversorMarca, dto.InversorModelo, dto.InversorNumeroSerie, dto.InversorPotenciaNominalkW);
        var nodo = new Nodo(dto.Codigo, dto.ParqueSolarId, inversor, dto.Descripcion, dto.Estado);

        await _nodoRepository.AgregarAsync(nodo, cancellationToken).ConfigureAwait(false);
        await _nodoRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return nodo.Id;
    }

    public async Task EditarNodoInversorAsync(EditarNodoInversorDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var nodo = await _nodoRepository.ObtenerPorIdAsync(dto.Id, cancellationToken).ConfigureAwait(false);
        if (nodo is null)
            throw new NodoNoEncontradoException();

        var existeCodigo = await _nodoRepository.ExisteCodigoAsync(dto.Codigo, nodo.ParqueSolarId, dto.Id, cancellationToken).ConfigureAwait(false);
        if (existeCodigo)
            throw new CodigoNodoDuplicadoException();

        nodo.ActualizarDatos(dto.Codigo, dto.Descripcion, dto.Estado);
        nodo.Inversor.ActualizarDatos(dto.InversorMarca, dto.InversorModelo, dto.InversorNumeroSerie, dto.InversorPotenciaNominalkW);

        await _nodoRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task EliminarNodoAsync(Guid nodoId, CancellationToken cancellationToken = default)
    {
        var nodo = await _nodoRepository.ObtenerPorIdAsync(nodoId, cancellationToken).ConfigureAwait(false);
        if (nodo is null)
            throw new NodoNoEncontradoException();

        _nodoRepository.Eliminar(nodo);
        await _nodoRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static NodoDto MapearNodo(Nodo nodo)
    {
        return new NodoDto
        {
            Id = nodo.Id,
            Codigo = nodo.Codigo,
            ParqueSolarId = nodo.ParqueSolarId,
            Descripcion = nodo.Descripcion,
            Estado = nodo.Estado,
            FechaCreacion = nodo.FechaCreacion,
            Inversor = nodo.Inversor is null ? null : new InversorDto
            {
                Id = nodo.Inversor.Id,
                Marca = nodo.Inversor.Marca,
                Modelo = nodo.Inversor.Modelo,
                NumeroSerie = nodo.Inversor.NumeroSerie,
                PotenciaNominalkW = nodo.Inversor.PotenciaNominalkW
            }
        };
    }

    private static NodoDetalleDto MapearDetalle(Nodo nodo)
    {
        return new NodoDetalleDto
        {
            Id = nodo.Id,
            Codigo = nodo.Codigo,
            ParqueSolarId = nodo.ParqueSolarId,
            ParqueSolarNombre = nodo.ParqueSolar?.Nombre,
            Descripcion = nodo.Descripcion,
            Estado = nodo.Estado,
            FechaCreacion = nodo.FechaCreacion,
            Inversor = nodo.Inversor is null ? null : new InversorDto
            {
                Id = nodo.Inversor.Id,
                Marca = nodo.Inversor.Marca,
                Modelo = nodo.Inversor.Modelo,
                NumeroSerie = nodo.Inversor.NumeroSerie,
                PotenciaNominalkW = nodo.Inversor.PotenciaNominalkW
            }
        };
    }
}
