using back_end.Modules.servicios.Models;
using back_end.Modules.servicios.Repositories;
using back_end.Modules.servicios.DTOs;
using back_end.Modules.Item.Repositories;

namespace back_end.Modules.servicios.Services
{    
    public interface IServicioService
    {
        Task<List<ServicioResponseDTO>> GetAllAsync();
        Task<List<ServicioResponseDTO>> SearchServiciosAsync(string searchTerm);
        Task<ServicioResponseDTO?> GetByIdAsync(Guid id);
        Task<ServicioResponseDTO?> CreateAsync(ServicioCreateDTO dto);
        Task<ServicioResponseDTO?> UpdateAsync(Guid id, ServicioUpdateDTO dto);
        Task<bool> DeleteAsync(Guid id);
        
        // Métodos para DetalleServicio
        Task<DetalleServicioDTO?> AddDetalleServicioAsync(Guid servicioId, DetalleServicioCreateDTO dto);
        Task<DetalleServicioDTO?> UpdateDetalleServicioAsync(Guid id, DetalleServicioUpdateDTO dto);
        Task<bool> RemoveDetalleServicioAsync(Guid id);
        Task<bool> RemoveMultipleDetalleServiciosAsync(Guid servicioId, DetalleServicioDeleteDTO dto);
        
        // Métodos de compatibilidad para servicioItem
        Task<ServicioItemDTO?> AddServicioItemAsync(Guid servicioId, ServicioItemCreateDTO dto);
        Task<ServicioItemDTO?> UpdateServicioItemAsync(Guid id, ServicioItemUpdateDTO dto);
        Task<bool> RemoveServicioItemAsync(Guid id);
        Task<bool> RemoveMultipleServicioItemsAsync(Guid servicioId, ServicioItemsDeleteDTO dto);
    }

    public class ServicioService : IServicioService
    {
        private readonly IServicioRepository _repository;
        private readonly IItemRepository _itemRepository;
        private readonly ILogger<ServicioService> _logger;

        public ServicioService(IServicioRepository repository, IItemRepository itemRepository, ILogger<ServicioService> logger)
        {
            _repository = repository;
            _itemRepository = itemRepository;
            _logger = logger;
        }        public async Task<List<ServicioResponseDTO>> GetAllAsync()
        {
            try
            {
                // Usamos el método GetAllAsync del repositorio
                var servicios = await _repository.GetAllAsync();
                return servicios.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los servicios");
                return new List<ServicioResponseDTO>();
            }
        }

        public async Task<List<ServicioResponseDTO>> SearchServiciosAsync(string searchTerm)
        {
            try
            {
                // Usamos el método de búsqueda sin correo
                var servicios = await _repository.SearchServiciosAsync(searchTerm);
                return servicios.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar servicios con término {SearchTerm}", searchTerm);
                return new List<ServicioResponseDTO>();
            }
        }

        public async Task<ServicioResponseDTO?> GetByIdAsync(Guid id)
        {
            try
            {
                var servicio = await _repository.GetByIdAsync(id);
                return servicio != null ? MapToDTO(servicio) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener servicio con ID {Id}", id);
                return null;
            }
        }
        
        public async Task<ServicioResponseDTO?> CreateAsync(ServicioCreateDTO dto)
        {
            try
            {
                var servicio = new Servicio
                {
                    Id = Guid.NewGuid(),
                    Nombre = dto.NombreServicio,
                    Descripcion = dto.Descripcion,
                    PrecioBase = dto.PrecioBase
                };

                var creado = await _repository.CreateAsync(servicio);                if (creado != null && dto.Items != null && dto.Items.Any())
                {
                    foreach (var itemDto in dto.Items)
                    {
                        var detalle = new DetalleServicio
                        {
                            Id = Guid.NewGuid(),
                            ServicioId = servicio.Id,
                            InventarioId = itemDto.InventarioId,
                            Cantidad = itemDto.Cantidad,
                            Estado = itemDto.Estado?.Length > 10 ? itemDto.Estado.Substring(0, 10) : itemDto.Estado,
                            PrecioActual = itemDto.PrecioActual,
                            Fecha = DateTime.Now
                        };

                        await _repository.AddDetalleServicioAsync(detalle);
                    }

                    // Recargar el servicio con sus detalles
                    creado = await _repository.GetByIdAsync(servicio.Id);
                }

                return creado != null ? MapToDTO(creado) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear servicio");
                return null;
            }
        }
        
