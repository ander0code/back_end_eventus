using Microsoft.EntityFrameworkCore;
using back_end.Modules.reportes.DTOs;
using back_end.Modules.servicios.Models;
using back_end.Modules.reservas.Models;
using back_end.Modules.pagos.Models;
using back_end.Core.Data;

namespace back_end.Modules.reportes.Repositories;

public interface IServiciosReporteRepository
{
    Task<IEnumerable<ServiciosMasFrecuentesDto>> GetServiciosMasFrecuentesAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 10);
    Task<IEnumerable<VariacionIngresosMensualesServicioDto>> GetVariacionIngresosMensualesServicioAsync(string? servicioId, DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<PromedioItemsPorServicioDto>> GetPromedioItemsPorServicioAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<ServiciosSinReservasDto>> GetServiciosSinReservasAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<ServiciosEventosCanceladosDto>> GetServiciosEventosCanceladosAsync(DateTime? fechaInicio, DateTime? fechaFin);
}

public class ServiciosReporteRepository : IServiciosReporteRepository
{
    private readonly DbEventusContext _context;

    public ServiciosReporteRepository(DbEventusContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ServiciosMasFrecuentesDto>> GetServiciosMasFrecuentesAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 10)
    {
        var query = _context.Set<Servicio>()
            .Include(s => s.Reservas)
            .AsQueryable();

        var servicios = await query.ToListAsync();

        var totalReservas = servicios.Sum(s => s.Reservas.Count(r => 
            (!fechaInicio.HasValue || r.FechaRegistro >= fechaInicio) &&
            (!fechaFin.HasValue || r.FechaRegistro <= fechaFin) &&
            r.Estado != "Cancelado" && r.Estado != "Cancelada"));

        var pagosQuery = _context.Set<Pago>()
            .Include(p => p.IdReservaNavigation)
            .ThenInclude(r => r!.Servicio)
            .Where(p => p.IdReservaNavigation != null && 
                       p.IdReservaNavigation.Servicio != null &&
                       p.IdReservaNavigation.Estado != "Cancelado" && 
                       p.IdReservaNavigation.Estado != "Cancelada")
            .AsQueryable();

        if (fechaInicio.HasValue)
            pagosQuery = pagosQuery.Where(p => p.FechaPago >= fechaInicio);
        if (fechaFin.HasValue)
            pagosQuery = pagosQuery.Where(p => p.FechaPago <= fechaFin);

        var pagosPorServicio = await pagosQuery
            .GroupBy(p => new { 
                ServicioId = p.IdReservaNavigation!.ServicioId,
                NombreServicio = p.IdReservaNavigation.Servicio!.Nombre
            })
            .Select(g => new {
                ServicioId = g.Key.ServicioId,
                NombreServicio = g.Key.NombreServicio,
                IngresoTotal = g.Sum(p => Convert.ToDecimal(p.Monto)),
                IngresoPromedio = g.Average(p => Convert.ToDecimal(p.Monto)),
                CantidadReservasConPagos = g.Select(p => p.IdReserva).Distinct().Count()
            })
            .ToListAsync();

        var resultado = servicios
            .Select(s => new {
                Servicio = s,
                ReservasFiltradas = s.Reservas.Where(r => 
                    (!fechaInicio.HasValue || r.FechaRegistro >= fechaInicio) &&
                    (!fechaFin.HasValue || r.FechaRegistro <= fechaFin) &&
                    r.Estado != "Cancelado" && r.Estado != "Cancelada").ToList()
            })
            .Where(x => x.ReservasFiltradas.Any())
            .Select(x => {
                var pagoInfo = pagosPorServicio.FirstOrDefault(p => p.ServicioId == x.Servicio.Id);
                return new ServiciosMasFrecuentesDto
                {
                    ServicioId = x.Servicio.Id,
                    NombreServicio = x.Servicio.Nombre,
                    CantidadReservas = x.ReservasFiltradas.Count,
                    PorcentajeUso = totalReservas > 0 ? Math.Round(((decimal)x.ReservasFiltradas.Count / totalReservas) * 100, 2) : 0,
                    IngresoTotal = pagoInfo?.IngresoTotal ?? 0,
                    IngresoPromedio = pagoInfo?.IngresoPromedio ?? 0
                };
            })
            .OrderByDescending(x => x.CantidadReservas)
            .Take(top)
            .ToList();

        return resultado;
    }

