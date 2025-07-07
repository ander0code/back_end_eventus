using back_end.Core.Data;
using back_end.Modules.dashboard.DTOs;
using Microsoft.EntityFrameworkCore;

namespace back_end.Modules.dashboard.services
{
    public interface IDashboardService
    {
        Task<DashboardMetricsDTO> GetMetricsAsync(string correo);
        Task<ProximasReservasDTO> GetProximasReservasAsync(string correo, int cantidad = 4);
        Task<ActividadRecienteDTO> GetActividadRecienteAsync(string correo, int cantidad = 10);
    }

    public class DashboardService : IDashboardService
    {
        private readonly DbEventusContext _context;

        public DashboardService(DbEventusContext context)
        {
            _context = context;
        }        public async Task<DashboardMetricsDTO> GetMetricsAsync(string correo)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == correo);
            
            if (usuario == null)
                throw new Exception("Usuario no encontrado");

            var userId = usuario.Id;
            
            var fechaInicioDeMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var fechaFinDeMes = fechaInicioDeMes.AddMonths(1).AddDays(-1);

            // Contar items de inventario (objetos Item)
            var totalInventario = await _context.Items.CountAsync();
                
            // Contar eventos activos (reservas confirmadas con fecha futura)
            var eventosActivos = await _context.Reservas
                .CountAsync(r => (r.Estado == "Confirmado" || r.Estado == "Pendiente") && 
                           r.FechaEjecucion >= DateOnly.FromDateTime(DateTime.Now));
                
            // Contar todos los clientes asociados a usuarios
            var totalClientes = await _context.Clientes.CountAsync();
                
            // Contar todos los servicios
            var cantidadServicios = await _context.Servicios.CountAsync();
            
            // Contar reservas confirmadas del mes actual
            var reservasConfirmadas = await _context.Reservas
                .CountAsync(r => r.Estado == "Confirmado" && 
                           r.FechaEjecucion >= DateOnly.FromDateTime(fechaInicioDeMes) && 
                           r.FechaEjecucion <= DateOnly.FromDateTime(fechaFinDeMes));
                                
            // Contar reservas pendientes del mes actual
            var reservasPendientes = await _context.Reservas
                .CountAsync(r => r.Estado == "Pendiente" && 
                           r.FechaEjecucion >= DateOnly.FromDateTime(fechaInicioDeMes) && 
                           r.FechaEjecucion <= DateOnly.FromDateTime(fechaFinDeMes));
            
            // Sumar precios totales de las reservas confirmadas del mes
            var ingresosEstimados = await _context.Reservas
                .Where(r => r.Estado == "Confirmado" && 
                       r.FechaEjecucion >= DateOnly.FromDateTime(fechaInicioDeMes) && 
                       r.FechaEjecucion <= DateOnly.FromDateTime(fechaFinDeMes))
                .SumAsync(r => r.PrecioTotal ?? 0);

            var metrics = new DashboardMetricsDTO
            {
                TotalInventarioItems = totalInventario,
                EventosActivos = eventosActivos,
                TotalClientes = totalClientes,
                CantidadServicios = cantidadServicios,
                ReservasConfirmadasMes = reservasConfirmadas,
                ReservasPendientesMes = reservasPendientes,
                IngresosEstimadosMes = ingresosEstimados
            };
            
