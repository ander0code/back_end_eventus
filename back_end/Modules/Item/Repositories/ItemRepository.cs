using back_end.Core.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ItemModel = back_end.Modules.Item.Models.Item; // Usar un alias para evitar la confusión con el namespace

namespace back_end.Modules.Item.Repositories
{    
    public interface IItemRepository
    {
        Task<List<Models.Item>> GetAllAsync();
        Task<Models.Item?> GetByIdAsync(string id);
        Task<Models.Item?> CreateAsync(Models.Item item);
        Task<Models.Item?> UpdateAsync(Models.Item item);
        Task<bool> DeleteAsync(Models.Item item);
        Task<bool> ActualizarStockAsync(string id, int newStock);
        Task<bool> ActualizarStockDisponibleAsync(string id, int nuevoStockDisponible);
        Task<List<Models.Item>> GetItemsConStockEnUsoAsync(List<string> itemsIds);
        Task<bool> UpdateBatchAsync(List<Models.Item> items);
        Task<int> ContarReservasUsandoServicioAsync(string servicioId);
        Task<int> ContarReservasActivasUsandoServicioAsync(string servicioId);
    }

    public class ItemRepository : IItemRepository
    {
        private readonly DbEventusContext _context;
        private readonly ILogger<ItemRepository> _logger;

        public ItemRepository(DbEventusContext context, ILogger<ItemRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public async Task<List<Models.Item>> GetAllAsync()
        {
            try
            {
                return await _context.Items
                    .Include(i => i.DetalleServicios)
                        .ThenInclude(ds => ds.Servicio)
                    .OrderByDescending(i => i.Id) // agregado
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los items");
                throw;
            }
        }
        
        public async Task<Models.Item?> GetByIdAsync(string id)
        {
            try
            {
                return await _context.Items
                    .Include(i => i.DetalleServicios)
                        .ThenInclude(ds => ds.Servicio)
                    .FirstOrDefaultAsync(i => i.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener item con ID {Id}", id);
                throw;
            }
        }
        
        public async Task<Models.Item?> CreateAsync(Models.Item item)
        {
            try
            {
                _context.Items.Add(item);
                await _context.SaveChangesAsync();
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear item {ItemName}", item.Nombre);
                throw;
            }
        }
        
        public async Task<Models.Item?> UpdateAsync(Models.Item item)
        {
            try
            {
                _context.Entry(item).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar item con ID {ItemId}", item.Id);
                throw;
            }
        }
        
        public async Task<bool> DeleteAsync(Models.Item item)
        {
            try
            {
                _context.Items.Remove(item);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar item con ID {ItemId}", item.Id);
                throw;
            }
        }
        
        public async Task<bool> ActualizarStockAsync(string id, int newStock)
        {
            try
            {
                var item = await _context.Items.FindAsync(id);
                if (item == null) return false;

                item.Stock = newStock;
                item.StockDisponible = newStock;
                _context.Items.Update(item);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        public async Task<bool> ActualizarStockDisponibleAsync(string id, int nuevoStockDisponible)
        {
            try
            {
                var item = await _context.Items.FindAsync(id);
                if (item == null) return false;

                item.StockDisponible = nuevoStockDisponible;
                _context.Items.Update(item);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        public async Task<List<Models.Item>> GetItemsConStockEnUsoAsync(List<string> itemsIds)
        {
            try
            {
                return await _context.Items
                    .Include(i => i.DetalleServicios)
                        .ThenInclude(ds => ds.Servicio)
                    // Asegurarnos de que el ID no sea nulo antes de verificar si está en la lista
                    .Where(i => i.Id != null && itemsIds.Contains(i.Id))
                    .ToListAsync();
            }
            catch
            {
                return new List<Models.Item>();
            }
        }
        
        public async Task<bool> UpdateBatchAsync(List<Models.Item> items)
        {
            try
            {
                _context.Items.UpdateRange(items);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        public async Task<int> ContarReservasUsandoServicioAsync(string servicioId)
        {
            try
            {
                // Contar todas las reservas que usan este servicio
                return await _context.Reservas
                    .Where(r => r.ServicioId == servicioId)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar reservas que usan el servicio {ServicioId}", servicioId);
                return 0;
            }
        }
        
        public async Task<int> ContarReservasActivasUsandoServicioAsync(string servicioId)
        {
            try
            {
                // Contar sólo reservas ACTIVAS (pendientes o confirmadas)
                // Excluye las reservas finalizadas o canceladas que ya no afectan al stock
                var estadosActivos = new[] { "pendiente", "confirmado", "confirmada", "pendiente de pago" };
                
                return await _context.Reservas
                    .Where(r => r.ServicioId == servicioId && 
                           r.Estado != null && 
                           estadosActivos.Contains(r.Estado.ToLower()))
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar reservas activas que usan el servicio {ServicioId}", servicioId);
                return 0;
            }
        }
    }
}