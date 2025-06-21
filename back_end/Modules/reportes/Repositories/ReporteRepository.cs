using Microsoft.EntityFrameworkCore;
using back_end.Modules.reportes.DTOs;
using back_end.Modules.Item.Models;
using back_end.Modules.pagos.Models;
using back_end.Modules.clientes.Models;
using back_end.Modules.reservas.Models;
using back_end.Modules.servicios.Models;

namespace back_end.Modules.reportes.Repositories;

public class ReporteRepository : IReporteRepository
{
    private readonly DbContext _context;

    public ReporteRepository(DbContext context)
    {
        _context = context;
    }    public async Task<IEnumerable<ReporteItemDto>> GetReporteItemsAsync(ReporteItemParametrosDto parametros)
    {
        var query = _context.Set<back_end.Modules.Item.Models.Item>().AsQueryable();        if (!string.IsNullOrEmpty(parametros.Nombre))
            query = query.Where(i => i.Nombre != null && i.Nombre.Contains(parametros.Nombre));

        if (parametros.StockMinimo.HasValue)
            query = query.Where(i => i.Stock >= parametros.StockMinimo);

        if (parametros.StockMaximo.HasValue)
            query = query.Where(i => i.Stock <= parametros.StockMaximo);

        return await query
            .Include(i => i.DetalleServicios)
            .Select(i => new ReporteItemDto
            {
                Id = i.Id,
                Nombre = i.Nombre,
                Descripcion = i.Descripcion,
                Stock = i.Stock,
                StockDisponible = i.StockDisponible,
                Preciobase = i.Preciobase,
                CantidadServicios = i.DetalleServicios.Count
            })
            .ToListAsync();
    }    public async Task<IEnumerable<ReportePagoDto>> GetReportePagosAsync(ReportePagoParametrosDto parametros)
    {
        var query = _context.Set<Pago>().AsQueryable();

        if (parametros.FechaInicio.HasValue)
            query = query.Where(p => p.FechaPago >= parametros.FechaInicio);

        if (parametros.FechaFin.HasValue)
            query = query.Where(p => p.FechaPago <= parametros.FechaFin);
        if (!string.IsNullOrEmpty(parametros.TipoPago))
            query = query.Where(p => p.IdTipoPagoNavigation != null && p.IdTipoPagoNavigation.Nombre != null && p.IdTipoPagoNavigation.Nombre.Contains(parametros.TipoPago));

        if (!string.IsNullOrEmpty(parametros.ClienteId))
            query = query.Where(p => p.IdReservaNavigation != null && p.IdReservaNavigation.ClienteId == parametros.ClienteId);

        return await query
        .Include(p => p.IdTipoPagoNavigation)
        .Include(p => p.IdReservaNavigation)
        .Select(p => new ReportePagoDto
        {
            Id = p.Id,
            IdReserva = p.IdReserva,
            TipoPago = p.IdTipoPagoNavigation != null ? p.IdTipoPagoNavigation.Nombre : null,
            Monto = p.Monto,
            FechaPago = p.FechaPago,
            NombreEvento = p.IdReservaNavigation != null ? p.IdReservaNavigation.NombreEvento : null,
            ClienteRazonSocial = p.IdReservaNavigation != null && p.IdReservaNavigation.Cliente != null ? p.IdReservaNavigation.Cliente.RazonSocial : null
        })
        .ToListAsync();
    }    public async Task<IEnumerable<ReporteClienteDto>> GetReporteClientesAsync(ReporteClienteParametrosDto parametros)
    {
        var query = _context.Set<Cliente>().AsQueryable();

        if (!string.IsNullOrEmpty(parametros.TipoCliente))
            query = query.Where(c => c.TipoCliente == parametros.TipoCliente);        if (!string.IsNullOrEmpty(parametros.Ruc))
            query = query.Where(c => c.Ruc != null && c.Ruc.Contains(parametros.Ruc));

        if (!string.IsNullOrEmpty(parametros.RazonSocial))
            query = query.Where(c => c.RazonSocial != null && c.RazonSocial.Contains(parametros.RazonSocial));        var result = await query
            .Include(c => c.Reservas)
            .ToListAsync();

        return result.Select(c => new ReporteClienteDto
        {
            Id = c.Id,
            TipoCliente = c.TipoCliente,
            Direccion = c.Direccion,
            Ruc = c.Ruc,
            RazonSocial = c.RazonSocial,
            CantidadReservas = c.Reservas.Count,
            MontoTotalReservas = c.Reservas.Sum(r => r.PrecioTotal),
            UltimaReserva = c.Reservas.OrderByDescending(r => r.FechaRegistro).FirstOrDefault()?.FechaRegistro
        })
        .Where(c => !parametros.ReservasMinimas.HasValue || c.CantidadReservas >= parametros.ReservasMinimas);
    }    public async Task<IEnumerable<ReporteReservaDto>> GetReporteReservasAsync(ReporteReservaParametrosDto parametros)
    {
        var query = _context.Set<Reserva>().AsQueryable();

        if (parametros.FechaEjecucionInicio.HasValue)
            query = query.Where(r => r.FechaEjecucion >= DateOnly.FromDateTime(parametros.FechaEjecucionInicio.Value));

        if (parametros.FechaEjecucionFin.HasValue)
            query = query.Where(r => r.FechaEjecucion <= DateOnly.FromDateTime(parametros.FechaEjecucionFin.Value));

        if (parametros.FechaRegistroInicio.HasValue)
            query = query.Where(r => r.FechaRegistro >= parametros.FechaRegistroInicio);

        if (parametros.FechaRegistroFin.HasValue)
            query = query.Where(r => r.FechaRegistro <= parametros.FechaRegistroFin);

        if (!string.IsNullOrEmpty(parametros.Estado))
            query = query.Where(r => r.Estado == parametros.Estado);

        if (!string.IsNullOrEmpty(parametros.ClienteId))
            query = query.Where(r => r.ClienteId == parametros.ClienteId);

        if (parametros.PrecioMinimo.HasValue)
            query = query.Where(r => r.PrecioTotal >= parametros.PrecioMinimo);

        if (parametros.PrecioMaximo.HasValue)
            query = query.Where(r => r.PrecioTotal <= parametros.PrecioMaximo);

        return await query
            .Include(r => r.Cliente)
            .Include(r => r.Servicio)
            .Include(r => r.TiposEventoNavigation)
            .Include(r => r.Pagos)            .Select(r => new ReporteReservaDto
            {
                Id = r.Id,
                NombreEvento = r.NombreEvento,
                FechaEjecucion = r.FechaEjecucion,
                FechaRegistro = r.FechaRegistro,
                Descripcion = r.Descripcion,
                Estado = r.Estado,
                PrecioTotal = r.PrecioTotal,
                TipoEvento = r.TiposEventoNavigation != null ? r.TiposEventoNavigation.Nombre : null,
                ClienteRazonSocial = r.Cliente != null ? r.Cliente.RazonSocial : null,
                ServicioNombre = r.Servicio != null ? r.Servicio.Nombre : null,
                PrecioAdelanto = r.PrecioAdelanto,
                CantidadPagos = r.Pagos.Count
            })
            .ToListAsync();
    }    public async Task<IEnumerable<ReporteServicioDto>> GetReporteServiciosAsync(ReporteServicioParametrosDto parametros)
    {
        var query = _context.Set<Servicio>().AsQueryable();        if (!string.IsNullOrEmpty(parametros.Nombre))
            query = query.Where(s => s.Nombre != null && s.Nombre.Contains(parametros.Nombre));

        if (parametros.PrecioMinimo.HasValue)
            query = query.Where(s => s.PrecioBase >= parametros.PrecioMinimo);

        if (parametros.PrecioMaximo.HasValue)
            query = query.Where(s => s.PrecioBase <= parametros.PrecioMaximo);
        var result = await query
            .Include(s => s.Reservas)
            .Include(s => s.DetalleServicios)
            .ToListAsync();

        return result.Select(s => new ReporteServicioDto
        {
            Id = s.Id,
            Nombre = s.Nombre,
            Descripcion = s.Descripcion,
            PrecioBase = s.PrecioBase,
            CantidadReservas = s.Reservas.Count,
            CantidadDetalles = s.DetalleServicios.Count,
            IngresosTotales = s.Reservas.Sum(r => r.PrecioTotal),
            UltimaReserva = s.Reservas.OrderByDescending(r => r.FechaRegistro).FirstOrDefault()?.FechaRegistro
        })
        .Where(s => !parametros.ReservasMinimas.HasValue || s.CantidadReservas >= parametros.ReservasMinimas);
    }
}