using Microsoft.EntityFrameworkCore;
using back_end.Modules.reportes.DTOs;
using back_end.Modules.Item.Models;
using back_end.Modules.pagos.Models;
using back_end.Modules.clientes.Models;
using back_end.Modules.reservas.Models;
using back_end.Modules.servicios.Models;
using back_end.Core.Data;

namespace back_end.Modules.reportes.Repositories;

public class ReporteRepository : IReporteRepository
{
    private readonly DbEventusContext _context;

    public ReporteRepository(DbEventusContext context)
    {
        _context = context;
    }

    // MÉTRICAS - CLIENTES
    public async Task<IEnumerable<ClientesNuevosPorMesDto>> GetClientesNuevosPorMesAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var query = _context.Set<Cliente>()
            .Include(c => c.Reservas)
            .AsQueryable();

        var clientesConPrimeraReserva = await query
            .Where(c => c.Reservas.Any())
            .Select(c => new {
                ClienteId = c.Id,
                PrimeraReserva = c.Reservas.OrderBy(r => r.FechaRegistro).First().FechaRegistro
            })
            .ToListAsync();

        var clientesFiltrados = clientesConPrimeraReserva
            .Where(c => c.PrimeraReserva.HasValue &&
                       (!fechaInicio.HasValue || c.PrimeraReserva >= fechaInicio) &&
                       (!fechaFin.HasValue || c.PrimeraReserva <= fechaFin))
            .GroupBy(c => new { 
                Año = c.PrimeraReserva!.Value.Year, 
                Mes = c.PrimeraReserva!.Value.Month 
            })
            .Select(g => new ClientesNuevosPorMesDto
            {
                Año = g.Key.Año,
                Mes = g.Key.Mes,
                NombreMes = new DateTime(g.Key.Año, g.Key.Mes, 1).ToString("MMMM"),
                CantidadClientesNuevos = g.Count()
            })
            .OrderBy(x => x.Año).ThenBy(x => x.Mes);

