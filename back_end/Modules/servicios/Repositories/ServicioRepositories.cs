using back_end.Core.Data;
using back_end.Modules.servicios.Models;
using Microsoft.EntityFrameworkCore;

namespace back_end.Modules.servicios.Repositories
{
    public interface IServicioRepository
    {
        Task<List<Servicio>> GetByCorreoAsync(string correo);
        Task<Servicio?> CreateAsync(Servicio servicio);
        Task<Servicio?> UpdateAsync(Servicio servicio);
        Task<bool> DeleteAsync(Servicio servicio);
        Task<Servicio?> GetByIdAndCorreoAsync(Guid id, string correo);
    }

    public class ServicioRepository : IServicioRepository
    {
        private readonly DbEventusContext _context;

        public ServicioRepository(DbEventusContext context)
        {
            _context = context;
        }

        public async Task<List<Servicio>> GetByCorreoAsync(string correo)
        {
            return await _context.Servicios
                .Include(s => s.Usuario)
                .Where(s => s.Usuario != null && s.Usuario.CorreoElectronico == correo)
                .ToListAsync();
        }

        public async Task<Servicio?> GetByIdAndCorreoAsync(Guid id, string correo)
        {
            return await _context.Servicios
                .Include(s => s.Usuario)
                .FirstOrDefaultAsync(s => s.Id == id && s.Usuario != null && s.Usuario.CorreoElectronico == correo);
        }

        public async Task<Servicio?> CreateAsync(Servicio servicio)
        {
            _context.Servicios.Add(servicio);
            await _context.SaveChangesAsync();
            return servicio;
        }

        public async Task<Servicio?> UpdateAsync(Servicio servicio)
        {
            _context.Servicios.Update(servicio);
            await _context.SaveChangesAsync();
            return servicio;
        }

        public async Task<bool> DeleteAsync(Servicio servicio)
        {
            _context.Servicios.Remove(servicio);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}