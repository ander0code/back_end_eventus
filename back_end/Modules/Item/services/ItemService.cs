using back_end.Modules.Item.DTOs;
using back_end.Modules.Item.Repositories;
using back_end.Core.Utils;

namespace back_end.Modules.Item.Services
{
    public interface IItemService
    {
        Task<List<ItemResponseDTO>> GetAllAsync();
        Task<ItemResponseDTO?> GetByIdAsync(string id);
        Task<ItemResponseDTO?> CreateAsync(ItemCreateDTO dto);
        Task<ItemResponseDTO?> UpdateAsync(string id, ItemUpdateDTO dto);
        Task<bool> DeleteAsync(string id);
        Task<List<ItemResponseDTO>> SearchByNameAsync(string term);
        Task<bool> UpdateStockAsync(string id, int newStock);
        Task<List<ItemListResponseDTO>> GetAllWithAvailabilityAsync();
        Task<bool> RecalcularStockDisponibleAsync(string id);
        Task<bool> RecalcularStockDisponibleBatchAsync(List<string> itemsIds);
    }

    public class ItemService : IItemService
    {
        private readonly IItemRepository _repository;
        private readonly ILogger<ItemService> _logger;

        public ItemService(IItemRepository repository, ILogger<ItemService> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        
        ///  Obtener todos los items
        public async Task<List<ItemResponseDTO>> GetAllAsync()
        {
            try
            {
                var items = await _repository.GetAllAsync();
                return items.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los items");
                return new List<ItemResponseDTO>();
            }
        }

        public async Task<ItemResponseDTO?> GetByIdAsync(string id)
        {
            try
            {
                var item = await _repository.GetByIdAsync(id);
                return item != null ? MapToDTO(item) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener item con ID {Id}", id);
                return null;
            }
        }

        ///  Crear item
        public async Task<ItemResponseDTO?> CreateAsync(ItemCreateDTO dto)
        {
            try
            {
                // Mantener el precio como string
                var item = new Models.Item
                {
                    Id = IdGenerator.GenerateId("Item"),
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    Stock = dto.Stock,
                    StockDisponible = dto.Stock ?? 0,
                    Preciobase = dto.Preciobase // Asignar directamente el string
                };

                var created = await _repository.CreateAsync(item);
                return created != null ? MapToDTO(created) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear item {Nombre}: {Message}", dto.Nombre, ex.Message);
                return null;
            }
        }
        
        ///  Actualizar item
        public async Task<ItemResponseDTO?> UpdateAsync(string id, ItemUpdateDTO dto)
        {
            try
            {
                var existingItem = await _repository.GetByIdAsync(id);
                if (existingItem == null) return null;

                existingItem.Nombre = dto.Nombre ?? existingItem.Nombre;
                existingItem.Descripcion = dto.Descripcion ?? existingItem.Descripcion;
                existingItem.Stock = dto.Stock ?? existingItem.Stock;
                
                // Actualizar el precio directamente como string
                if (dto.Preciobase != null)
                {
                    existingItem.Preciobase = dto.Preciobase;
                }

                var updated = await _repository.UpdateAsync(existingItem);
                return updated != null ? MapToDTO(updated) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar item con ID {Id}: {Message}", id, ex.Message);
                return null;
            }
        }

        ///  Eliminar item
        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var item = await _repository.GetByIdAsync(id);
                if (item == null) return false;

                return await _repository.DeleteAsync(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar item con ID {Id}", id);
                return false;
            }
        }

