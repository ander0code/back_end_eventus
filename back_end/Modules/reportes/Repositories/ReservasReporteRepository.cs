using Microsoft.EntityFrameworkCore;
using back_end.Modules.reportes.DTOs;
using back_end.Modules.reservas.Models;
using back_end.Modules.pagos.Models;
using back_end.Modules.clientes.Models;
using back_end.Core.Data;

namespace back_end.Modules.reportes.Repositories;

public interface IReservasReporteRepository
{
    Task<IEnumerable<ReservasPorMesDto>> GetReservasPorMesAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<IngresosPromedioPorTipoEventoDto>> GetIngresosPromedioPorTipoEventoAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<ReservasAdelantoAltoDto>> GetReservasAdelantoAltoAsync(decimal porcentajeMinimo = 50, DateTime? fechaInicio = null, DateTime? fechaFin = null);
    Task<DuracionPromedioReservasDto> GetDuracionPromedioReservasAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<TasaConversionEstadoDto> GetTasaConversionEstadoAsync(DateTime? fechaInicio, DateTime? fechaFin);
}

public class ReservasReporteRepository : IReservasReporteRepository
{
    private readonly DbEventusContext _context;

    public ReservasReporteRepository(DbEventusContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ReservasPorMesDto>> GetReservasPorMesAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var query = _context.Set<Reserva>()
            .Where(r => r.FechaRegistro.HasValue)
            .AsQueryable();

        if (fechaInicio.HasValue)
            query = query.Where(r => r.FechaRegistro >= fechaInicio);
        if (fechaFin.HasValue)
            query = query.Where(r => r.FechaRegistro <= fechaFin);

        var resultado = await query
            .GroupBy(r => new { 
                Anio = r.FechaRegistro!.Value.Year, 
                Mes = r.FechaRegistro!.Value.Month 
            })
            .Select(g => new ReservasPorMesDto
            {
                Anio = g.Key.Anio,
                Mes = g.Key.Mes,
                NombreMes = new DateTime(g.Key.Anio, g.Key.Mes, 1).ToString("MMMM"),
                CantidadReservas = g.Count(r => r.Estado != "Cancelado" && r.Estado != "Cancelada"),
                MontoTotal = g.Where(r => r.Estado != "Cancelado" && r.Estado != "Cancelada").Sum(r => r.PrecioTotal ?? 0),
                MontoPromedio = g.Where(r => r.Estado != "Cancelado" && r.Estado != "Cancelada").Any() ? 
                               g.Where(r => r.Estado != "Cancelado" && r.Estado != "Cancelada").Average(r => r.PrecioTotal ?? 0) : 0
            })
            .OrderBy(x => x.Anio).ThenBy(x => x.Mes)
            .ToListAsync();

        return resultado;
    }

    public async Task<IEnumerable<IngresosPromedioPorTipoEventoDto>> GetIngresosPromedioPorTipoEventoAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var query = _context.Set<Pago>()
            .Include(p => p.IdReservaNavigation)
            .ThenInclude(r => r!.TiposEventoNavigation)
            .Where(p => p.IdReservaNavigation != null && 
                       p.IdReservaNavigation.TiposEventoNavigation != null &&
                       p.IdReservaNavigation.Estado != "Cancelado" && 
                       p.IdReservaNavigation.Estado != "Cancelada")
            .AsQueryable();

        if (fechaInicio.HasValue)
            query = query.Where(p => p.FechaPago >= fechaInicio);
        if (fechaFin.HasValue)
            query = query.Where(p => p.FechaPago <= fechaFin);

        var resultado = await query
            .GroupBy(p => p.IdReservaNavigation!.TiposEventoNavigation!.Nombre)
            .Select(g => new IngresosPromedioPorTipoEventoDto
            {
                TipoEvento = g.Key,
                IngresoPromedio = g.Average(p => Convert.ToDecimal(p.Monto)),
                CantidadReservas = g.Select(p => p.IdReserva).Distinct().Count(),
                IngresoTotal = g.Sum(p => Convert.ToDecimal(p.Monto)),
                IngresoMinimo = g.Min(p => Convert.ToDecimal(p.Monto)),
                IngresoMaximo = g.Max(p => Convert.ToDecimal(p.Monto))
            })
            .OrderByDescending(x => x.IngresoPromedio)
            .ToListAsync();

