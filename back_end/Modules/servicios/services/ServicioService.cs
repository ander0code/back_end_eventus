using back_end.Modules.servicios.DTOs;
using back_end.Modules.servicios.Models;
using back_end.Modules.servicios.Repositories;
using back_end.Modules.inventario.Repositories;
using back_end.Modules.usuarios.Repositories;

namespace back_end.Modules.servicios.Services
{
    public interface IServicioService
    {
        Task<List<ServicioResponseDTO>> GetByCorreoAsync(string correo);
        Task<List<ServicioResponseDTO>> SearchServiciosAsync(string correo, string searchTerm);
        Task<ServicioResponseDTO?> GetByIdAsync(Guid id, string correo);
        Task<ServicioResponseDTO?> CreateAsync(string correo, ServicioCreateDTO dto);
        Task<ServicioResponseDTO?> UpdateAsync(string correo, Guid id, ServicioUpdateDTO dto);
        Task<bool> DeleteAsync(string correo, Guid id);
        
        // Operaciones específicas para gestionar los ítems del servicio
        Task<ServicioItemDTO?> AddItemToServicioAsync(string correo, Guid servicioId, ServicioItemCreateDTO dto);
        Task<List<ServicioItemDTO>> AddMultipleItemsToServicioAsync(string correo, Guid servicioId, List<ServicioItemCreateDTO> dtos);
        Task<ServicioItemDTO?> UpdateServicioItemAsync(string correo, Guid servicioId, Guid itemId, ServicioItemUpdateDTO dto);
        Task<bool> RemoveItemFromServicioAsync(string correo, Guid servicioId, Guid itemId);
        Task<List<ServicioItemDTO>> GetServicioItemsAsync(string correo, Guid servicioId);
    }

    public class ServicioService : IServicioService
    {
        private readonly IServicioRepository _repository;
        private readonly IInventarioRepository _inventarioRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ILogger<ServicioService> _logger;

        public ServicioService(
            IServicioRepository repository, 
            IInventarioRepository inventarioRepository, 
            IUsuarioRepository usuarioRepository,
            ILogger<ServicioService> logger)
        {
            _repository = repository;
            _inventarioRepository = inventarioRepository;
            _usuarioRepository = usuarioRepository;
            _logger = logger;
        }

        public async Task<List<ServicioResponseDTO>> GetByCorreoAsync(string correo)
        {
            var servicios = await _repository.GetByCorreoAsync(correo);
            return servicios.Select(MapToDTO).ToList();
        }
        
        public async Task<List<ServicioResponseDTO>> SearchServiciosAsync(string correo, string searchTerm)
        {
            var servicios = await _repository.SearchServiciosAsync(correo, searchTerm);
            return servicios.Select(MapToDTO).ToList();
        }

        public async Task<ServicioResponseDTO?> GetByIdAsync(Guid id, string correo)
        {
            var servicio = await _repository.GetByIdAndCorreoAsync(id, correo);
            return servicio == null ? null : MapToDTO(servicio);
        }