        public async Task<ServicioResponseDTO?> UpdateAsync(Guid id, ServicioUpdateDTO dto)
        {
            try
            {
                var servicio = await _repository.GetByIdAsync(id);
                if (servicio == null) return null;

                servicio.Nombre = dto.NombreServicio ?? servicio.Nombre;
                servicio.Descripcion = dto.Descripcion ?? servicio.Descripcion;
                servicio.PrecioBase = dto.PrecioBase ?? servicio.PrecioBase;

                var actualizado = await _repository.UpdateAsync(servicio);

                // Agregar nuevos items si se proporcionan
                if (actualizado != null && dto.ItemsToAdd != null && dto.ItemsToAdd.Any())
                {
                    foreach (var itemDto in dto.ItemsToAdd)
                    {                        var detalle = new DetalleServicio
                        {
                            Id = Guid.NewGuid(),
                            ServicioId = servicio.Id,
                            InventarioId = itemDto.InventarioId,
                            Cantidad = itemDto.Cantidad,
                            Estado = itemDto.Estado?.Length > 10 ? itemDto.Estado.Substring(0, 10) : itemDto.Estado,
                            PrecioActual = itemDto.PrecioActual,
                            Fecha = DateTime.Now
                        };

                        await _repository.AddDetalleServicioAsync(detalle);
                    }
                }

                // Eliminar items si se solicita
                if (actualizado != null && dto.ItemsToRemove != null && dto.ItemsToRemove.Any())
                {
                    var detalles = await _repository.GetDetalleServiciosByServicioIdAsync(servicio.Id);
                    var detallesToRemove = detalles.Where(d => dto.ItemsToRemove.Contains(d.Id)).ToList();
                    if (detallesToRemove.Any())
                    {
                        await _repository.RemoveMultipleDetalleServiciosAsync(detallesToRemove);
                    }
                }

                // Recargar el servicio con sus detalles actualizados
                return await GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar servicio con ID {Id}", id);
                return null;
            }
        }
        
        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var servicio = await _repository.GetByIdAsync(id);
                if (servicio == null) return false;

                return await _repository.DeleteAsync(servicio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar servicio con ID {Id}", id);
                return false;
            }
        }