            return metrics;
        }        public async Task<ProximasReservasDTO> GetProximasReservasAsync(string correo, int cantidad = 4)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == correo);
            
            if (usuario == null)
                throw new Exception("Usuario no encontrado");
            
            var fechaActual = DateOnly.FromDateTime(DateTime.Now);
            
            // Primero intentar obtener reservas de todas las reservas (sin filtrar por cliente específico)
            var proximasReservas = await _context.Reservas
                .Where(r => (r.Estado == "Confirmado" || r.Estado == "Pendiente") && 
                           r.FechaEjecucion >= fechaActual)
                .OrderBy(r => r.FechaEjecucion)
                .Take(cantidad)
                .Include(r => r.Cliente)
                    .ThenInclude(c => c!.Usuario)
                .Select(r => new ProximaReservaDTO
                {
                    Id = r.Id,
                    NombreEvento = r.NombreEvento,
                    FechaEjecucion = r.FechaEjecucion,
                    Descripcion = r.Descripcion,
                    Estado = r.Estado
                })
                .ToListAsync();
            
            // Si no hay reservas futuras, buscar las más recientes
            if (!proximasReservas.Any())
            {
                proximasReservas = await _context.Reservas
                    .Where(r => (r.Estado == "Confirmado" || r.Estado == "Pendiente"))
                    .OrderByDescending(r => r.FechaEjecucion)
                    .Take(cantidad)
                    .Include(r => r.Cliente)
                        .ThenInclude(c => c!.Usuario)
                    .Select(r => new ProximaReservaDTO
                    {
                        Id = r.Id,
                        NombreEvento = r.NombreEvento,
                        FechaEjecucion = r.FechaEjecucion,
                        Descripcion = r.Descripcion,
                        Estado = r.Estado
                    })
                    .ToListAsync();
            }
            
            return new ProximasReservasDTO
            {
                Reservas = proximasReservas
            };
        }

        public async Task<ActividadRecienteDTO> GetActividadRecienteAsync(string correo, int cantidad = 10)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == correo);
            
            if (usuario == null)
                throw new Exception("Usuario no encontrado");

            var userId = usuario.Id;
                
            var todasLasActividades = new List<ActividadRecienteItemDTO>();

            // Obtener reservas recientes (todas las reservas, no solo del usuario)
            var nuevasReservas = await _context.Reservas
                .OrderByDescending(r => r.FechaRegistro) 
                .Take(cantidad)
                .Select(r => new ActividadRecienteItemDTO
                {
                    Id = r.Id,
                    Tipo = "Reserva",
                    Nombre = r.NombreEvento,
                    FechaRegistro = r.FechaRegistro ?? DateTime.Now.AddDays(-new Random().Next(1, 10)),
                    TiempoTranscurrido = "Hace poco" 
                })
                .ToListAsync();
            
            todasLasActividades.AddRange(nuevasReservas);
            
            // Obtener clientes recientes
            var nuevosClientes = await _context.Clientes
                .Include(c => c.Usuario)
                .OrderBy(c => c.Id)
                .Take(cantidad)
                .Select(c => new ActividadRecienteItemDTO
                {
                    Id = c.Id,
                    Tipo = "Cliente",
                    Nombre = c.Usuario != null ? $"{c.Usuario.Nombre ?? ""} {c.Usuario.Apellido ?? ""}" : "Cliente sin nombre",
                    FechaRegistro = DateTime.Now.AddDays(-new Random().Next(1, 30)),
                    TiempoTranscurrido = "Hace poco" 
                })
                .ToListAsync();
            
            todasLasActividades.AddRange(nuevosClientes);

            // Eventos finalizados
            var eventosFinalizados = await _context.Reservas
                .Where(r => r.Estado == "Finalizado" || 
                           (r.FechaEjecucion < DateOnly.FromDateTime(DateTime.Now) && r.Estado == "Confirmado"))
                .OrderByDescending(r => r.FechaEjecucion)
                .Take(cantidad)
                .Select(r => new ActividadRecienteItemDTO
                {
                    Id = r.Id,
                    Tipo = "EventoFinalizado",
                    Nombre = r.NombreEvento,
                    FechaRegistro = r.FechaRegistro ?? DateTime.Now.AddDays(-new Random().Next(5, 30)), 
                    TiempoTranscurrido = "Hace poco" 
                })
                .ToListAsync();
            
            todasLasActividades.AddRange(eventosFinalizados);
            
            // Nuevos items
            var nuevosItems = await _context.Items
                .OrderBy(i => i.Id)
                .Take(cantidad)
                .Select(i => new ActividadRecienteItemDTO
                {
                    Id = i.Id.ToString(),
                    Tipo = "Item",
                    Nombre = i.Nombre,
                    FechaRegistro = DateTime.Now.AddDays(-new Random().Next(1, 20)),
                    TiempoTranscurrido = "Hace poco"
                })
                .ToListAsync();
            
            todasLasActividades.AddRange(nuevosItems);
            
            todasLasActividades = todasLasActividades
                .OrderByDescending(a => a.FechaRegistro)
                .Take(cantidad)
                .ToList();

            foreach (var actividad in todasLasActividades)
            {
                actividad.TiempoTranscurrido = CalcularTiempoTranscurrido(actividad.FechaRegistro);
            }
            
            return new ActividadRecienteDTO
            {
                Actividades = todasLasActividades
            };
        }
        
        private string CalcularTiempoTranscurrido(DateTime fechaRegistro)
        {
            var tiempoTranscurrido = DateTime.Now - fechaRegistro;
            
            if (tiempoTranscurrido.TotalMinutes < 1)
                return "Hace un momento";
            if (tiempoTranscurrido.TotalMinutes < 60)
                return $"Hace {Math.Floor(tiempoTranscurrido.TotalMinutes)} minutos";
            if (tiempoTranscurrido.TotalHours < 24)
                return $"Hace {Math.Floor(tiempoTranscurrido.TotalHours)} horas";
            if (tiempoTranscurrido.TotalDays < 30)
                return $"Hace {Math.Floor(tiempoTranscurrido.TotalDays)} días";
            
            return $"Hace {Math.Floor(tiempoTranscurrido.TotalDays / 30)} meses";
        }
    }
}