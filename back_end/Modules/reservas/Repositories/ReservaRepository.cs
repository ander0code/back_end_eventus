using back_end.Core.Data;
using back_end.Modules.reservas.Models;
using Microsoft.EntityFrameworkCore;

namespace back_end.Modules.reservas.Repositories
{
    public interface IReservaRepository
    {
        Task<List<Reserva>> GetByCorreoUsuarioAsync(string correo);
        Task<Reserva?> GetByIdAndCorreoAsync(int id, string correo);
        Task<Reserva> CreateAsync(Reserva reserva);
        Task<Reserva> UpdateAsync(Reserva reserva);
        Task<bool> DeleteAsync(Reserva reserva);
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
                .Where(r => r.Usuario!.Correo == correo)
                .ToListAsync();
        }

        public async Task<Reserva?> GetByIdAndCorreoAsync(int id, string correo)
        {
            return await _context.Reservas
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.Id == id && r.Usuario!.Correo == correo);
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
    }
}
