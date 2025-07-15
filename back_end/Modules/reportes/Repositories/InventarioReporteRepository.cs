using Microsoft.EntityFrameworkCore;
using back_end.Modules.reportes.DTOs;
using back_end.Modules.Item.Models;
using back_end.Modules.servicios.Models;
using back_end.Core.Data;

namespace back_end.Modules.reportes.Repositories;

public interface IInventarioReporteRepository
{
    Task<IEnumerable<ItemsMasUtilizadosDto>> GetItemsMasUtilizadosAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 10);
    Task<IEnumerable<StockPromedioPorTipoServicioDto>> GetStockPromedioPorTipoServicioAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<TasaDisponibilidadDto>> GetTasaDisponibilidadAsync();
}

public class InventarioReporteRepository : IInventarioReporteRepository
{
    private readonly DbEventusContext _context;

    public InventarioReporteRepository(DbEventusContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ItemsMasUtilizadosDto>> GetItemsMasUtilizadosAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 10)
    {
        var query = _context.Set<back_end.Modules.Item.Models.Item>()
            .Include(i => i.DetalleServicios)
            .AsQueryable();

        var resultado = await query
            .Where(i => i.DetalleServicios.Any(ds => 
                (!fechaInicio.HasValue || ds.Fecha >= fechaInicio) &&
                (!fechaFin.HasValue || ds.Fecha <= fechaFin)))
            .Select(i => new ItemsMasUtilizadosDto
            {
                InventarioId = i.Id,
                NombreItem = i.Nombre,
                TotalCantidadUtilizada = (int)i.DetalleServicios
                    .Where(ds => (!fechaInicio.HasValue || ds.Fecha >= fechaInicio) &&
                                (!fechaFin.HasValue || ds.Fecha <= fechaFin))
                    .Sum(ds => ds.Cantidad ?? 0),
                FrecuenciaUso = i.DetalleServicios
                    .Count(ds => (!fechaInicio.HasValue || ds.Fecha >= fechaInicio) &&
                               (!fechaFin.HasValue || ds.Fecha <= fechaFin)),
                PromedioUsoPorServicio = i.DetalleServicios
                    .Where(ds => (!fechaInicio.HasValue || ds.Fecha >= fechaInicio) &&
                                (!fechaFin.HasValue || ds.Fecha <= fechaFin))
                    .Any() ?
                    (decimal)i.DetalleServicios
                        .Where(ds => (!fechaInicio.HasValue || ds.Fecha >= fechaInicio) &&
                                    (!fechaFin.HasValue || ds.Fecha <= fechaFin))
                        .Average(ds => ds.Cantidad ?? 0) : 0
            })
            .OrderByDescending(x => x.TotalCantidadUtilizada)
            .Take(top)
            .ToListAsync();

        return resultado;
    }

    public async Task<IEnumerable<StockPromedioPorTipoServicioDto>> GetStockPromedioPorTipoServicioAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var resultado = await _context.Set<Servicio>()
            .Include(s => s.DetalleServicios)
            .Where(s => s.DetalleServicios.Any(ds => 
                (!fechaInicio.HasValue || ds.Fecha >= fechaInicio) &&
                (!fechaFin.HasValue || ds.Fecha <= fechaFin)))
            .Select(s => new StockPromedioPorTipoServicioDto
            {
                ServicioId = s.Id,
                NombreServicio = s.Nombre,
                StockPromedioUtilizado = s.DetalleServicios
                    .Where(ds => (!fechaInicio.HasValue || ds.Fecha >= fechaInicio) &&
                                (!fechaFin.HasValue || ds.Fecha <= fechaFin))
                    .Any() ?
                    (decimal)s.DetalleServicios
                        .Where(ds => (!fechaInicio.HasValue || ds.Fecha >= fechaInicio) &&
                                    (!fechaFin.HasValue || ds.Fecha <= fechaFin))
                        .Average(ds => ds.Cantidad ?? 0) : 0,
                CantidadDetalles = s.DetalleServicios
                    .Count(ds => (!fechaInicio.HasValue || ds.Fecha >= fechaInicio) &&
                               (!fechaFin.HasValue || ds.Fecha <= fechaFin))
            })
            .ToListAsync();

        return resultado;
    }

    public async Task<IEnumerable<TasaDisponibilidadDto>> GetTasaDisponibilidadAsync()
    {
        var resultado = await _context.Set<back_end.Modules.Item.Models.Item>()
            .Where(i => i.Stock.HasValue && i.Stock > 0)
            .Select(i => new TasaDisponibilidadDto
            {
                InventarioId = i.Id,
                NombreItem = i.Nombre,
                Stock = i.Stock ?? 0,
                StockDisponible = i.StockDisponible,
                TasaDisponibilidadPorc = i.Stock.HasValue && i.Stock > 0 ? 
                    Math.Round(((decimal)i.StockDisponible / i.Stock.Value) * 100, 2) : 0
            })
            .ToListAsync();

        return resultado;
    }
}