        ///  Buscar items por nombre
        public async Task<List<ItemResponseDTO>> SearchByNameAsync(string term)
        {
            try
            {
                var items = await _repository.GetAllAsync();
                var filtered = items
                    .Where(i => i.Nombre != null &&
                           i.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                return filtered.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar items por término {Term}", term);
                return new List<ItemResponseDTO>();
            }
        }

        ///  actualizar stock
        public async Task<bool> UpdateStockAsync(string id, int newStock)
        {
            try
            {
                return await _repository.ActualizarStockAsync(id, newStock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock del item con ID {Id}", id);
                return false;
            }
        }

        /// Recalcular stock disponible basado en uso actual (para un solo item)
        public async Task<bool> RecalcularStockDisponibleAsync(string id)
        {
            try
            {
                return await RecalcularStockDisponibleBatchAsync(new List<string> { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al recalcular stock disponible para item con ID {Id}", id);
                return false;
            }
        }

        /// Recalcular stock disponible para múltiples items a la vez (más eficiente)
        public async Task<bool> RecalcularStockDisponibleBatchAsync(List<string> itemsIds)
        {
            try
            {
                // Procesar todos los items en una sola consulta optimizada
                var itemsConStock = await _repository.GetItemsConStockEnUsoAsync(itemsIds);
                var itemsActualizados = new List<Models.Item>();

                foreach (var itemInfo in itemsConStock)
                {
                    var stockActual = itemInfo.Stock ?? 0;
                    var cantidadEnUso = itemInfo.CantidadEnUso;
                    var nuevoStockDisponible = (int)(stockActual - cantidadEnUso);

                    // Solo actualizar si el valor cambió
                    if (itemInfo.StockDisponible != nuevoStockDisponible)
                    {
                        itemInfo.StockDisponible = nuevoStockDisponible;
                        itemsActualizados.Add(itemInfo);
                    }
                }

                if (itemsActualizados.Any())
                {
                    await _repository.UpdateBatchAsync(itemsActualizados);
                    
                    // Usar log mínimo solo si hay más de un ítem
                    if (itemsActualizados.Count > 1)
                        _logger.LogDebug("Stock recalculado en lote para {Count} items", itemsActualizados.Count);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al recalcular stock disponible en lote");
                return false;
            }
        }

        /// Calcular la cantidad real en uso considerando múltiples reservas
        private async Task<double> CalcularCantidadEnUsoRealAsync(Models.Item item)
        {
            if (item.DetalleServicios == null || !item.DetalleServicios.Any())
                return 0;

            // Agrupar por servicio y calcular cuántas veces se usa cada servicio
            var serviciosUsados = item.DetalleServicios
                .Where(ds => !string.IsNullOrEmpty(ds.ServicioId)) 
                .GroupBy(ds => ds.ServicioId!) 
                .ToList();

            double cantidadTotalEnUso = 0;

            foreach (var servicioGroup in serviciosUsados)
            {
                var servicioId = servicioGroup.Key;
                var cantidadPorServicio = servicioGroup.Sum(ds => ds.Cantidad ?? 0);

                // Contar cuántas reservas ACTIVAS usan este servicio (excluyendo Finalizadas y Canceladas)
                var reservasUsandoServicio = await _repository.ContarReservasActivasUsandoServicioAsync(servicioId);
                
                // Multiplicar la cantidad por el número de reservas que usan este servicio
                var cantidadRealPorServicio = cantidadPorServicio * reservasUsandoServicio;
                
                cantidadTotalEnUso += cantidadRealPorServicio;
            }

            return cantidadTotalEnUso;
        }

        ///  Obtener todos los items con disponibilidad
        public async Task<List<ItemListResponseDTO>> GetAllWithAvailabilityAsync()
        {
            try
            {
                var items = await _repository.GetAllAsync();
                var result = new List<ItemListResponseDTO>();

                foreach (var item in items)
                {
                    // Recalcular para cada item considerando múltiples usos
                    var cantidadEnUso = await CalcularCantidadEnUsoRealAsync(item);
                    var stockActual = item.Stock ?? 0;
                    var stockDisponible = (int)(stockActual - cantidadEnUso);

                    // Actualizar el modelo si es necesario
                    if (item.StockDisponible != stockDisponible)
                    {
                        item.StockDisponible = stockDisponible;
                        await _repository.UpdateAsync(item);
                    }
                    
                    result.Add(new ItemListResponseDTO
                    {
                        Id = item.Id,
                        Nombre = item.Nombre,
                        Descripcion = item.Descripcion,
                        Stock = item.Stock,
                        StockDisponible = stockDisponible,
                        Preciobase = item.Preciobase,
                        ItemsEnUso = (int)cantidadEnUso 
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los items con disponibilidad");
                return new List<ItemListResponseDTO>();
            }
        }

        private ItemResponseDTO MapToDTO(Models.Item item)
        {
            // Calcular la cantidad en uso directamente
            int enUso = 0;
            if (item.DetalleServicios != null && item.DetalleServicios.Any())
            {
                enUso = (int)(item.Stock ?? 0) - item.StockDisponible;
            }
            
            return new ItemResponseDTO
            {
                Id = item.Id,
                Nombre = item.Nombre,
                Descripcion = item.Descripcion,
                Stock = item.Stock,
                StockDisponible = item.StockDisponible,
                Preciobase = item.Preciobase,
                ItemsEnUso = enUso >= 0 ? enUso : 0
            };
        }
    }
}