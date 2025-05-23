using back_end.Core.Data;
using back_end.Modules.Item.Models;
using Microsoft.EntityFrameworkCore;

namespace back_end.Modules.Item.Repositories
{
    public interface IItemRepository
    {
        Task<List<Models.Item>> GetAllAsync();
        Task<Models.Item?> GetByIdAsync(Guid id);
        Task<List<Models.Item>> GetByCorreoAsync(string correo);
        Task<Models.Item> CreateAsync(Models.Item item);
        Task<Models.Item> UpdateAsync(Models.Item item);
        Task<bool> DeleteAsync(Models.Item item);
        Task<bool> ActualizarStockAsync(Guid id, int cantidad);
        Task<List<Models.Item>> GetByStockBelowMinAsync(int minStock);
    }

    public class ItemRepository : IItemRepository
    {
        private readonly DbEventusContext _context;

        public ItemRepository(DbEventusContext context)
        {
            _context = context;
        }

        public async Task<List<Models.Item>> GetAllAsync()
        {
            return await _context.Items
                .Include(i => i.DetalleServicios)
                .ToListAsync();
        }

        public async Task<Models.Item?> GetByIdAsync(Guid id)
        {
            return await _context.Items
                .Include(i => i.DetalleServicios)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<Models.Item>> GetByCorreoAsync(string correo)
        {
            // Suponiendo que necesitas relacionar ítems con usuarios a través de alguna relación
            // Esta parte podría requerir ajustes según la estructura de tu base de datos
            return await _context.Items
                .Include(i => i.DetalleServicios)
                .ToListAsync();
        }

        public async Task<Models.Item> CreateAsync(Models.Item item)
        {
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<Models.Item> UpdateAsync(Models.Item item)
        {
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> DeleteAsync(Models.Item item)
        {
            _context.Items.Remove(item);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ActualizarStockAsync(Guid id, int cantidad)
        {
            var item = await GetByIdAsync(id);
            if (item == null) return false;

            if (item.Stock == null)
                item.Stock = 0;

            item.Stock += cantidad;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Models.Item>> GetByStockBelowMinAsync(int minStock)
        {
            return await _context.Items
                .Where(i => i.Stock < minStock)
                .ToListAsync();
        }
    }
}