        public async Task<ServicioResponseDTO?> CreateAsync(string correo, ServicioCreateDTO dto)
        {
            var usuario = await _usuarioRepository.GetByCorreoAsync(correo);
            if (usuario == null)
            {
                _logger.LogWarning("Usuario no encontrado con correo: {Correo}", correo);
                return null;
            }

            var nuevo = new Servicio
            {
                UsuarioId = usuario.Id,
                Nombre = dto.NombreServicio ?? "Servicio sin nombre",
                Descripcion = dto.Descripcion,
                PrecioBase = dto.PrecioBase,
                Categoria = dto.TipoEvento,
                Imagenes = dto.Imagenes
            };

            var creado = await _repository.CreateAsync(nuevo);
            if (creado == null)
            {
                _logger.LogError("Error al crear servicio para usuario con correo: {Correo}", correo);
                return null;
            }

            // Si hay items en la DTO, agregarlos al servicio
            if (dto.Items != null && dto.Items.Any())
            {
                foreach (var itemDto in dto.Items)
                {
                    // Verificar que el item de inventario exista y pertenezca al usuario
                    var inventario = await _inventarioRepository.GetByIdAsync(itemDto.InventarioId);
                    if (inventario != null && inventario.UsuarioId == usuario.Id)
                    {
                        var servicioItem = new ServicioItem
                        {
                            ServicioId = creado.Id,
                            InventarioId = itemDto.InventarioId,
                            Cantidad = itemDto.Cantidad
                        };
                        
                        await _repository.AddServicioItemAsync(servicioItem);
                    }
                    else
                    {
                        _logger.LogWarning("Item de inventario no encontrado o no pertenece al usuario: {InventarioId}", itemDto.InventarioId);
                    }
                }
            }

            // Recargar el servicio con sus items
            var servicioConItems = await _repository.GetByIdAsync(creado.Id);
            return servicioConItems == null ? null : MapToDTO(servicioConItems);
        }

        public async Task<ServicioResponseDTO?> UpdateAsync(string correo, Guid id, ServicioUpdateDTO dto)
        {
            var existente = await _repository.GetByIdAndCorreoAsync(id, correo);
            if (existente == null)
            {
                _logger.LogWarning("Servicio no encontrado con ID: {Id} para correo: {Correo}", id, correo);
                return null;
            }

            // Actualiza solo si no es null
            if (dto.NombreServicio != null) existente.Nombre = dto.NombreServicio;
            if (dto.Descripcion != null) existente.Descripcion = dto.Descripcion;
            if (dto.PrecioBase != null) existente.PrecioBase = dto.PrecioBase;
            if (dto.TipoEvento != null) existente.Categoria = dto.TipoEvento;
            if (dto.Imagenes != null) existente.Imagenes = dto.Imagenes;

            var actualizado = await _repository.UpdateAsync(existente);
            if (actualizado == null)
            {
                _logger.LogError("Error al actualizar servicio con ID: {Id}", id);
                return null;
            }

            // Manejar los items que se deben agregar o actualizar
            if (dto.ItemsToAdd != null && dto.ItemsToAdd.Any())
            {
                foreach (var itemDto in dto.ItemsToAdd)
                {
                    // Verificar si el item ya existe en el servicio
                    var existingItem = existente.ServicioItems.FirstOrDefault(si => si.InventarioId == itemDto.InventarioId);
                    
                    if (existingItem != null)
                    {
                        // Actualizar cantidad del item existente
                        existingItem.Cantidad = itemDto.Cantidad;
                        await _repository.UpdateServicioItemAsync(existingItem);
                    }
                    else
                    {
                        // Verificar que el inventario exista y pertenezca al usuario
                        var inventario = await _inventarioRepository.GetByIdAsync(itemDto.InventarioId);
                        if (inventario != null && inventario.Usuario.CorreoElectronico == correo)
                        {
                            // Agregar nuevo item
                            var nuevoItem = new ServicioItem
                            {
                                ServicioId = existente.Id,
                                InventarioId = itemDto.InventarioId,
                                Cantidad = itemDto.Cantidad
                            };
                            await _repository.AddServicioItemAsync(nuevoItem);
                        }
                        else
                        {
                            _logger.LogWarning("Item de inventario no encontrado o no pertenece al usuario: {InventarioId}", itemDto.InventarioId);
                        }
                    }
                }
            }

            // Eliminar items si se solicita
            if (dto.ItemsToRemove != null && dto.ItemsToRemove.Any())
            {
                foreach (var itemId in dto.ItemsToRemove)
                {
                    var item = await _repository.GetServicioItemByIdAsync(itemId);
                    if (item != null && item.ServicioId == existente.Id)
                    {
                        await _repository.RemoveServicioItemAsync(item);
                    }
                }
            }

            // Recargar el servicio con sus items actualizados
            var servicioActualizado = await _repository.GetByIdAsync(id);
            return servicioActualizado == null ? null : MapToDTO(servicioActualizado);
        }