        return clientesFiltrados;
    }

    public async Task<IEnumerable<PromedioAdelantoPorClienteDto>> GetPromedioAdelantoPorClienteAsync(string? clienteId)
    {
        var query = _context.Set<Cliente>()
            .Include(c => c.Reservas)
            .ThenInclude(r => r.Pagos)
            .AsQueryable();

        if (!string.IsNullOrEmpty(clienteId))
            query = query.Where(c => c.Id == clienteId);

        var resultado = await query
            .Where(c => c.Reservas.Any(r => r.Pagos.Any()))
            .Select(c => new PromedioAdelantoPorClienteDto
            {
                ClienteId = c.Id,
                RazonSocial = c.RazonSocial,
                PromedioAdelantoPorc = c.Reservas
                    .Where(r => r.PrecioTotal.HasValue && r.PrecioTotal > 0 && r.Pagos.Any())
                    .Average(r => (r.Pagos.Sum(p => Convert.ToDecimal(p.Monto)) / r.PrecioTotal!.Value) * 100),
                CantidadReservas = c.Reservas.Count(r => r.Pagos.Any())
            })
            .ToListAsync();

        return resultado;
    }

    public async Task<TasaRetencionClientesDto> GetTasaRetencionClientesAsync(DateTime? fechaInicio, DateTime? fechaFin)
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
            PorcentajeMultiplesReservas = totalClientes > 0 ? (decimal)clientesConMultiplesReservas / totalClientes * 100 : 0,
            TasaRetencion = totalClientes > 0 ? (decimal)clientesConMultiplesReservas / totalClientes * 100 : 0
        };
    }

    // Implementación de nuevas métricas - INVENTARIO/ITEMS
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
                NombreItem = i.Nombre,                TotalCantidadUtilizada = (int)i.DetalleServicios
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
                    ((decimal)i.StockDisponible / i.Stock.Value) * 100 : 0
            })
            .ToListAsync();

        return resultado;
    }

    // Implementación de nuevas métricas - PAGOS
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
            PromedioDias = (decimal)pagosConDias.Average(p => p.DiasEntreFechas),
            CantidadReservasConPagos = pagosConDias.Count,
            DiasMinimo = (decimal)pagosConDias.Min(p => p.DiasEntreFechas),
            DiasMaximo = (decimal)pagosConDias.Max(p => p.DiasEntreFechas)
        };
    }

    public async Task<IEnumerable<ReservasPagosIncompletosDto>> GetReservasPagosIncompletosAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var query = _context.Set<Reserva>()
            .Include(r => r.Cliente)
            .Include(r => r.Pagos)
            .Where(r => r.PrecioTotal.HasValue && r.PrecioTotal > 0)
            .AsQueryable();

        if (fechaInicio.HasValue)
            query = query.Where(r => r.FechaRegistro >= fechaInicio);
        if (fechaFin.HasValue)
            query = query.Where(r => r.FechaRegistro <= fechaFin);

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
                ClienteRazonSocial = x.Reserva.Cliente?.RazonSocial,
                PrecioTotal = x.Reserva.PrecioTotal!.Value,
                TotalPagado = x.TotalPagado,
                MontoPendiente = x.Reserva.PrecioTotal!.Value - x.TotalPagado,
                PorcentajePagado = (x.TotalPagado / x.Reserva.PrecioTotal!.Value) * 100
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
                PorcentajeUso = totalPagos > 0 ? ((decimal)g.Count() / totalPagos) * 100 : 0
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
                Año = p.FechaPago.Year, 
                Mes = p.FechaPago.Month 
            })
            .Select(g => new TendenciaMensualIngresosDto
            {
                Año = g.Key.Año,
                Mes = g.Key.Mes,
                NombreMes = new DateTime(g.Key.Año, g.Key.Mes, 1).ToString("MMMM"),
                MontoTotal = g.Sum(p => Convert.ToDecimal(p.Monto)),
                CantidadPagos = g.Count(),
                MontoPromedio = g.Average(p => Convert.ToDecimal(p.Monto))
            })
            .OrderBy(x => x.Año).ThenBy(x => x.Mes)
            .ToListAsync();

        return resultado;
    }

    // Implementación de nuevas métricas - RESERVAS
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
                Año = r.FechaRegistro!.Value.Year, 
                Mes = r.FechaRegistro!.Value.Month 
            })
            .Select(g => new ReservasPorMesDto
            {
                Año = g.Key.Año,
                Mes = g.Key.Mes,
                NombreMes = new DateTime(g.Key.Año, g.Key.Mes, 1).ToString("MMMM"),
                CantidadReservas = g.Count(),
                MontoTotal = g.Sum(r => r.PrecioTotal ?? 0),
                MontoPromedio = g.Average(r => r.PrecioTotal ?? 0)
            })
            .OrderBy(x => x.Año).ThenBy(x => x.Mes)
            .ToListAsync();

        return resultado;
    }

    public async Task<IEnumerable<IngresosPromedioPorTipoEventoDto>> GetIngresosPromedioPorTipoEventoAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var query = _context.Set<Reserva>()
            .Include(r => r.TiposEventoNavigation)
            .Where(r => r.PrecioTotal.HasValue)
            .AsQueryable();

        if (fechaInicio.HasValue)
            query = query.Where(r => r.FechaRegistro >= fechaInicio);
        if (fechaFin.HasValue)
            query = query.Where(r => r.FechaRegistro <= fechaFin);

        var resultado = await query
            .GroupBy(r => r.TiposEventoNavigation!.Nombre)
            .Select(g => new IngresosPromedioPorTipoEventoDto
            {
                TipoEvento = g.Key,
                IngresoPromedio = g.Average(r => r.PrecioTotal!.Value),
                CantidadReservas = g.Count(),
                IngresoTotal = g.Sum(r => r.PrecioTotal!.Value),
                IngresoMinimo = g.Min(r => r.PrecioTotal!.Value),
                IngresoMaximo = g.Max(r => r.PrecioTotal!.Value)
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
            .Where(r => r.PrecioTotal.HasValue && r.PrecioTotal > 0 && r.Pagos.Any())
            .AsQueryable();

        if (fechaInicio.HasValue)
            query = query.Where(r => r.FechaRegistro >= fechaInicio);
        if (fechaFin.HasValue)
            query = query.Where(r => r.FechaRegistro <= fechaFin);

        var reservas = await query.ToListAsync();

        var resultado = reservas
            .Select(r => new {
                Reserva = r,
                MontoAdelanto = r.Pagos.Sum(p => Convert.ToDecimal(p.Monto)),
                PorcentajeAdelanto = (r.Pagos.Sum(p => Convert.ToDecimal(p.Monto)) / r.PrecioTotal!.Value) * 100
            })
            .Where(x => x.PorcentajeAdelanto >= porcentajeMinimo)
            .Select(x => new ReservasAdelantoAltoDto
            {
                ReservaId = x.Reserva.Id,
                NombreEvento = x.Reserva.NombreEvento,
                ClienteRazonSocial = x.Reserva.Cliente?.RazonSocial,
                PrecioTotal = x.Reserva.PrecioTotal!.Value,
                MontoAdelanto = x.MontoAdelanto,
                PorcentajeAdelanto = x.PorcentajeAdelanto
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
            query = query.Where(r => r.FechaRegistro >= fechaInicio);
        if (fechaFin.HasValue)
            query = query.Where(r => r.FechaRegistro <= fechaFin);

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
        var query = _context.Set<Reserva>().AsQueryable();

        if (fechaInicio.HasValue)
            query = query.Where(r => r.FechaRegistro >= fechaInicio);
        if (fechaFin.HasValue)
            query = query.Where(r => r.FechaRegistro <= fechaFin);

        var reservas = await query
            .GroupBy(r => r.Estado)
            .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
            .ToListAsync();

        var pendientes = reservas.FirstOrDefault(r => r.Estado == "Pendiente")?.Cantidad ?? 0;
        var confirmadas = reservas.FirstOrDefault(r => r.Estado == "Confirmado")?.Cantidad ?? 0;
        var canceladas = reservas.FirstOrDefault(r => r.Estado == "Cancelado")?.Cantidad ?? 0;

        var totalReservas = pendientes + confirmadas + canceladas;

        return new TasaConversionEstadoDto
        {
            ReservasPendientes = pendientes,
            ReservasConfirmadas = confirmadas,
            ReservasCanceladas = canceladas,
            TasaConversionPendienteConfirmado = pendientes > 0 ? ((decimal)confirmadas / (pendientes + confirmadas)) * 100 : 0,
            TasaCancelacion = totalReservas > 0 ? ((decimal)canceladas / totalReservas) * 100 : 0
        };
    }

    public async Task<IEnumerable<DistribucionReservasPorClienteDto>> GetDistribucionReservasPorClienteAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var fechaTresMesesAtras = DateTime.Now.AddMonths(-3);

        var query = _context.Set<Cliente>()
            .Include(c => c.Reservas)
            .AsQueryable();

        var clientes = await query.ToListAsync();

        var resultado = clientes
            .Where(c => c.Reservas.Any(r => 
                (!fechaInicio.HasValue || r.FechaRegistro >= fechaInicio) &&
                (!fechaFin.HasValue || r.FechaRegistro <= fechaFin)))
            .Select(c => new DistribucionReservasPorClienteDto
            {
                ClienteId = c.Id,
                RazonSocial = c.RazonSocial,
                TotalReservas = c.Reservas.Count(r => 
                    (!fechaInicio.HasValue || r.FechaRegistro >= fechaInicio) &&
                    (!fechaFin.HasValue || r.FechaRegistro <= fechaFin)),
                ReservasUltimosTresMeses = c.Reservas.Count(r => 
                    r.FechaRegistro >= fechaTresMesesAtras &&
                    (!fechaFin.HasValue || r.FechaRegistro <= fechaFin)),
                MontoTotalReservas = c.Reservas
                    .Where(r => (!fechaInicio.HasValue || r.FechaRegistro >= fechaInicio) &&
                              (!fechaFin.HasValue || r.FechaRegistro <= fechaFin))
                    .Sum(r => r.PrecioTotal ?? 0),
                UltimaReserva = c.Reservas
                    .Where(r => (!fechaInicio.HasValue || r.FechaRegistro >= fechaInicio) &&
                              (!fechaFin.HasValue || r.FechaRegistro <= fechaFin))
                    .OrderByDescending(r => r.FechaRegistro)
                    .FirstOrDefault()?.FechaRegistro
            })
            .OrderByDescending(x => x.TotalReservas)
            .ToList();

        return resultado;
    }

    // Implementación de nuevas métricas - SERVICIOS
    public async Task<IEnumerable<ServiciosMasFrecuentesDto>> GetServiciosMasFrecuentesAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 10)
    {
        var query = _context.Set<Servicio>()
            .Include(s => s.Reservas)
            .AsQueryable();

        var servicios = await query.ToListAsync();

        var totalReservas = servicios.Sum(s => s.Reservas.Count(r => 
            (!fechaInicio.HasValue || r.FechaRegistro >= fechaInicio) &&
            (!fechaFin.HasValue || r.FechaRegistro <= fechaFin)));

        var resultado = servicios
            .Select(s => new {
                Servicio = s,
                ReservasFiltradas = s.Reservas.Where(r => 
                    (!fechaInicio.HasValue || r.FechaRegistro >= fechaInicio) &&
                    (!fechaFin.HasValue || r.FechaRegistro <= fechaFin)).ToList()
            })
            .Where(x => x.ReservasFiltradas.Any())
            .Select(x => new ServiciosMasFrecuentesDto
            {
                ServicioId = x.Servicio.Id,
                NombreServicio = x.Servicio.Nombre,
                CantidadReservas = x.ReservasFiltradas.Count,
                PorcentajeUso = totalReservas > 0 ? ((decimal)x.ReservasFiltradas.Count / totalReservas) * 100 : 0,
                IngresoTotal = x.ReservasFiltradas.Sum(r => r.PrecioTotal ?? 0),
                IngresoPromedio = x.ReservasFiltradas.Any() ? x.ReservasFiltradas.Average(r => r.PrecioTotal ?? 0) : 0
            })
            .OrderByDescending(x => x.CantidadReservas)
            .Take(top)
            .ToList();

        return resultado;
    }

    public async Task<IEnumerable<VariacionIngresosMensualesServicioDto>> GetVariacionIngresosMensualesServicioAsync(Guid? servicioId, DateTime? fechaInicio, DateTime? fechaFin)
    {
        var query = _context.Set<Reserva>()
            .Include(r => r.Servicio)
            .Where(r => r.FechaRegistro.HasValue && r.PrecioTotal.HasValue)
            .AsQueryable();

        if (servicioId.HasValue)
            query = query.Where(r => r.ServicioId == servicioId);

        if (fechaInicio.HasValue)
            query = query.Where(r => r.FechaRegistro >= fechaInicio);
        if (fechaFin.HasValue)
            query = query.Where(r => r.FechaRegistro <= fechaFin);

        var reservasPorMes = await query
            .GroupBy(r => new { 
                r.ServicioId, 
                r.Servicio!.Nombre,
                Año = r.FechaRegistro!.Value.Year, 
                Mes = r.FechaRegistro!.Value.Month 
            })
            .Select(g => new {
                ServicioId = g.Key.ServicioId,
                NombreServicio = g.Key.Nombre,
                Año = g.Key.Año,
                Mes = g.Key.Mes,
                MontoMensual = g.Sum(r => r.PrecioTotal!.Value),
                CantidadReservas = g.Count()
            })
            .OrderBy(x => x.ServicioId).ThenBy(x => x.Año).ThenBy(x => x.Mes)
            .ToListAsync();

        var resultado = new List<VariacionIngresosMensualesServicioDto>();

        foreach (var grupo in reservasPorMes.GroupBy(x => x.ServicioId))
        {
            var mesesOrdenados = grupo.OrderBy(x => x.Año).ThenBy(x => x.Mes).ToList();
            
            for (int i = 0; i < mesesOrdenados.Count; i++)
            {
                var mesActual = mesesOrdenados[i];
                var variacion = i > 0 ? 
                    ((mesActual.MontoMensual - mesesOrdenados[i-1].MontoMensual) / mesesOrdenados[i-1].MontoMensual) * 100 : 0;

                resultado.Add(new VariacionIngresosMensualesServicioDto
                {
                    ServicioId = mesActual.ServicioId!.Value,
                    NombreServicio = mesActual.NombreServicio,
                    Año = mesActual.Año,
                    Mes = mesActual.Mes,
                    NombreMes = new DateTime(mesActual.Año, mesActual.Mes, 1).ToString("MMMM"),
                    MontoMensual = mesActual.MontoMensual,
                    CantidadReservas = mesActual.CantidadReservas,
                    VariacionPorc = variacion
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
                    (decimal)x.DetallesFiltrados.Average(ds => ds.Cantidad ?? 0) : 0,
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
                PrecioBase = s.PrecioBase,
                DiasInactivo = s.Reservas.Any() ? 
                    (int)(DateTime.Now - s.Reservas.Max(r => r.FechaRegistro ?? DateTime.MinValue)).TotalDays :
                    (int)(DateTime.Now - DateTime.MinValue).TotalDays
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
                PorcentajeCancelacion = ((decimal)x.ReservasCanceladas / x.TotalReservas) * 100,
                MontoPerdidasCancelacion = x.MontoPerdidasCancelacion
            })
            .OrderByDescending(x => x.PorcentajeCancelacion)
            .ToList();

        return resultado;
    }

    // Implementación del resumen ejecutivo
    public async Task<ResumenEjecutivoDto> GetResumenEjecutivoAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var fechaInicioEfectiva = fechaInicio ?? DateTime.Now.AddMonths(-12);
        var fechaFinEfectiva = fechaFin ?? DateTime.Now;
        var inicioMesAnterior = DateTime.Now.AddMonths(-1).Date;
        var finMesAnterior = DateTime.Now.Date.AddDays(-1);

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

        var ingresosTotales = await _context.Set<Reserva>()
            .Where(r => r.FechaRegistro >= fechaInicioEfectiva && r.FechaRegistro <= fechaFinEfectiva && r.PrecioTotal.HasValue)
            .SumAsync(r => r.PrecioTotal!.Value);

        var ingresosUltimoMes = await _context.Set<Reserva>()
            .Where(r => r.FechaRegistro >= inicioMesAnterior && r.FechaRegistro <= finMesAnterior && r.PrecioTotal.HasValue)
            .SumAsync(r => r.PrecioTotal!.Value);

        var tasaConversion = await GetTasaConversionEstadoAsync(fechaInicioEfectiva, fechaFinEfectiva);

        // Métricas de pagos
        var reservasConPagos = await _context.Set<Reserva>()
            .Include(r => r.Pagos)
            .Where(r => r.FechaRegistro >= fechaInicioEfectiva && r.FechaRegistro <= fechaFinEfectiva && r.PrecioTotal.HasValue)
            .ToListAsync();

        var montoPromedioReserva = reservasConPagos.Any() ? reservasConPagos.Average(r => r.PrecioTotal!.Value) : 0;

        var reservasConPagosCompletos = reservasConPagos.Count(r => 
            r.Pagos.Sum(p => Convert.ToDecimal(p.Monto)) >= r.PrecioTotal!.Value);
        var porcentajePagosCompletos = reservasConPagos.Any() ? 
            ((decimal)reservasConPagosCompletos / reservasConPagos.Count) * 100 : 0;

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
            itemsConStock.Average(i => ((decimal)i.StockDisponible / i.Stock) * 100) : 0;

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
            TasaRetencionClientes = tasaRetencion.TasaRetencion,

            // Métricas de reservas
            TotalReservas = totalReservas,
            ReservasUltimoMes = reservasUltimoMes,
            IngresosTotales = ingresosTotales,
            IngresosUltimoMes = ingresosUltimoMes,
            TasaConversionReservas = tasaConversion.TasaConversionPendienteConfirmado,

            // Métricas de pagos
            MontoPromedioReserva = montoPromedioReserva,
            PorcentajePagosCompletos = porcentajePagosCompletos,
            PromedioDiasPago = promedioDiasPago.PromedioDias,

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
}