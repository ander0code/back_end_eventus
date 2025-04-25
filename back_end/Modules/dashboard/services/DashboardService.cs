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
        }

        public async Task<DashboardMetricsDTO> GetMetricsAsync(string correo)
        {

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.CorreoElectronico == correo);
            
            if (usuario == null)
                throw new Exception("Usuario no encontrado");

            var userId = usuario.Id;
            

            var fechaInicioDeMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var fechaFinDeMes = fechaInicioDeMes.AddMonths(1).AddDays(-1);

            var totalInventario = await _context.Inventarios
                .CountAsync(i => i.UsuarioId == userId);
                
            var eventosActivos = await _context.Reservas
                .CountAsync(r => r.UsuarioId == userId && r.Estado == "Confirmado" && r.FechaEvento >= DateOnly.FromDateTime(DateTime.Now));
                
            var totalClientes = await _context.Clientes
                .CountAsync(c => c.UsuarioId == userId);
                
            var cantidadServicios = await _context.Servicios
                .CountAsync(s => s.UsuarioId == userId);
            
            var reservasConfirmadas = await _context.Reservas
                .CountAsync(r => r.UsuarioId == userId && 
                                r.Estado == "Confirmado" && 
                                r.FechaEvento >= DateOnly.FromDateTime(fechaInicioDeMes) && 
                                r.FechaEvento <= DateOnly.FromDateTime(fechaFinDeMes));
                                
            var reservasPendientes = await _context.Reservas
                .CountAsync(r => r.UsuarioId == userId && 
                                r.Estado == "Pendiente" && 
                                r.FechaEvento >= DateOnly.FromDateTime(fechaInicioDeMes) && 
                                r.FechaEvento <= DateOnly.FromDateTime(fechaFinDeMes));
            
            var ingresosEstimados = await _context.Reservas
                .Where(r => r.UsuarioId == userId && 
                           r.Estado == "Confirmado" && 
                           r.FechaEvento >= DateOnly.FromDateTime(fechaInicioDeMes) && 
                           r.FechaEvento <= DateOnly.FromDateTime(fechaFinDeMes))
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
        }

        public async Task<ProximasReservasDTO> GetProximasReservasAsync(string correo, int cantidad = 4)
        {

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.CorreoElectronico == correo);
            
            if (usuario == null)
                throw new Exception("Usuario no encontrado");

            var userId = usuario.Id;
            
            var fechaActual = DateOnly.FromDateTime(DateTime.Now);
            
            var proximasReservas = await _context.Reservas
                .Where(r => r.UsuarioId == userId && 
                           (r.Estado == "Confirmado" || r.Estado == "Pendiente") && 
                           r.FechaEvento >= fechaActual)
                .OrderBy(r => r.FechaEvento)
                .ThenBy(r => r.HoraEvento)
                .Take(cantidad)
                .Select(r => new ProximaReservaDTO
                {
                    Id = r.Id,
                    NombreEvento = r.NombreEvento,
                    FechaEvento = r.FechaEvento,
                    HoraEvento = r.HoraEvento,
                    Descripcion = r.Descripcion
                })
                .ToListAsync();
            
            if (!proximasReservas.Any())
            {

                proximasReservas = await _context.Reservas
                    .Where(r => r.UsuarioId == userId && 
                              (r.Estado == "Confirmado" || r.Estado == "Pendiente") &&
                              r.FechaEvento >= fechaActual.AddDays(-7)) 
                    .OrderByDescending(r => r.FechaEvento)
                    .Take(cantidad)
                    .Select(r => new ProximaReservaDTO
                    {
                        Id = r.Id,
                        NombreEvento = r.NombreEvento,
                        FechaEvento = r.FechaEvento,
                        HoraEvento = r.HoraEvento,
                        Descripcion = r.Descripcion
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
                .FirstOrDefaultAsync(u => u.CorreoElectronico == correo);
            
            if (usuario == null)
                throw new Exception("Usuario no encontrado");

            var userId = usuario.Id;
            
            var todasLasActividades = new List<ActividadRecienteItemDTO>();

            var nuevasReservas = await _context.Reservas
                .Where(r => r.UsuarioId == userId)
                .OrderByDescending(r => r.Id) 
                .Take(cantidad)
                .Select(r => new ActividadRecienteItemDTO
                {
                    Id = r.Id,
                    Tipo = "Reserva",
                    Nombre = r.NombreEvento,
                    FechaRegistro = DateTime.Now.AddDays(-new Random().Next(1, 10)),
                    TiempoTranscurrido = "Hace poco" 
                })
                .ToListAsync();
            
            todasLasActividades.AddRange(nuevasReservas);
            
            var nuevosClientes = await _context.Clientes
                .Where(c => c.UsuarioId == userId)
                .OrderByDescending(c => c.FechaRegistro)
                .Take(cantidad)
                .Select(c => new ActividadRecienteItemDTO
                {
                    Id = c.Id,
                    Tipo = "Cliente",
                    Nombre = c.Nombre,
                    FechaRegistro = c.FechaRegistro ?? DateTime.Now,
                    TiempoTranscurrido = "Hace poco" 
                })
                .ToListAsync();
            
            todasLasActividades.AddRange(nuevosClientes);

            var eventosFinalizados = await _context.Reservas
                .Where(r => r.UsuarioId == userId && 
                           r.Estado == "Finalizado" || 
                           (r.FechaEvento < DateOnly.FromDateTime(DateTime.Now) && r.Estado == "Confirmado"))
                .OrderByDescending(r => r.FechaEvento)
                .Take(cantidad)
                .Select(r => new ActividadRecienteItemDTO
                {
                    Id = r.Id,
                    Tipo = "EventoFinalizado",
                    Nombre = r.NombreEvento,
                    FechaRegistro = DateTime.Now.AddDays(-new Random().Next(5, 30)), 
                    TiempoTranscurrido = "Hace poco" 
                })
                .ToListAsync();
            
            todasLasActividades.AddRange(eventosFinalizados);
            
            var nuevosItems = await _context.Inventarios
                .Where(i => i.UsuarioId == userId)
                .OrderByDescending(i => i.FechaRegistro)
                .Take(cantidad)
                .Select(i => new ActividadRecienteItemDTO
                {
                    Id = i.Id,
                    Tipo = "Item",
                    Nombre = i.Nombre,
                    FechaRegistro = i.FechaRegistro ?? DateTime.Now,
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