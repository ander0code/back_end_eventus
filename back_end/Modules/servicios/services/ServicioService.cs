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
        Task<ServicioResponseDTO?> GetByIdAsync(Guid id, string correo);
        Task<ServicioResponseDTO?> CreateAsync(string correo, ServicioCreateDTO dto);
        Task<ServicioResponseDTO?> UpdateAsync(string correo, Guid id, ServicioUpdateDTO dto);
        Task<bool> DeleteAsync(string correo, Guid id);
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
                        // Verificar si hay stock suficiente
                        int cantidadRequerida = itemDto.Cantidad ?? 1;
                        if (inventario.Stock < cantidadRequerida)
                        {
                            _logger.LogWarning("Stock insuficiente para el item de inventario: {InventarioId}. Stock actual: {Stock}, Requerido: {Requerido}",
                                itemDto.InventarioId, inventario.Stock, cantidadRequerida);
                            continue; // Saltar este ítem si no hay stock suficiente
                        }

                        var servicioItem = new ServicioItem
                        {
                            ServicioId = creado.Id,
                            InventarioId = itemDto.InventarioId,
                            Cantidad = cantidadRequerida
                        };
                        
                        var itemCreado = await _repository.AddServicioItemAsync(servicioItem);
                        
                        // Actualizar el stock del inventario si se agregó el ítem correctamente
                        if (itemCreado != null)
                        {
                            inventario.Stock -= cantidadRequerida;
                            await _inventarioRepository.UpdateAsync(inventario);
                            _logger.LogInformation("Stock reducido al crear servicio: {InventarioId}, Cantidad: {Cantidad}", 
                                itemDto.InventarioId, cantidadRequerida);
                        }
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

            // Devolver el stock de los ítems al inventario antes de eliminar el servicio
            foreach (var item in servicio.ServicioItems)
            {
                if (item.Cantidad.HasValue && item.Cantidad.Value > 0)
                {
                    var inventario = await _inventarioRepository.GetByIdAsync(item.InventarioId);
                    if (inventario != null)
                    {
                        // Devolver el stock al inventario
                        inventario.Stock += item.Cantidad.Value;
                        await _inventarioRepository.UpdateAsync(inventario);
                        _logger.LogInformation("Stock devuelto al inventario al eliminar servicio: {InventarioId}, Cantidad: {Cantidad}", 
                            item.InventarioId, item.Cantidad.Value);
                    }
                }
            }

            return await _repository.DeleteAsync(servicio);
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
                CategoriaItem = item.Inventario?.Categoria,
                StockActual = item.Inventario?.Stock // Incluir el stock actual del inventario
            };
        }
    }
}