        public async Task<bool> DeleteAsync(string correo, Guid id)
        {
            var servicio = await _repository.GetByIdAndCorreoAsync(id, correo);
            if (servicio == null)
            {
                _logger.LogWarning("Servicio no encontrado para eliminar con ID: {Id} y correo: {Correo}", id, correo);
                return false;
            }

            return await _repository.DeleteAsync(servicio);
        }

        public async Task<ServicioItemDTO?> AddItemToServicioAsync(string correo, Guid servicioId, ServicioItemCreateDTO dto)
        {
            // Verificar que el servicio exista y pertenezca al usuario
            var servicio = await _repository.GetByIdAndCorreoAsync(servicioId, correo);
            if (servicio == null)
            {
                _logger.LogWarning("Servicio no encontrado con ID: {Id} para correo: {Correo}", servicioId, correo);
                return null;
            }
            
            // Verificar que el inventario exista y pertenezca al mismo usuario
            var inventario = await _inventarioRepository.GetByIdAsync(dto.InventarioId);
            if (inventario == null || inventario.Usuario?.CorreoElectronico != correo)
            {
                _logger.LogWarning("Item de inventario no encontrado o no pertenece al usuario: {InventarioId}", dto.InventarioId);
                return null;
            }
            
            // Verificar si ya existe un ServicioItem con el mismo InventarioId
            var existingItem = servicio.ServicioItems.FirstOrDefault(si => si.InventarioId == dto.InventarioId);
            if (existingItem != null)
            {
                // Actualizar la cantidad del item existente
                existingItem.Cantidad = dto.Cantidad;
                var updated = await _repository.UpdateServicioItemAsync(existingItem);
                return updated == null ? null : MapToItemDTO(updated);
            }
            
            // Crear nuevo ServicioItem
            var nuevoItem = new ServicioItem
            {
                ServicioId = servicioId,
                InventarioId = dto.InventarioId,
                Cantidad = dto.Cantidad
            };
            
            var created = await _repository.AddServicioItemAsync(nuevoItem);
            return created == null ? null : MapToItemDTO(created);
        }

        public async Task<List<ServicioItemDTO>> AddMultipleItemsToServicioAsync(string correo, Guid servicioId, List<ServicioItemCreateDTO> dtos)
        {
            // Verificar que el servicio exista y pertenezca al usuario
            var servicio = await _repository.GetByIdAndCorreoAsync(servicioId, correo);
            if (servicio == null)
            {
                _logger.LogWarning("Servicio no encontrado con ID: {Id} para correo: {Correo}", servicioId, correo);
                return new List<ServicioItemDTO>();
            }
            
            var resultItems = new List<ServicioItemDTO>();
            
            foreach (var dto in dtos)
            {
                // Verificar que el inventario exista y pertenezca al mismo usuario
                var inventario = await _inventarioRepository.GetByIdAsync(dto.InventarioId);
                if (inventario == null || inventario.Usuario?.CorreoElectronico != correo)
                {
                    _logger.LogWarning("Item de inventario no encontrado o no pertenece al usuario: {InventarioId}", dto.InventarioId);
                    continue;
                }
                
                // Verificar si ya existe un ServicioItem con el mismo InventarioId
                var existingItem = servicio.ServicioItems.FirstOrDefault(si => si.InventarioId == dto.InventarioId);
                if (existingItem != null)
                {
                    // Actualizar la cantidad del item existente
                    existingItem.Cantidad = dto.Cantidad;
                    var updated = await _repository.UpdateServicioItemAsync(existingItem);
                    if (updated != null)
                    {
                        resultItems.Add(MapToItemDTO(updated));
                    }
                }
                else
                {
                    // Crear nuevo ServicioItem
                    var nuevoItem = new ServicioItem
                    {
                        ServicioId = servicioId,
                        InventarioId = dto.InventarioId,
                        Cantidad = dto.Cantidad
                    };
                    
                    var created = await _repository.AddServicioItemAsync(nuevoItem);
                    if (created != null)
                    {
                        resultItems.Add(MapToItemDTO(created));
                    }
                }
            }
            
            return resultItems;
        }

