using Microsoft.EntityFrameworkCore;
using back_end.Modules.reportes.DTOs;
using back_end.Modules.clientes.Models;
using back_end.Modules.reservas.Models;
using back_end.Modules.pagos.Models;
using back_end.Modules.servicios.Models;
using back_end.Core.Data;

namespace back_end.Modules.reportes.Repositories;

public interface IResumenEjecutivoRepository
{
    Task<ResumenEjecutivoDto> GetResumenEjecutivoAsync(DateTime? fechaInicio, DateTime? fechaFin);
}

public class ResumenEjecutivoRepository : IResumenEjecutivoRepository
{
    private readonly DbEventusContext _context;
    private readonly IReservasReporteRepository _reservasReporteRepository;

    public ResumenEjecutivoRepository(DbEventusContext context, IReservasReporteRepository reservasReporteRepository)
    {
        _context = context;
        _reservasReporteRepository = reservasReporteRepository;
    }

    public async Task<ResumenEjecutivoDto> GetResumenEjecutivoAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var fechaInicioEfectiva = fechaInicio ?? DateTime.Now.AddMonths(-12);
        var fechaFinEfectiva = fechaFin ?? DateTime.Now;
        var inicioMesAnterior = DateTime.Now.AddDays(-30).Date;
        var finMesAnterior = DateTime.Now.Date;

        // Métricas de clientes
        var totalClientes = await _context.Set<Cliente>().CountAsync();
        var clientesNuevosUltimoMes = await _context.Set<Cliente>()
            .Include(c => c.Reservas)
            .Where(c => c.Reservas.Any(r => r.FechaRegistro >= inicioMesAnterior && r.FechaRegistro <= finMesAnterior))
            .CountAsync();

        var tasaRetencion = await GetTasaRetencionClientesAsync(fechaInicioEfectiva, fechaFinEfectiva);

        // Métricas de reservas
        var totalReservas = await _context.Set<Reserva>()
            .CountAsync(r => r.FechaRegistro >= fechaInicioEfectiva && r.FechaRegistro <= fechaFinEfectiva);
        
        var reservasUltimoMes = await _context.Set<Reserva>()
            .CountAsync(r => r.FechaRegistro >= inicioMesAnterior && r.FechaRegistro <= finMesAnterior);

        var totalIngresos = await _context.Set<Pago>()
            .Where(p => p.FechaPago >= fechaInicioEfectiva && 
                       p.FechaPago <= fechaFinEfectiva)
            .SumAsync(p => Convert.ToDecimal(p.Monto));

        var totalIngresosUltimoMes = await _context.Set<Pago>()
            .Where(p => p.FechaPago >= inicioMesAnterior && 
                       p.FechaPago <= finMesAnterior)
            .SumAsync(p => Convert.ToDecimal(p.Monto));

        var tasaConversion = await _reservasReporteRepository.GetTasaConversionEstadoAsync(fechaInicioEfectiva, fechaFinEfectiva);

        // Métricas de pagos
        var reservasConPagos = await _context.Set<Reserva>()
            .Include(r => r.Pagos)
            .Where(r => r.FechaRegistro >= fechaInicioEfectiva && 
                       r.FechaRegistro <= fechaFinEfectiva && 
                       r.PrecioTotal.HasValue)
            .ToListAsync();

        var montoPromedioReserva = reservasConPagos.Any() ? reservasConPagos.Average(r => r.PrecioTotal!.Value) : 0;

        var reservasConPagosCompletos = reservasConPagos.Count(r => 
            r.Pagos.Sum(p => Convert.ToDecimal(p.Monto)) >= r.PrecioTotal!.Value);
        var porcentajePagosCompletos = reservasConPagos.Any() ? 
            Math.Round(((decimal)reservasConPagosCompletos / reservasConPagos.Count) * 100, 2) : 0;

        var promedioDiasPago = await GetPromedioDiasReservaPagoAsync(fechaInicioEfectiva, fechaFinEfectiva);

        // Métricas de servicios
        var totalServicios = await _context.Set<Servicio>().CountAsync();
        var serviciosActivos = await _context.Set<Servicio>()
            .Include(s => s.Reservas)
            .CountAsync(s => s.Reservas.Any(r => r.FechaRegistro >= fechaInicioEfectiva && r.FechaRegistro <= fechaFinEfectiva));

