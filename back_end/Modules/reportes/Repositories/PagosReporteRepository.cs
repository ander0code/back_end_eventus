using Microsoft.EntityFrameworkCore;
using back_end.Modules.reportes.DTOs;
using back_end.Modules.pagos.Models;
using back_end.Modules.reservas.Models;
using back_end.Core.Data;

namespace back_end.Modules.reportes.Repositories;

public interface IPagosReporteRepository
{
    Task<IEnumerable<MontoPromedioPorPagoDto>> GetMontoPromedioPorPagoAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<PromediotDiasReservaPagoDto> GetPromedioDiasReservaPagoAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<ReservasPagosIncompletosDto>> GetReservasPagosIncompletosAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<TasaUsoMetodoPagoDto>> GetTasaUsoMetodoPagoAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<TendenciaMensualIngresosDto>> GetTendenciaMensualIngresosAsync(DateTime? fechaInicio, DateTime? fechaFin);
}

public class PagosReporteRepository : IPagosReporteRepository
{
    private readonly DbEventusContext _context;

    public PagosReporteRepository(DbEventusContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MontoPromedioPorPagoDto>> GetMontoPromedioPorPagoAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var query = _context.Set<Pago>()
            .Include(p => p.IdTipoPagoNavigation)
            .AsQueryable();

        if (fechaInicio.HasValue)
            query = query.Where(p => p.FechaPago >= fechaInicio);
        if (fechaFin.HasValue)
            query = query.Where(p => p.FechaPago <= fechaFin);

        var resultado = await query
            .GroupBy(p => p.IdTipoPagoNavigation!.Nombre)
            .Select(g => new MontoPromedioPorPagoDto
            {
                TipoPago = g.Key,
                MontoPromedio = g.Average(p => Convert.ToDecimal(p.Monto)),
                CantidadPagos = g.Count(),
                MontoTotal = g.Sum(p => Convert.ToDecimal(p.Monto))
            })
            .ToListAsync();

        return resultado;
    }

    public async Task<PromediotDiasReservaPagoDto> GetPromedioDiasReservaPagoAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var query = _context.Set<Pago>()
            .Include(p => p.IdReservaNavigation)
            .Where(p => p.IdReservaNavigation != null && p.IdReservaNavigation.FechaRegistro.HasValue)
            .AsQueryable();

        if (fechaInicio.HasValue)
            query = query.Where(p => p.FechaPago >= fechaInicio);
        if (fechaFin.HasValue)
            query = query.Where(p => p.FechaPago <= fechaFin);

        var pagosConDias = await query
            .Select(p => new {
                DiasEntreFechas = (p.FechaPago - p.IdReservaNavigation!.FechaRegistro!.Value).TotalDays
            })
            .ToListAsync();

        if (!pagosConDias.Any())
        {
            return new PromediotDiasReservaPagoDto
            {
                PromedioDias = 0,
                CantidadReservasConPagos = 0,
                DiasMinimo = 0,
                DiasMaximo = 0
            };
        }

        return new PromediotDiasReservaPagoDto
        {
            PromedioDias = Math.Round((decimal)pagosConDias.Average(p => p.DiasEntreFechas), 2),
            CantidadReservasConPagos = pagosConDias.Count,
            DiasMinimo = Math.Round((decimal)pagosConDias.Min(p => p.DiasEntreFechas), 2),
            DiasMaximo = Math.Round((decimal)pagosConDias.Max(p => p.DiasEntreFechas), 2)
        };
    }

    public async Task<IEnumerable<ReservasPagosIncompletosDto>> GetReservasPagosIncompletosAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var query = _context.Set<Reserva>()
            .Include(r => r.Cliente)
            .ThenInclude(c => c!.Usuario)
            .Include(r => r.Pagos)
            .Where(r => r.PrecioTotal.HasValue && r.PrecioTotal > 0 && r.FechaEjecucion.HasValue)
            .AsQueryable();

        if (fechaInicio.HasValue)
            query = query.Where(r => r.FechaEjecucion >= DateOnly.FromDateTime(fechaInicio.Value));
        if (fechaFin.HasValue)
            query = query.Where(r => r.FechaEjecucion <= DateOnly.FromDateTime(fechaFin.Value));

        var reservasConPagos = await query.ToListAsync();

        var resultado = reservasConPagos
            .Select(r => new {
                Reserva = r,
                TotalPagado = r.Pagos.Sum(p => Convert.ToDecimal(p.Monto))
            })
            .Where(x => x.TotalPagado < x.Reserva.PrecioTotal!.Value)
            .Select(x => new ReservasPagosIncompletosDto
            {
                ReservaId = x.Reserva.Id,
                NombreEvento = x.Reserva.NombreEvento,
                ClienteRazonSocial = x.Reserva.Cliente?.Usuario?.Nombre,
                PrecioTotal = x.Reserva.PrecioTotal!.Value,
                TotalPagado = x.TotalPagado,
                MontoPendiente = x.Reserva.PrecioTotal!.Value - x.TotalPagado,
                PorcentajePagado = Math.Round((x.TotalPagado / x.Reserva.PrecioTotal!.Value) * 100, 2)
            })
            .ToList();

        return resultado;
    }

    public async Task<IEnumerable<TasaUsoMetodoPagoDto>> GetTasaUsoMetodoPagoAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var query = _context.Set<Pago>()
            .Include(p => p.IdTipoPagoNavigation)
            .AsQueryable();

        if (fechaInicio.HasValue)
            query = query.Where(p => p.FechaPago >= fechaInicio);
        if (fechaFin.HasValue)
            query = query.Where(p => p.FechaPago <= fechaFin);

        var pagos = await query.ToListAsync();
        var totalPagos = pagos.Count;

        var resultado = pagos
            .GroupBy(p => p.IdTipoPagoNavigation?.Nombre)
            .Select(g => new TasaUsoMetodoPagoDto
            {
                TipoPago = g.Key,
                CantidadUsos = g.Count(),
                MontoTotal = g.Sum(p => Convert.ToDecimal(p.Monto)),
                PorcentajeUso = totalPagos > 0 ? Math.Round(((decimal)g.Count() / totalPagos) * 100, 2) : 0
            })
            .OrderByDescending(x => x.PorcentajeUso)
            .ToList();

        return resultado;
    }

    public async Task<IEnumerable<TendenciaMensualIngresosDto>> GetTendenciaMensualIngresosAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var query = _context.Set<Pago>().AsQueryable();

        if (fechaInicio.HasValue)
            query = query.Where(p => p.FechaPago >= fechaInicio);
        if (fechaFin.HasValue)
            query = query.Where(p => p.FechaPago <= fechaFin);

        var resultado = await query
            .GroupBy(p => new { 
                Anio = p.FechaPago.Year, 
                Mes = p.FechaPago.Month 
            })
            .Select(g => new TendenciaMensualIngresosDto
            {
                Anio = g.Key.Anio,
                Mes = g.Key.Mes,
                NombreMes = new DateTime(g.Key.Anio, g.Key.Mes, 1).ToString("MMMM"),
                MontoTotal = g.Sum(p => Convert.ToDecimal(p.Monto)),
                CantidadPagos = g.Count(),
                MontoPromedio = g.Average(p => Convert.ToDecimal(p.Monto))
            })
            .OrderBy(x => x.Anio).ThenBy(x => x.Mes)
            .ToListAsync();

        return resultado;
    }
}
