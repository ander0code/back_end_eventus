using back_end.Core.Data;
using back_end.Modules.reservas.Models;
using Microsoft.EntityFrameworkCore;

namespace back_end.Modules.reservas.Repositories
{
    public interface IReservaRepository
    {
        Task<List<Reserva>> GetByCorreoUsuarioAsync(string correo);
        Task<Reserva?> GetByIdAndCorreoAsync(Guid id, string correo);
        Task<Reserva> CreateAsync(Reserva reserva);
        Task<Reserva> UpdateAsync(Reserva reserva);
        Task<bool> DeleteAsync(Reserva reserva);
        
        Task<ReservaServicio?> AddReservaServicioAsync(ReservaServicio reservaServicio);
        Task<ReservaServicio?> UpdateReservaServicioAsync(ReservaServicio reservaServicio);
        Task<bool> RemoveReservaServicioAsync(ReservaServicio reservaServicio);
        Task<List<ReservaServicio>> GetReservaServiciosByReservaIdAsync(Guid reservaId);
    }

    public class ReservaRepository : IReservaRepository
    {
        private readonly DbEventusContext _context;

        public ReservaRepository(DbEventusContext context)
        {
            _context = context;
        }

        public async Task<List<Reserva>> GetByCorreoUsuarioAsync(string correo)
        {
            return await _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Cliente)
                .Include(r => r.ReservaServicios)
                    .ThenInclude(rs => rs.Servicio)
                .Where(r => r.Usuario.CorreoElectronico == correo)
                .ToListAsync();
        }

        public async Task<Reserva?> GetByIdAndCorreoAsync(Guid id, string correo)
        {
            return await _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Cliente)
                .Include(r => r.ReservaServicios)
                    .ThenInclude(rs => rs.Servicio)
                .FirstOrDefaultAsync(r => r.Id == id && r.Usuario.CorreoElectronico == correo);
        }

        public async Task<Reserva> CreateAsync(Reserva reserva)
        {
            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();
            return reserva;
        }

        public async Task<Reserva> UpdateAsync(Reserva reserva)
        {
            _context.Entry(reserva).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return reserva;
        }

        public async Task<bool> DeleteAsync(Reserva reserva)
        {
            _context.Reservas.Remove(reserva);
            return await _context.SaveChangesAsync() > 0;
        }

        
        public async Task<ReservaServicio?> AddReservaServicioAsync(ReservaServicio reservaServicio)
        {
            _context.ReservaServicios.Add(reservaServicio);
            await _context.SaveChangesAsync();
            return reservaServicio;
        }
        
        public async Task<ReservaServicio?> UpdateReservaServicioAsync(ReservaServicio reservaServicio)
        {
            _context.Entry(reservaServicio).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return reservaServicio;
        }
        
        public async Task<bool> RemoveReservaServicioAsync(ReservaServicio reservaServicio)
        {
            _context.ReservaServicios.Remove(reservaServicio);
            return await _context.SaveChangesAsync() > 0;
        }
        
        public async Task<List<ReservaServicio>> GetReservaServiciosByReservaIdAsync(Guid reservaId)
        {
            return await _context.ReservaServicios
                .Include(rs => rs.Servicio)
                .Where(rs => rs.ReservaId == reservaId)
                .ToListAsync();
        }
    }
}
