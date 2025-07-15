using Microsoft.EntityFrameworkCore;
using back_end.Modules.reportes.DTOs;
using back_end.Modules.clientes.Models;
using back_end.Core.Data;

namespace back_end.Modules.reportes.Repositories;

public interface IClientesReporteRepository
{
    Task<IEnumerable<ClientesNuevosPorMesDto>> GetClientesNuevosPorMesAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<PromedioAdelantoPorClienteDto>> GetPromedioAdelantoPorClienteAsync(string? clienteId);
    Task<TasaRetencionClientesDto> GetTasaRetencionClientesAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<IEnumerable<DistribucionReservasPorClienteDto>> GetDistribucionReservasPorClienteAsync(DateTime? fechaInicio, DateTime? fechaFin);
}

public class ClientesReporteRepository : IClientesReporteRepository
{
    private readonly DbEventusContext _context;

    public ClientesReporteRepository(DbEventusContext context)
    {
        _context = context;
    }

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
                Anio = c.PrimeraReserva!.Value.Year, 
                Mes = c.PrimeraReserva!.Value.Month 
            })
            .Select(g => new ClientesNuevosPorMesDto
            {
                Anio = g.Key.Anio,
                Mes = g.Key.Mes,
                NombreMes = new DateTime(g.Key.Anio, g.Key.Mes, 1).ToString("MMMM"),
                CantidadClientesNuevos = g.Count()
            })
            .OrderBy(x => x.Anio).ThenBy(x => x.Mes);

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
                    .Average(r => Math.Round((r.Pagos.Sum(p => Convert.ToDecimal(p.Monto)) / r.PrecioTotal!.Value) * 100, 2)),
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
            PorcentajeMultiplesReservas = totalClientes > 0 ? Math.Round((decimal)clientesConMultiplesReservas / totalClientes * 100, 2) : 0,
            TasaRetencion = totalClientes > 0 ? Math.Round((decimal)clientesConMultiplesReservas / totalClientes * 100, 2) : 0
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
                r.FechaEjecucion.HasValue &&
                (!fechaInicio.HasValue || r.FechaEjecucion >= DateOnly.FromDateTime(fechaInicio.Value)) &&
                (!fechaFin.HasValue || r.FechaEjecucion <= DateOnly.FromDateTime(fechaFin.Value))))
            .Select(c => new DistribucionReservasPorClienteDto
            {
                ClienteId = c.Id,
                RazonSocial = c.RazonSocial,
                TotalReservas = c.Reservas.Count(r => 
                    r.FechaEjecucion.HasValue &&
                    (!fechaInicio.HasValue || r.FechaEjecucion >= DateOnly.FromDateTime(fechaInicio.Value)) &&
                    (!fechaFin.HasValue || r.FechaEjecucion <= DateOnly.FromDateTime(fechaFin.Value))),
                ReservasUltimosTresMeses = c.Reservas.Count(r => 
                    r.FechaEjecucion.HasValue &&
                    r.FechaEjecucion >= DateOnly.FromDateTime(fechaTresMesesAtras) &&
                    (!fechaFin.HasValue || r.FechaEjecucion <= DateOnly.FromDateTime(fechaFin.Value))),
                MontoTotalReservas = c.Reservas
                    .Where(r => r.FechaEjecucion.HasValue &&
                              (!fechaInicio.HasValue || r.FechaEjecucion >= DateOnly.FromDateTime(fechaInicio.Value)) &&
                              (!fechaFin.HasValue || r.FechaEjecucion <= DateOnly.FromDateTime(fechaFin.Value)))
                    .Sum(r => r.PrecioTotal ?? 0),
                UltimaReserva = c.Reservas
                    .Where(r => r.FechaEjecucion.HasValue &&
                              (!fechaInicio.HasValue || r.FechaEjecucion >= DateOnly.FromDateTime(fechaInicio.Value)) &&
                              (!fechaFin.HasValue || r.FechaEjecucion <= DateOnly.FromDateTime(fechaFin.Value)))
                    .OrderByDescending(r => r.FechaEjecucion)
                    .FirstOrDefault()?.FechaEjecucion?.ToDateTime(TimeOnly.MinValue)
            })
            .OrderByDescending(x => x.TotalReservas)
            .ToList();

        return resultado;
    }
}