        public async Task<ServicioItemDTO?> UpdateServicioItemAsync(string correo, Guid servicioId, Guid itemId, ServicioItemUpdateDTO dto)
        {
            // Verificar que el servicio exista y pertenezca al usuario
            var servicio = await _repository.GetByIdAndCorreoAsync(servicioId, correo);
            if (servicio == null)
            {
                _logger.LogWarning("Servicio no encontrado con ID: {Id} para correo: {Correo}", servicioId, correo);
                return null;
            }
            
            // Verificar que el item exista y pertenezca al servicio
            var item = await _repository.GetServicioItemByIdAsync(itemId);
            if (item == null || item.ServicioId != servicioId)
            {
                _logger.LogWarning("Item no encontrado en el servicio: {ItemId}", itemId);
                return null;
            }
            
            // Actualizar la cantidad
            if (dto.Cantidad.HasValue)
            {
                item.Cantidad = dto.Cantidad;
            }
            
            var updated = await _repository.UpdateServicioItemAsync(item);
            return updated == null ? null : MapToItemDTO(updated);
        }

        public async Task<bool> RemoveItemFromServicioAsync(string correo, Guid servicioId, Guid itemId)
        {
            // Verificar que el servicio exista y pertenezca al usuario
            var servicio = await _repository.GetByIdAndCorreoAsync(servicioId, correo);
            if (servicio == null)
            {
                _logger.LogWarning("Servicio no encontrado con ID: {Id} para correo: {Correo}", servicioId, correo);
                return false;
            }
            
            // Verificar que el item exista y pertenezca al servicio
            var item = await _repository.GetServicioItemByIdAsync(itemId);
            if (item == null || item.ServicioId != servicioId)
            {
                _logger.LogWarning("Item no encontrado en el servicio: {ItemId}", itemId);
                return false;
            }
            
            return await _repository.RemoveServicioItemAsync(item);
        }

        public async Task<List<ServicioItemDTO>> GetServicioItemsAsync(string correo, Guid servicioId)
        {
            // Verificar que el servicio exista y pertenezca al usuario
            var servicio = await _repository.GetByIdAndCorreoAsync(servicioId, correo);
            if (servicio == null)
            {
                _logger.LogWarning("Servicio no encontrado con ID: {Id} para correo: {Correo}", servicioId, correo);
                return new List<ServicioItemDTO>();
            }
            
            var items = await _repository.GetServicioItemsByServicioIdAsync(servicioId);
            return items.Select(MapToItemDTO).ToList();
        }

        private ServicioResponseDTO MapToDTO(Servicio servicio)
        {
            return new ServicioResponseDTO
            {
                Id = servicio.Id,
                NombreServicio = servicio.Nombre,
                Descripcion = servicio.Descripcion,
                PrecioBase = servicio.PrecioBase,
                TipoEvento = servicio.Categoria,
                Imagenes = servicio.Imagenes,
                FechaCreacion = DateTime.UtcNow, // No está en el modelo, usamos fecha actual
                UsuarioId = servicio.UsuarioId,
                Items = servicio.ServicioItems.Select(MapToItemDTO).ToList()
            };
        }
        
        private ServicioItemDTO MapToItemDTO(ServicioItem item)
        {
            return new ServicioItemDTO
            {
                Id = item.Id,
                InventarioId = item.InventarioId,
                Cantidad = item.Cantidad,
                NombreItem = item.Inventario?.Nombre,
                CategoriaItem = item.Inventario?.Categoria
            };
        }
    }
}
