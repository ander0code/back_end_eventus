using back_end.Core.Data;
using back_end.Modules.servicios.Models;
using back_end.Modules.Item.Models;
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
        
        Task<DetalleServicio?> GetDetalleServicioByIdAsync(Guid id);
        Task<List<DetalleServicio>> GetDetalleServiciosByServicioIdAsync(Guid servicioId);
        Task<DetalleServicio?> AddDetalleServicioAsync(DetalleServicio detalle);
        Task<DetalleServicio?> UpdateDetalleServicioAsync(DetalleServicio detalle);
        Task<bool> RemoveDetalleServicioAsync(DetalleServicio detalle);
        Task<bool> RemoveMultipleDetalleServiciosAsync(IEnumerable<DetalleServicio> detalles);
        
        // Métodos para compatibilidad con el código antiguo
        Task<DetalleServicio?> GetServicioItemByIdAsync(Guid id);
        Task<List<DetalleServicio>> GetServicioItemsByServicioIdAsync(Guid servicioId);
        Task<DetalleServicio?> AddServicioItemAsync(DetalleServicio item);
        Task<DetalleServicio?> UpdateServicioItemAsync(DetalleServicio item);
        Task<bool> RemoveServicioItemAsync(DetalleServicio item);
        Task<bool> RemoveMultipleServicioItemsAsync(IEnumerable<DetalleServicio> items);
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
                .Include(s => s.DetalleServicios)
                    .ThenInclude(ds => ds.Inventario)
                .ToListAsync();
        }
        
        public async Task<List<Servicio>> SearchServiciosAsync(string correo, string searchTerm)
        {
            return await _context.Servicios
                .Include(s => s.DetalleServicios)
                    .ThenInclude(ds => ds.Inventario)
                .Where(s => s.Nombre != null && s.Nombre.Contains(searchTerm) || 
                           (s.Descripcion != null && s.Descripcion.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<Servicio?> GetByIdAsync(Guid id)
        {
            return await _context.Servicios
                .Include(s => s.DetalleServicios)
                    .ThenInclude(ds => ds.Inventario)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Servicio?> GetByIdAndCorreoAsync(Guid id, string correo)
        {
            return await _context.Servicios
                .Include(s => s.DetalleServicios)
                    .ThenInclude(ds => ds.Inventario)
                .FirstOrDefaultAsync(s => s.Id == id);
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
                var detalles = await _context.DetalleServicios
                    .Where(ds => ds.ServicioId == servicio.Id)
                    .ToListAsync();
                
                _context.DetalleServicios.RemoveRange(detalles);

                _context.Servicios.Remove(servicio);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<DetalleServicio?> GetDetalleServicioByIdAsync(Guid id)
        {
            return await _context.DetalleServicios
                .Include(ds => ds.Inventario)
                .Include(ds => ds.Servicio)
                .FirstOrDefaultAsync(ds => ds.Id == id);
        }

        public async Task<List<DetalleServicio>> GetDetalleServiciosByServicioIdAsync(Guid servicioId)
        {
            return await _context.DetalleServicios
                .Include(ds => ds.Inventario)
                .Where(ds => ds.ServicioId == servicioId)
                .ToListAsync();
        }

        public async Task<DetalleServicio?> AddDetalleServicioAsync(DetalleServicio detalle)
        {
            _context.DetalleServicios.Add(detalle);
            await _context.SaveChangesAsync();
            return detalle;
        }

        public async Task<DetalleServicio?> UpdateDetalleServicioAsync(DetalleServicio detalle)
        {
            _context.Entry(detalle).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return detalle;
        }

        public async Task<bool> RemoveDetalleServicioAsync(DetalleServicio detalle)
        {
            _context.DetalleServicios.Remove(detalle);
            return await _context.SaveChangesAsync() > 0;
        }
        
        public async Task<bool> RemoveMultipleDetalleServiciosAsync(IEnumerable<DetalleServicio> detalles)
        {
            _context.DetalleServicios.RemoveRange(detalles);
            return await _context.SaveChangesAsync() > 0;
        }
        
        // Implementaciones para mantener compatibilidad con código antiguo
        public Task<DetalleServicio?> GetServicioItemByIdAsync(Guid id)
        {
            return GetDetalleServicioByIdAsync(id);
        }

        public Task<List<DetalleServicio>> GetServicioItemsByServicioIdAsync(Guid servicioId)
        {
            return GetDetalleServiciosByServicioIdAsync(servicioId);
        }

        public Task<DetalleServicio?> AddServicioItemAsync(DetalleServicio item)
        {
            return AddDetalleServicioAsync(item);
        }

        public Task<DetalleServicio?> UpdateServicioItemAsync(DetalleServicio item)
        {
            return UpdateDetalleServicioAsync(item);
        }

        public Task<bool> RemoveServicioItemAsync(DetalleServicio item)
        {
            return RemoveDetalleServicioAsync(item);
        }
        
        public Task<bool> RemoveMultipleServicioItemsAsync(IEnumerable<DetalleServicio> items)
        {
            return RemoveMultipleDetalleServiciosAsync(items);
        }
    }
}