        // DetalleServicio methods
        public async Task<DetalleServicioDTO?> AddDetalleServicioAsync(Guid servicioId, DetalleServicioCreateDTO dto)
        {
            try
            {
                var servicio = await _repository.GetByIdAsync(servicioId);
                if (servicio == null) return null;

                var item = await _itemRepository.GetByIdAsync(dto.InventarioId);
                if (item == null) return null;                var detalle = new DetalleServicio
                {
                    Id = Guid.NewGuid(),
                    ServicioId = servicioId,
                    InventarioId = dto.InventarioId,
                    Cantidad = dto.Cantidad,
                    Estado = dto.Estado?.Length > 10 ? dto.Estado.Substring(0, 10) : dto.Estado, // Limitamos a 10 caracteres
                    PrecioActual = dto.PrecioActual ?? item.Preciobase,
                    Fecha = DateTime.Now
                };

                var creado = await _repository.AddDetalleServicioAsync(detalle);
                
                if (creado != null)
                {
                    return new DetalleServicioDTO
                    {
                        Id = creado.Id,
                        InventarioId = creado.InventarioId,
                        Cantidad = creado.Cantidad,
                        NombreItem = creado.Inventario?.Nombre,
                        Estado = creado.Estado,
                        Fecha = creado.Fecha,
                        PrecioActual = creado.PrecioActual,
                        StockActual = creado.Inventario?.Stock ?? 0
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar detalle de servicio para servicio {ServicioId}", servicioId);
                return null;
            }
        }        public async Task<DetalleServicioDTO?> UpdateDetalleServicioAsync(Guid id, DetalleServicioUpdateDTO dto)
        {
            try
            {
                var detalle = await _repository.GetDetalleServicioByIdAsync(id);
                if (detalle == null) return null;
                
                detalle.Cantidad = dto.Cantidad ?? detalle.Cantidad;
                
                // Aplicamos la limitación de 10 caracteres al actualizar el estado
                if (dto.Estado != null)
                {
                    detalle.Estado = dto.Estado.Length > 10 ? dto.Estado.Substring(0, 10) : dto.Estado;
                }
                
                detalle.PrecioActual = dto.PrecioActual ?? detalle.PrecioActual;

                var actualizado = await _repository.UpdateDetalleServicioAsync(detalle);
                if (actualizado != null)
                {
                    return new DetalleServicioDTO
                    {
                        Id = actualizado.Id,
                        InventarioId = actualizado.InventarioId,
                        Cantidad = actualizado.Cantidad,
                        NombreItem = actualizado.Inventario?.Nombre,
                        Estado = actualizado.Estado,
                        Fecha = actualizado.Fecha,
                        PrecioActual = actualizado.PrecioActual,
                        StockActual = actualizado.Inventario?.Stock ?? 0
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar detalle de servicio con ID {Id}", id);
                return null;
            }
        }

        public async Task<bool> RemoveDetalleServicioAsync(Guid id)
        {
            try
            {
                var detalle = await _repository.GetDetalleServicioByIdAsync(id);
                if (detalle == null) return false;

                return await _repository.RemoveDetalleServicioAsync(detalle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar detalle de servicio con ID {Id}", id);
                return false;
            }
        }

        public async Task<bool> RemoveMultipleDetalleServiciosAsync(Guid servicioId, DetalleServicioDeleteDTO dto)
        {
            try
            {
                if (dto.ItemIds == null || !dto.ItemIds.Any()) return false;

                var detalles = await _repository.GetDetalleServiciosByServicioIdAsync(servicioId);
                var detallesToRemove = detalles.Where(d => dto.ItemIds.Contains(d.Id)).ToList();

                if (!detallesToRemove.Any()) return false;

                return await _repository.RemoveMultipleDetalleServiciosAsync(detallesToRemove);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar múltiples detalles de servicio para servicio {ServicioId}", servicioId);
                return false;
            }
        }        // Métodos de compatibilidad para servicioItem
        public async Task<ServicioItemDTO?> AddServicioItemAsync(Guid servicioId, ServicioItemCreateDTO dto)
        {
            // Convertimos el DTO de entrada
            var detalleDto = new DetalleServicioCreateDTO
            {
                InventarioId = dto.InventarioId,
                Cantidad = dto.Cantidad,
                Estado = dto.Estado,
                PrecioActual = dto.PrecioActual
            };
            
            var result = await AddDetalleServicioAsync(servicioId, detalleDto);
            if (result == null) return null;
            
            // Convertimos el resultado de nuevo al tipo ServicioItemDTO
            return new ServicioItemDTO
            {
                Id = result.Id,
                InventarioId = result.InventarioId,
                Cantidad = result.Cantidad,
                NombreItem = result.NombreItem,
                Estado = result.Estado,
                Fecha = result.Fecha,
                PrecioActual = result.PrecioActual,
                StockActual = result.StockActual
            };
        }

        public async Task<ServicioItemDTO?> UpdateServicioItemAsync(Guid id, ServicioItemUpdateDTO dto)
        {
            // Convertimos el DTO de entrada
            var detalleDto = new DetalleServicioUpdateDTO
            {
                Cantidad = dto.Cantidad,
                Estado = dto.Estado,
                PrecioActual = dto.PrecioActual
            };
            
            var result = await UpdateDetalleServicioAsync(id, detalleDto);
            if (result == null) return null;
            
            // Convertimos el resultado de nuevo al tipo ServicioItemDTO
            return new ServicioItemDTO
            {
                Id = result.Id,
                InventarioId = result.InventarioId,
                Cantidad = result.Cantidad,
                NombreItem = result.NombreItem,
                Estado = result.Estado,
                Fecha = result.Fecha,
                PrecioActual = result.PrecioActual,
                StockActual = result.StockActual
            };
        }

        public Task<bool> RemoveServicioItemAsync(Guid id)
        {
            return RemoveDetalleServicioAsync(id);
        }        public async Task<bool> RemoveMultipleServicioItemsAsync(Guid servicioId, ServicioItemsDeleteDTO dto)
        {
            // Convertimos el DTO de entrada
            var detalleDto = new DetalleServicioDeleteDTO
            {
                ItemIds = dto.ItemIds
            };
            
            return await RemoveMultipleDetalleServiciosAsync(servicioId, detalleDto);
        }        private ServicioResponseDTO MapToDTO(Servicio servicio)
        {
            var dto = new ServicioResponseDTO
            {
                Id = servicio.Id,
                NombreServicio = servicio.Nombre,
                Descripcion = servicio.Descripcion,
                PrecioBase = servicio.PrecioBase,
                // No mapeamos campos que no existen en el modelo
                // TipoEvento, Imagenes, FechaCreacion y UsuarioId se mantienen como null
                Items = new List<ServicioItemDTO>()
            };

            if (servicio.DetalleServicios != null)
            {
                foreach (var detalle in servicio.DetalleServicios)
                {
                    dto.Items.Add(new ServicioItemDTO
                    {
                        Id = detalle.Id,
                        InventarioId = detalle.InventarioId ?? Guid.Empty,
                        Cantidad = detalle.Cantidad,
                        NombreItem = detalle.Inventario?.Nombre,
                        Estado = detalle.Estado,
                        Fecha = detalle.Fecha,
                        PrecioActual = detalle.PrecioActual,
                        StockActual = detalle.Inventario?.Stock ?? 0
                    });
                }
            }

            return dto;
        }
    }
}