    public async Task<IEnumerable<VariacionIngresosMensualesServicioDto>> GetVariacionIngresosMensualesServicioAsync(string? servicioId, DateTime? fechaInicio, DateTime? fechaFin)
    {
        var query = _context.Set<Reserva>()
            .Include(r => r.Servicio)
            .Where(r => r.FechaEjecucion.HasValue && r.PrecioTotal.HasValue)
            .AsQueryable();

        if (!string.IsNullOrEmpty(servicioId))
            query = query.Where(r => r.ServicioId == servicioId);

        if (fechaInicio.HasValue)
            query = query.Where(r => r.FechaEjecucion >= DateOnly.FromDateTime(fechaInicio.Value));
        if (fechaFin.HasValue)
            query = query.Where(r => r.FechaEjecucion <= DateOnly.FromDateTime(fechaFin.Value));

        var reservasPorMes = await query
            .GroupBy(r => new { 
                r.ServicioId, 
                r.Servicio!.Nombre,
                Anio = r.FechaEjecucion!.Value.Year, 
                Mes = r.FechaEjecucion!.Value.Month 
            })
            .Select(g => new {
                ServicioId = g.Key.ServicioId,
                NombreServicio = g.Key.Nombre,
                Anio = g.Key.Anio,
                Mes = g.Key.Mes,
                MontoMensual = g.Sum(r => r.PrecioTotal!.Value),
                CantidadReservas = g.Count()
            })
            .OrderBy(x => x.ServicioId).ThenBy(x => x.Anio).ThenBy(x => x.Mes)
            .ToListAsync();

        var resultado = new List<VariacionIngresosMensualesServicioDto>();

        foreach (var grupo in reservasPorMes.GroupBy(x => x.ServicioId))
        {
            var mesesOrdenados = grupo.OrderBy(x => x.Anio).ThenBy(x => x.Mes).ToList();
            
            for (int i = 0; i < mesesOrdenados.Count; i++)
            {
                var mesActual = mesesOrdenados[i];
                var variacion = i > 0 ? 
                    ((mesActual.MontoMensual - mesesOrdenados[i-1].MontoMensual) / mesesOrdenados[i-1].MontoMensual) * 100 : 0;

                resultado.Add(new VariacionIngresosMensualesServicioDto
                {
                    ServicioId = mesActual.ServicioId!,
                    NombreServicio = mesActual.NombreServicio,
                    Anio = mesActual.Anio,
                    Mes = mesActual.Mes,
                    NombreMes = new DateTime(mesActual.Anio, mesActual.Mes, 1).ToString("MMMM"),
                    MontoMensual = mesActual.MontoMensual,
                    CantidadReservas = mesActual.CantidadReservas,
                    VariacionPorc = Math.Round(variacion, 2)
                });
            }
        }

        return resultado;
    }

    public async Task<IEnumerable<PromedioItemsPorServicioDto>> GetPromedioItemsPorServicioAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var query = _context.Set<Servicio>()
            .Include(s => s.DetalleServicios)
            .Include(s => s.Reservas)
            .AsQueryable();

        var servicios = await query.ToListAsync();