        return resultado;
    }

    public async Task<IEnumerable<ReservasAdelantoAltoDto>> GetReservasAdelantoAltoAsync(decimal porcentajeMinimo = 50, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        var query = _context.Set<Reserva>()
            .Include(r => r.Cliente)
            .Include(r => r.Pagos)
            .Where(r => r.PrecioTotal.HasValue && r.PrecioTotal > 0 && r.Pagos.Any() && r.FechaEjecucion.HasValue)
            .AsQueryable();

        if (fechaInicio.HasValue)
            query = query.Where(r => r.FechaEjecucion >= DateOnly.FromDateTime(fechaInicio.Value));
        if (fechaFin.HasValue)
            query = query.Where(r => r.FechaEjecucion <= DateOnly.FromDateTime(fechaFin.Value));

        var reservas = await query.ToListAsync();

        var resultado = reservas
            .Select(r => new {
                Reserva = r,
                MontoAdelanto = r.Pagos.Sum(p => Convert.ToDecimal(p.Monto)),
                PorcentajeAdelanto = Math.Round((r.Pagos.Sum(p => Convert.ToDecimal(p.Monto)) / r.PrecioTotal!.Value) * 100, 2)
            })
            .Where(x => x.PorcentajeAdelanto >= porcentajeMinimo)
            .Select(x => new ReservasAdelantoAltoDto
            {
                ReservaId = x.Reserva.Id,
                NombreEvento = x.Reserva.NombreEvento,
                ClienteRazonSocial = x.Reserva.Cliente?.RazonSocial,
                PrecioTotal = x.Reserva.PrecioTotal!.Value,
                MontoAdelanto = x.MontoAdelanto,
                PorcentajeAdelanto = Math.Round(x.PorcentajeAdelanto, 2)
            })
            .OrderByDescending(x => x.PorcentajeAdelanto)
            .ToList();

        return resultado;
    }

    public async Task<DuracionPromedioReservasDto> GetDuracionPromedioReservasAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var query = _context.Set<Reserva>()
            .Where(r => r.FechaRegistro.HasValue && r.FechaEjecucion.HasValue)
            .AsQueryable();

        if (fechaInicio.HasValue)
            query = query.Where(r => r.FechaEjecucion >= DateOnly.FromDateTime(fechaInicio.Value));
        if (fechaFin.HasValue)
            query = query.Where(r => r.FechaEjecucion <= DateOnly.FromDateTime(fechaFin.Value));

        var reservas = await query
            .Select(r => new {
                DuracionDias = (r.FechaEjecucion!.Value.ToDateTime(TimeOnly.MinValue) - r.FechaRegistro!.Value).TotalDays
            })
            .ToListAsync();

        if (!reservas.Any())
        {
            return new DuracionPromedioReservasDto
            {
                DuracionPromedioDias = 0,
                CantidadReservas = 0,
                DuracionMinimaDias = 0,
                DuracionMaximaDias = 0
            };
        }

        return new DuracionPromedioReservasDto
        {
            DuracionPromedioDias = (decimal)reservas.Average(r => r.DuracionDias),
            CantidadReservas = reservas.Count,
            DuracionMinimaDias = (decimal)reservas.Min(r => r.DuracionDias),
            DuracionMaximaDias = (decimal)reservas.Max(r => r.DuracionDias)
        };
    }

    public async Task<TasaConversionEstadoDto> GetTasaConversionEstadoAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var query = _context.Set<Reserva>()
            .AsQueryable();

        if (fechaInicio.HasValue || fechaFin.HasValue)
        {
            query = query.Where(r => r.FechaRegistro.HasValue);
            
            if (fechaInicio.HasValue)
                query = query.Where(r => r.FechaRegistro >= fechaInicio);
            if (fechaFin.HasValue)
                query = query.Where(r => r.FechaRegistro <= fechaFin);
        }

        var reservas = await query
            .GroupBy(r => r.Estado)
            .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
            .ToListAsync();

        var pendientes = reservas.FirstOrDefault(r => r.Estado == "Pendiente")?.Cantidad ?? 0;
        var confirmadas = reservas.FirstOrDefault(r => r.Estado == "Confirmado")?.Cantidad ?? 0;
        var canceladas = reservas.FirstOrDefault(r => r.Estado == "Cancelado")?.Cantidad ?? 0;
        var finalizadas = reservas.FirstOrDefault(r => r.Estado == "Finalizado")?.Cantidad ?? 0;

        var totalReservas = pendientes + confirmadas + canceladas + finalizadas;
        var reservasActivas = pendientes + confirmadas;

        return new TasaConversionEstadoDto
        {
            ReservasPendientes = pendientes,
            ReservasConfirmadas = confirmadas,
            ReservasCanceladas = canceladas,
            ReservasFinalizadas = finalizadas,
            TasaConversionPendienteConfirmado = reservasActivas > 0 ? Math.Round(((decimal)confirmadas / reservasActivas) * 100, 2) : 0,
            TasaCancelacion = totalReservas > 0 ? Math.Round(((decimal)canceladas / totalReservas) * 100, 2) : 0,
            TasaFinalizacion = totalReservas > 0 ? Math.Round(((decimal)finalizadas / totalReservas) * 100, 2) : 0
        };
    }
}
