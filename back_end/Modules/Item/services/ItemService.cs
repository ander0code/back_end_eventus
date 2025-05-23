using back_end.Modules.Item.DTOs;
using back_end.Modules.Item.Repositories;

namespace back_end.Modules.Item.Services
{
    public interface IItemService
    {
        Task<List<ItemResponseDTO>> GetAllAsync();
        Task<ItemResponseDTO?> GetByIdAsync(Guid id);
        Task<ItemResponseDTO?> CreateAsync(ItemCreateDTO dto);
        Task<ItemResponseDTO?> UpdateAsync(Guid id, ItemUpdateDTO dto);
        Task<bool> DeleteAsync(Guid id);
        Task<List<ItemResponseDTO>> SearchByNameAsync(string term);
        Task<bool> UpdateStockAsync(Guid id, int newStock);
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

        public async Task<ItemResponseDTO?> GetByIdAsync(Guid id)
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
                    Id = Guid.NewGuid(),
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    Stock = dto.Stock,
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
        public async Task<ItemResponseDTO?> UpdateAsync(Guid id, ItemUpdateDTO dto)
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
        public async Task<bool> DeleteAsync(Guid id)
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
        public async Task<bool> UpdateStockAsync(Guid id, int newStock)
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

        private ItemResponseDTO MapToDTO(Models.Item item)
        {
            return new ItemResponseDTO
            {
                Id = item.Id,
                Nombre = item.Nombre,
                Descripcion = item.Descripcion,
                Stock = item.Stock,
                Preciobase = item.Preciobase
            };
        }
    }
}