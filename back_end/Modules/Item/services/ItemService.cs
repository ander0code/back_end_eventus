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
                var item = new Models.Item
                {
                    Id = IdGenerator.GenerateId("Item"),
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    Stock = dto.Stock,
                    StockDisponible = 0, // Inicializamos StockDisponible en 0
                    Preciobase = dto.Preciobase
                };

                var created = await _repository.CreateAsync(item);
                return created != null ? MapToDTO(created) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear item {Nombre}", dto.Nombre);
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
                existingItem.Preciobase = dto.Preciobase ?? existingItem.Preciobase;

                var updated = await _repository.UpdateAsync(existingItem);
                return updated != null ? MapToDTO(updated) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar item con ID {Id}", id);
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

        /// Recalcular stock disponible basado en uso actual
        public async Task<bool> RecalcularStockDisponibleAsync(string id)
        {
            try
            {
                // Obtener el item con todas sus relaciones actualizadas
                var item = await _repository.GetByIdAsync(id);
                if (item == null) return false;

                // Calcular cantidad en uso considerando múltiples reservas del mismo servicio
                var cantidadEnUso = await CalcularCantidadEnUsoRealAsync(item);
                var stockActual = item.Stock ?? 0;
                var nuevoStockDisponible = (int)(stockActual - cantidadEnUso);

                // Solo actualizar si el valor cambió
                if (item.StockDisponible != nuevoStockDisponible)
                {
                    item.StockDisponible = nuevoStockDisponible;
                    await _repository.UpdateAsync(item);
                    
                    _logger.LogInformation("Stock recalculado para item {ItemNombre}: Total: {StockTotal}, En uso REAL: {EnUso}, Disponible: {Disponible}", 
                        item.Nombre, stockActual, cantidadEnUso, nuevoStockDisponible);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al recalcular stock disponible para item con ID {Id}", id);
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
                .Where(ds => !string.IsNullOrEmpty(ds.ServicioId)) // Cambia HasValue por !string.IsNullOrEmpty
                .GroupBy(ds => ds.ServicioId!) // Agrupa por el string ServicioId
                .ToList();

            double cantidadTotalEnUso = 0;

            foreach (var servicioGroup in serviciosUsados)
            {
                var servicioId = servicioGroup.Key;
                var cantidadPorServicio = servicioGroup.Sum(ds => ds.Cantidad ?? 0);

                // Contar cuántas reservas usan este servicio
                var reservasUsandoServicio = await _repository.ContarReservasUsandoServicioAsync(servicioId);
                
                // Multiplicar la cantidad por el número de reservas que usan este servicio
                var cantidadRealPorServicio = cantidadPorServicio * reservasUsandoServicio;
                
                cantidadTotalEnUso += cantidadRealPorServicio;

                _logger.LogInformation("Servicio {ServicioId}: Cantidad base: {CantidadBase}, Reservas usando: {Reservas}, Total en uso: {TotalUso}", 
                    servicioId, cantidadPorServicio, reservasUsandoServicio, cantidadRealPorServicio);
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
                        Preciobase = item.Preciobase
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
            return new ItemResponseDTO
            {
                Id = item.Id,
                Nombre = item.Nombre,
                Descripcion = item.Descripcion,
                Stock = item.Stock,
                StockDisponible = item.StockDisponible,  // Agregamos el StockDisponible
                Preciobase = item.Preciobase
            };
        }
    }
}