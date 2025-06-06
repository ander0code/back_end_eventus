using back_end.Core.Data;
using Microsoft.EntityFrameworkCore;
using back_end.Core.Utils;

namespace back_end.Modules.Item.Repositories
{    public interface IItemRepository
    {
        Task<List<Models.Item>> GetAllAsync();
        Task<Models.Item?> GetByIdAsync(Guid id);
        Task<Models.Item> CreateAsync(Models.Item item);
        Task<Models.Item> UpdateAsync(Models.Item item);
        Task<bool> DeleteAsync(Models.Item item);
        Task<bool> ActualizarStockAsync(Guid id, int cantidad);
        Task<bool> ReducirStockAsync(Guid id, int cantidad);
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

        public async Task<Models.Item> CreateAsync(Models.Item item)
        {
            if (item.Id == Guid.Empty)
            {
                // Generamos un ID personalizado
                string customId = IdGenerator.GenerateId("Item");
                Guid itemGuid = CreateDeterministicGuid(customId);
                item.Id = itemGuid;
            }
            
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        private Guid CreateDeterministicGuid(string input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                return new Guid(hash);
            }
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
        }        public async Task<bool> ActualizarStockAsync(Guid id, int cantidad)
        {
            var item = await GetByIdAsync(id);
            if (item == null) return false;

            if (item.Stock == null)
                item.Stock = 0;

            item.Stock += cantidad;
            
            // Actualizar StockDisponible
            var cantidadEnUso = item.DetalleServicios?.Sum(ds => ds.Cantidad) ?? 0;
            item.StockDisponible = (int)(item.Stock - cantidadEnUso);
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReducirStockAsync(Guid id, int cantidad)
        {
            var item = await GetByIdAsync(id);
            if (item == null) return false;

            if (item.Stock == null)
                item.Stock = 0;
                
            // Verificamos si hay suficiente stock
            if (item.StockDisponible < cantidad)
                return false;
                
            item.Stock -= cantidad;
    
            // Actualizar StockDisponible
            var cantidadEnUso = item.DetalleServicios?.Sum(ds => ds.Cantidad) ?? 0;
            item.StockDisponible = (int)(item.Stock - cantidadEnUso);
    
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