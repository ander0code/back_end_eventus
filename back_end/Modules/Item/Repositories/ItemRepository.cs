using back_end.Core.Data;
using back_end.Modules.Item.Models;
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
            // Si el ID es vacío, generamos un nuevo ID personalizado (convertido a Guid)
            if (item.Id == Guid.Empty)
            {
                // Generamos un ID personalizado
                string customId = IdGenerator.GenerateId("Item");
                // Creamos un hash determinístico basado en el ID personalizado
                Guid itemGuid = CreateDeterministicGuid(customId);
                item.Id = itemGuid;
            }
            
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }
        
        // Método auxiliar para crear un Guid determinístico a partir de un string
        private Guid CreateDeterministicGuid(string input)
        {
            // Usar MD5 para crear un hash determinístico (para este caso es aceptable)
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
            if (item.Stock < cantidad)
                return false;
                
            item.Stock -= cantidad;
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