        var resultado = servicios
            .Where(s => s.DetalleServicios.Any(ds => 
                (!fechaInicio.HasValue || ds.Fecha >= fechaInicio) &&
                (!fechaFin.HasValue || ds.Fecha <= fechaFin)))
            .Select(s => new {
                Servicio = s,
                DetallesFiltrados = s.DetalleServicios.Where(ds => 
                    (!fechaInicio.HasValue || ds.Fecha >= fechaInicio) &&
                    (!fechaFin.HasValue || ds.Fecha <= fechaFin)).ToList(),
                ReservasFiltradas = s.Reservas.Where(r => 
                    (!fechaInicio.HasValue || r.FechaRegistro >= fechaInicio) &&
                    (!fechaFin.HasValue || r.FechaRegistro <= fechaFin)).Count()
            })
            .Select(x => new PromedioItemsPorServicioDto
            {
                ServicioId = x.Servicio.Id,
                NombreServicio = x.Servicio.Nombre,
                PromedioItemsUsados = x.DetallesFiltrados.Any() ? 
                    Math.Round((decimal)x.DetallesFiltrados.Average(ds => ds.Cantidad ?? 0), 2) : 0,
                TotalDetalles = x.DetallesFiltrados.Count,
                CantidadReservas = x.ReservasFiltradas
            })
            .ToList();

        return resultado;
    }

    public async Task<IEnumerable<ServiciosSinReservasDto>> GetServiciosSinReservasAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var query = _context.Set<Servicio>()
            .Include(s => s.Reservas)
            .AsQueryable();

        var servicios = await query.ToListAsync();

        var serviciosSinReservas = servicios
            .Where(s => !s.Reservas.Any(r => 
                (!fechaInicio.HasValue || r.FechaRegistro >= fechaInicio) &&
                (!fechaFin.HasValue || r.FechaRegistro <= fechaFin)))
            .ToList();

        var resultado = serviciosSinReservas
            .Select(s => new ServiciosSinReservasDto
            {
                ServicioId = s.Id,
                NombreServicio = s.Nombre,
                Descripcion = s.Descripcion,
                PrecioBase = s.PrecioBase
            })
            .ToList();

        return resultado;
    }

    public async Task<IEnumerable<ServiciosEventosCanceladosDto>> GetServiciosEventosCanceladosAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var query = _context.Set<Servicio>()
            .Include(s => s.Reservas)
            .AsQueryable();

        var servicios = await query.ToListAsync();

        var resultado = servicios
            .Where(s => s.Reservas.Any(r => 
                (!fechaInicio.HasValue || r.FechaRegistro >= fechaInicio) &&
                (!fechaFin.HasValue || r.FechaRegistro <= fechaFin)))
            .Select(s => new {
                Servicio = s,
                ReservasFiltradas = s.Reservas.Where(r => 
                    (!fechaInicio.HasValue || r.FechaRegistro >= fechaInicio) &&
                    (!fechaFin.HasValue || r.FechaRegistro <= fechaFin)).ToList()
            })
            .Select(x => new {
                ServicioId = x.Servicio.Id,
                NombreServicio = x.Servicio.Nombre,
                TotalReservas = x.ReservasFiltradas.Count,
                ReservasCanceladas = x.ReservasFiltradas.Count(r => r.Estado == "Cancelado"),
                MontoPerdidasCancelacion = x.ReservasFiltradas
                    .Where(r => r.Estado == "Cancelado")
                    .Sum(r => r.PrecioTotal ?? 0)
            })
            .Where(x => x.TotalReservas > 0)
            .Select(x => new ServiciosEventosCanceladosDto
            {
                ServicioId = x.ServicioId,
                NombreServicio = x.NombreServicio,
                TotalReservas = x.TotalReservas,
                ReservasCanceladas = x.ReservasCanceladas,
                PorcentajeCancelacion = Math.Round(((decimal)x.ReservasCanceladas / x.TotalReservas) * 100, 2),
                MontoPerdidasCancelacion = x.MontoPerdidasCancelacion
            })
            .OrderByDescending(x => x.PorcentajeCancelacion)
            .ToList();

        return resultado;
    }
}
