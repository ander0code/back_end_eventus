using back_end.Core.Data;
using Microsoft.EntityFrameworkCore;
using back_end.Core.Utils;

namespace back_end.Modules.Item.Repositories
{    public interface IItemRepository
    {
        Task<List<Models.Item>> GetAllAsync();
        Task<Models.Item?> GetByIdAsync(string id);
        Task<Models.Item> CreateAsync(Models.Item item);
        Task<Models.Item> UpdateAsync(Models.Item item);
        Task<bool> DeleteAsync(Models.Item item);
        Task<bool> ActualizarStockAsync(string id, int cantidad);
        Task<bool> ReducirStockAsync(string id, int cantidad);
        Task<List<Models.Item>> GetByStockBelowMinAsync(int minStock);
        Task<int> ContarReservasUsandoServicioAsync(string servicioId);
        Task<int> ContarReservasActivasUsandoServicioAsync(string servicioId);
        
        // Métodos optimizados faltantes
        Task<List<Models.Item>> GetItemsConStockEnUsoAsync(List<string> itemsIds);
        Task<bool> UpdateBatchAsync(List<Models.Item> items);
        Task<(bool esValido, string mensaje, Models.Item? item, int stockDisponibleActual)> ValidarStockParaDetalleAsync(string itemId, int cantidadRequerida);
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
        public async Task<Models.Item?> GetByIdAsync(string id)
        {
            return await _context.Items
                .Include(i => i.DetalleServicios)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Models.Item> CreateAsync(Models.Item item)
        {
            if (string.IsNullOrEmpty(item.Id))
            {
                item.Id = IdGenerator.GenerateId("Item");
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
            // Verificar si el item está relacionado con algún DetalleServicio
            var tieneRelacion = await _context.DetalleServicios.AnyAsync(ds => ds.InventarioId == item.Id);
            if (tieneRelacion)
            {
                // No permitir la eliminación si está relacionado
                return false;
            }

            _context.Items.Remove(item);
            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException)
            {
                // Loguea el error si quieres
                // _logger.LogError(ex, "Error al eliminar item con ID {Id}", item.Id);
                return false;
            }
        }
        public async Task<bool> ActualizarStockAsync(string id, int cantidad)
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

        public async Task<bool> ReducirStockAsync(string id, int cantidad)
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

        public async Task<int> ContarReservasUsandoServicioAsync(string servicioId)
        {
            try
            {
                // Contar cuántas reservas activas están usando este servicio
                var count = await _context.Reservas
                    .Where(r => r.ServicioId == servicioId &&
                               (r.Estado == null ||
                                (r.Estado.ToLower() != "finalizado" &&
                                 r.Estado.ToLower() != "cancelada" &&
                                 r.Estado.ToLower() != "cancelado")))
                    .CountAsync();

                return count;
            }
            catch
            {
                return 1;
            }
        }
        
                public async Task<int> ContarReservasActivasUsandoServicioAsync(string servicioId)
        {
            try
            {
                // Contar cuántas reservas ACTIVAS están usando este servicio (excluyendo Finalizadas y Canceladas)
                var count = await _context.Reservas
                    .Where(r => r.ServicioId == servicioId && 
                               (r.Estado == null || 
                                (r.Estado.ToLower() != "finalizado" && 
                                 r.Estado.ToLower() != "cancelada" && 
                                 r.Estado.ToLower() != "cancelado")))
                    .CountAsync();
                
                return count;
            }
            catch
            {
                return 1;
            }
        }

        public async Task<List<Models.Item>> GetItemsConStockEnUsoAsync(List<string> itemsIds)
        {
            try
            {
                // Una sola consulta para obtener todos los items con su stock en uso calculado
                var items = await _context.Items
                    .Where(i => itemsIds.Contains(i.Id))
                    .Include(i => i.DetalleServicios)
                        .ThenInclude(ds => ds.Servicio)
                            .ThenInclude(s => s!.Reservas.Where(r => r.Estado == null || 
                                (r.Estado.ToLower() != "finalizado" && 
                                 r.Estado.ToLower() != "cancelada" && 
                                 r.Estado.ToLower() != "cancelado")))
                    .ToListAsync();

                // Calcular la cantidad en uso para cada item
                foreach (var item in items)
                {
                    var cantidadEnUso = 0.0;
                    var serviciosUsados = item.DetalleServicios
                        .Where(ds => !string.IsNullOrEmpty(ds.ServicioId))
                        .GroupBy(ds => ds.ServicioId!)
                        .ToList();

                    foreach (var servicioGroup in serviciosUsados)
                    {
                        var cantidadPorServicio = servicioGroup.Sum(ds => ds.Cantidad ?? 0);
                        var reservasActivas = servicioGroup.First().Servicio?.Reservas?.Count() ?? 0;
                        cantidadEnUso += cantidadPorServicio * reservasActivas;
                    }

                    // Asignar el valor calculado temporalmente
                    item.CantidadEnUso = cantidadEnUso;
                }

                return items;
            }
            catch (Exception ex)
            {
                return new List<Models.Item>();
            }
        }

        public async Task<bool> UpdateBatchAsync(List<Models.Item> items)
        {
            try
            {
                foreach (var item in items)
                {
                    _context.Entry(item).State = EntityState.Modified;
                }
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<(bool esValido, string mensaje, Models.Item? item, int stockDisponibleActual)> ValidarStockParaDetalleAsync(string itemId, int cantidadRequerida)
        {
            try
            {
                var item = await _context.Items
                    .Include(i => i.DetalleServicios)
                        .ThenInclude(ds => ds.Servicio)
                            .ThenInclude(s => s!.Reservas.Where(r => r.Estado == null || 
                                (r.Estado.ToLower() != "finalizado" && 
                                 r.Estado.ToLower() != "cancelada" && 
                                 r.Estado.ToLower() != "cancelado")))
                    .FirstOrDefaultAsync(i => i.Id == itemId);

                if (item == null)
                {
                    return (false, "El item especificado no existe", null, 0);
                }

                // Calcular stock disponible actual
                var stockDisponible = item.StockDisponible;

                if (stockDisponible < cantidadRequerida)
                {
                    return (false, $"Stock insuficiente. Disponible: {stockDisponible}, Requerido: {cantidadRequerida}", item, stockDisponible);
                }

                return (true, "Stock suficiente", item, stockDisponible);
            }
            catch (Exception ex)
            {
                return (false, "Error al validar stock", null, 0);
            }
        }
    }
}