        var servicioMasFrecuente = await _context.Set<Servicio>()
            .Include(s => s.Reservas)
            .Where(s => s.Reservas.Any(r => r.FechaRegistro >= fechaInicioEfectiva && r.FechaRegistro <= fechaFinEfectiva))
            .OrderByDescending(s => s.Reservas.Count(r => r.FechaRegistro >= fechaInicioEfectiva && r.FechaRegistro <= fechaFinEfectiva))
            .Select(s => s.Nombre)
            .FirstOrDefaultAsync();

        // Métricas de inventario
        var totalItems = await _context.Set<back_end.Modules.Item.Models.Item>().CountAsync();
        
        var itemsConStock = await _context.Set<back_end.Modules.Item.Models.Item>()
            .Where(i => i.Stock.HasValue && i.Stock > 0)
            .Select(i => new { Stock = i.Stock!.Value, StockDisponible = i.StockDisponible })
            .ToListAsync();

        var tasaDisponibilidadPromedio = itemsConStock.Any() ? 
            Math.Round(itemsConStock.Average(i => ((decimal)i.StockDisponible / i.Stock) * 100), 2) : 0;

        var itemMasUtilizado = await _context.Set<back_end.Modules.Item.Models.Item>()
            .Include(i => i.DetalleServicios)
            .Where(i => i.DetalleServicios.Any(ds => ds.Fecha >= fechaInicioEfectiva && ds.Fecha <= fechaFinEfectiva))
            .OrderByDescending(i => i.DetalleServicios
                .Where(ds => ds.Fecha >= fechaInicioEfectiva && ds.Fecha <= fechaFinEfectiva)
                .Sum(ds => ds.Cantidad ?? 0))
            .Select(i => i.Nombre)
            .FirstOrDefaultAsync();

        return new ResumenEjecutivoDto
        {
            // Métricas de clientes
            TotalClientes = totalClientes,
            ClientesNuevosUltimoMes = clientesNuevosUltimoMes,
            TasaRetencionClientes = Math.Round(tasaRetencion.TasaRetencion, 2),

            // Métricas de reservas
            TotalReservas = totalReservas,
            ReservasUltimoMes = reservasUltimoMes,
            IngresosTotales = totalIngresos,
            IngresosUltimoMes = totalIngresosUltimoMes,
            TasaConversionReservas = Math.Round(tasaConversion.TasaConversionPendienteConfirmado, 2),

            // Métricas de pagos
            MontoPromedioReserva = montoPromedioReserva,
            PorcentajePagosCompletos = porcentajePagosCompletos,
            PromedioDiasPago = Math.Round(promedioDiasPago.PromedioDias, 2),

            // Métricas de servicios
            TotalServicios = totalServicios,
            ServiciosActivos = serviciosActivos,
            ServicioMasFrecuente = servicioMasFrecuente,

            // Métricas de inventario
            TotalItems = totalItems,
            TasaDisponibilidadPromedio = tasaDisponibilidadPromedio,
            ItemMasUtilizado = itemMasUtilizado,

            // Período del reporte
            FechaInicio = fechaInicioEfectiva,
            FechaFin = fechaFinEfectiva,
            FechaGeneracion = DateTime.Now
        };
    }

    private async Task<TasaRetencionClientesDto> GetTasaRetencionClientesAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var query = _context.Set<Cliente>()
            .Include(c => c.Reservas)
            .AsQueryable();

        var clientes = await query.ToListAsync();

        var clientesFiltrados = clientes
            .Where(c => c.Reservas.Any(r => 
                (!fechaInicio.HasValue || r.FechaRegistro >= fechaInicio) &&
                (!fechaFin.HasValue || r.FechaRegistro <= fechaFin)))
            .ToList();

        var totalClientes = clientesFiltrados.Count;
        var clientesConMultiplesReservas = clientesFiltrados.Count(c => 
            c.Reservas.Count(r => 
                (!fechaInicio.HasValue || r.FechaRegistro >= fechaInicio) &&
                (!fechaFin.HasValue || r.FechaRegistro <= fechaFin)) > 1);

        return new TasaRetencionClientesDto
        {
            TotalClientes = totalClientes,
            ClientesConMultiplesReservas = clientesConMultiplesReservas,
            PorcentajeMultiplesReservas = totalClientes > 0 ? Math.Round((decimal)clientesConMultiplesReservas / totalClientes * 100, 2) : 0,
            TasaRetencion = totalClientes > 0 ? Math.Round((decimal)clientesConMultiplesReservas / totalClientes * 100, 2) : 0
        };
    }

    private async Task<PromediotDiasReservaPagoDto> GetPromedioDiasReservaPagoAsync(DateTime? fechaInicio, DateTime? fechaFin)
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
}