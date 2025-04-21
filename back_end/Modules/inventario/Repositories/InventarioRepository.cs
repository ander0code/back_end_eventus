using back_end.Core.Data;
using back_end.Modules.inventario.Models;
using Microsoft.EntityFrameworkCore;

namespace back_end.Modules.inventario.Repositories
{
    public interface IInventarioRepository
    {
        Task<List<Inventario>> GetAllAsync();
        Task<Inventario?> GetByIdAsync(Guid id);
        Task<List<Inventario>> GetByUsuarioIdAsync(Guid usuarioId);
        Task<List<Inventario>> GetByCorreoAsync(string correo);
        Task<Inventario?> CreateAsync(Inventario inventario);
        Task<Inventario?> UpdateAsync(Inventario inventario);
        Task<bool> DeleteAsync(Inventario inventario);
        Task<bool> ActualizarStockAsync(Guid id, int cantidad);
        Task<List<Inventario>> SearchByNameOrCategoryAsync(string searchTerm);
        Task<List<Inventario>> GetByStockBelowMinAsync(int minStock);
    }

    public class InventarioRepository : IInventarioRepository
    {
        private readonly DbEventusContext _context;

        public InventarioRepository(DbEventusContext context)
        {
            _context = context;
        }

        public async Task<List<Inventario>> GetAllAsync()
        {
            return await _context.Inventarios
                .Include(i => i.Usuario)
                .ToListAsync();
        }

        public async Task<Inventario?> GetByIdAsync(Guid id)
        {
            return await _context.Inventarios
                .Include(i => i.Usuario)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<Inventario>> GetByUsuarioIdAsync(Guid usuarioId)
        {
            return await _context.Inventarios
                .Include(i => i.Usuario)
                .Where(i => i.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<List<Inventario>> GetByCorreoAsync(string correo)
        {
            return await _context.Inventarios
                .Include(i => i.Usuario)
                .Where(i => i.Usuario.CorreoElectronico == correo)
                .ToListAsync();
        }

        public async Task<Inventario?> CreateAsync(Inventario inventario)
        {
            _context.Inventarios.Add(inventario);
            await _context.SaveChangesAsync();
            return inventario;
        }

        public async Task<Inventario?> UpdateAsync(Inventario inventario)
        {
            _context.Entry(inventario).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return inventario;
        }

        public async Task<bool> DeleteAsync(Inventario inventario)
        {
            // Primero eliminar todos los ServicioItem asociados al elemento de inventario
            var servicioItems = await _context.ServicioItems
                .Where(si => si.InventarioId == inventario.Id)
                .ToListAsync();

            if (servicioItems.Any())
            {
                _context.ServicioItems.RemoveRange(servicioItems);
                // Guardar cambios para eliminar los items de servicio primero
                await _context.SaveChangesAsync();
            }

            // Ahora proceder a eliminar el elemento del inventario
            _context.Inventarios.Remove(inventario);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ActualizarStockAsync(Guid id, int cantidad)
        {
            var inventario = await _context.Inventarios.FindAsync(id);
            if (inventario == null) return false;
            
            inventario.Stock = cantidad;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Inventario>> SearchByNameOrCategoryAsync(string searchTerm)
        {
            return await _context.Inventarios
                .Include(i => i.Usuario)
                .Where(i => i.Nombre.Contains(searchTerm) || 
                           (i.Categoria != null && i.Categoria.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<List<Inventario>> GetByStockBelowMinAsync(int minStock)
        {
            return await _context.Inventarios
                .Include(i => i.Usuario)
                .Where(i => i.Stock.HasValue && i.Stock < minStock)
                .ToListAsync();
        }
    }
}