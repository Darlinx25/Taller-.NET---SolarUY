using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SolarUY.Domain.Entities;
using SolarUY.Domain.Repositories;
using SolarUY.Infrastructure.Persistence;

namespace SolarUY.Infrastructure.Repositories;

public class TareaMantenimientoRepository : ITareaMantenimientoRepository
{
    private readonly SolarUYDbContext _context;

    public TareaMantenimientoRepository(SolarUYDbContext context)
    {
        _context = context;
    }

    public async Task AgregarAsync(TareaMantenimiento tarea, CancellationToken cancellationToken = default)
    {
        await _context.TareasMantenimiento.AddAsync(tarea, cancellationToken);
    }

    public async Task<TareaMantenimiento?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TareasMantenimiento
            .Include(t => t.Panel)
                .ThenInclude(p => p!.Nodo)
                    .ThenInclude(n => n!.ParqueSolar)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TareaMantenimiento>> ObtenerPorPanelAsync(Guid panelId, CancellationToken cancellationToken = default)
    {
        return await _context.TareasMantenimiento
            .Where(t => t.PanelId == panelId)
            .OrderByDescending(t => t.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public void Eliminar(TareaMantenimiento tarea)
    {
        _context.TareasMantenimiento.Remove(tarea);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
