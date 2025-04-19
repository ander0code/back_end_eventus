using back_end.Core.Data;
using back_end.Modules.servicios.Models;
using Microsoft.EntityFrameworkCore;

namespace back_end.Modules.servicios.Repositories
{
    public interface IServicioRepository
    {
        Task<List<Servicio>> GetByCorreoAsync(string correo);
        Task<List<Servicio>> SearchServiciosAsync(string correo, string searchTerm);
        Task<Servicio?> GetByIdAsync(Guid id);
        Task<Servicio?> GetByIdAndCorreoAsync(Guid id, string correo);
        Task<Servicio?> CreateAsync(Servicio servicio);
        Task<Servicio?> UpdateAsync(Servicio servicio);
        Task<bool> DeleteAsync(Servicio servicio);
        
        // Métodos para gestionar los items del servicio (ServicioItem)
        Task<ServicioItem?> GetServicioItemByIdAsync(Guid id);
        Task<List<ServicioItem>> GetServicioItemsByServicioIdAsync(Guid servicioId);
        Task<ServicioItem?> AddServicioItemAsync(ServicioItem item);
        Task<ServicioItem?> UpdateServicioItemAsync(ServicioItem item);
        Task<bool> RemoveServicioItemAsync(ServicioItem item);
        Task<bool> RemoveMultipleServicioItemsAsync(IEnumerable<ServicioItem> items);
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
                .Include(s => s.ServicioItems)
                    .ThenInclude(si => si.Inventario)
                .Where(s => s.Usuario != null && s.Usuario.CorreoElectronico == correo)
                .ToListAsync();
        }
        
        public async Task<List<Servicio>> SearchServiciosAsync(string correo, string searchTerm)
        {
            return await _context.Servicios
                .Include(s => s.Usuario)
                .Include(s => s.ServicioItems)
                    .ThenInclude(si => si.Inventario)
                .Where(s => s.Usuario != null && 
                           s.Usuario.CorreoElectronico == correo &&
                           (s.Nombre.Contains(searchTerm) || 
                           (s.Descripcion != null && s.Descripcion.Contains(searchTerm)) ||
                           (s.Categoria != null && s.Categoria.Contains(searchTerm)))
                )
                .ToListAsync();
        }

        public async Task<Servicio?> GetByIdAsync(Guid id)
        {
            return await _context.Servicios
                .Include(s => s.Usuario)
                .Include(s => s.ServicioItems)
                    .ThenInclude(si => si.Inventario)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Servicio?> GetByIdAndCorreoAsync(Guid id, string correo)
        {
            return await _context.Servicios
                .Include(s => s.Usuario)
                .Include(s => s.ServicioItems)
                    .ThenInclude(si => si.Inventario)
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
            _context.Entry(servicio).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return servicio;
        }

        public async Task<bool> DeleteAsync(Servicio servicio)
        {
            try
            {
                // Primero eliminar los items relacionados para evitar violaciones de integridad referencial
                var items = await _context.ServicioItems
                    .Where(si => si.ServicioId == servicio.Id)
                    .ToListAsync();
                
                _context.ServicioItems.RemoveRange(items);
                
                // Luego eliminar el servicio
                _context.Servicios.Remove(servicio);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Implementación de métodos para gestionar los ServicioItems
        public async Task<ServicioItem?> GetServicioItemByIdAsync(Guid id)
        {
            return await _context.ServicioItems
                .Include(si => si.Inventario)
                .Include(si => si.Servicio)
                .FirstOrDefaultAsync(si => si.Id == id);
        }

        public async Task<List<ServicioItem>> GetServicioItemsByServicioIdAsync(Guid servicioId)
        {
            return await _context.ServicioItems
                .Include(si => si.Inventario)
                .Where(si => si.ServicioId == servicioId)
                .ToListAsync();
        }

        public async Task<ServicioItem?> AddServicioItemAsync(ServicioItem item)
        {
            _context.ServicioItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<ServicioItem?> UpdateServicioItemAsync(ServicioItem item)
        {
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> RemoveServicioItemAsync(ServicioItem item)
        {
            _context.ServicioItems.Remove(item);
            return await _context.SaveChangesAsync() > 0;
        }
        
        public async Task<bool> RemoveMultipleServicioItemsAsync(IEnumerable<ServicioItem> items)
        {
            _context.ServicioItems.RemoveRange(items);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}