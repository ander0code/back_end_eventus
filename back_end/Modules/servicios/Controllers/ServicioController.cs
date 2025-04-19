using back_end.Modules.servicios.DTOs;
using back_end.Modules.servicios.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace back_end.Modules.servicios.Controllers
{
    [ApiController]
    [Route("api/servicios")]
    [Authorize]
    public class ServicioController : ControllerBase
    {
        private readonly IServicioService _service;
        private readonly ILogger<ServicioController> _logger;

        public ServicioController(IServicioService service, ILogger<ServicioController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // Obtener todos los servicios de un usuario
        [HttpGet("{correo}")]
        public async Task<IActionResult> GetByCorreo(string correo)
        {
            try
            {
                _logger.LogInformation("Solicitud para obtener servicios del usuario con correo: {Correo}", correo);
                var servicios = await _service.GetByCorreoAsync(correo);
                return Ok(servicios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener servicios para el usuario con correo: {Correo}", correo);
                return StatusCode(500, new { message = "Error al obtener servicios" });
            }
        }
        
        // Obtener un servicio específico por ID
        [HttpGet("{correo}/{id:guid}")]
        public async Task<IActionResult> GetById(string correo, Guid id)
        {
            try
            {
                _logger.LogInformation("Solicitud para obtener servicio con ID: {Id} del usuario con correo: {Correo}", id, correo);
                var servicio = await _service.GetByIdAsync(id, correo);
                
                if (servicio == null)
                {
                    _logger.LogWarning("Servicio no encontrado con ID: {Id} para correo: {Correo}", id, correo);
                    return NotFound(new { message = "Servicio no encontrado" });
                }
                
                return Ok(servicio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener servicio con ID: {Id} para correo: {Correo}", id, correo);
                return StatusCode(500, new { message = "Error al obtener servicio" });
            }
        }
        
        // Buscar servicios por término (nombre, descripción o categoría)
        [HttpGet("{correo}/buscar")]
        public async Task<IActionResult> SearchServicios(string correo, [FromQuery] string termino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino))
                {
                    _logger.LogWarning("Búsqueda de servicios con término vacío");
                    return BadRequest(new { message = "El término de búsqueda es requerido" });
                }
                
                _logger.LogInformation("Buscando servicios con término: {Termino} para usuario: {Correo}", termino, correo);
                var servicios = await _service.SearchServiciosAsync(correo, termino);
                return Ok(servicios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar servicios para usuario con correo: {Correo}", correo);
                return StatusCode(500, new { message = "Error al buscar servicios" });
            }
        }

        // Crear un nuevo servicio
        [HttpPost("{correo}")]
        public async Task<IActionResult> Create(string correo, [FromBody] ServicioCreateDTO dto)
        {
            try
            {
                _logger.LogInformation("Solicitud para crear servicio del usuario con correo: {Correo}", correo);
                
                // Validación básica
                if (string.IsNullOrWhiteSpace(dto.NombreServicio))
                {
                    return BadRequest(new { message = "El nombre del servicio es requerido" });
                }
                
                var nuevoServicio = await _service.CreateAsync(correo, dto);
                
                if (nuevoServicio == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }
                
                return Created($"api/servicios/{correo}/{nuevoServicio.Id}", nuevoServicio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear servicio para el usuario con correo: {Correo}", correo);
                return StatusCode(500, new { message = "Error al crear servicio" });
            }
        }

        // Actualizar un servicio existente
        [HttpPut("{correo}/{id:guid}")]
        public async Task<IActionResult> Update(string correo, Guid id, [FromBody] ServicioUpdateDTO dto)
        {
            try
            {
                _logger.LogInformation("Solicitud para actualizar servicio con ID: {Id} del usuario con correo: {Correo}", id, correo);
                var actualizado = await _service.UpdateAsync(correo, id, dto);
                if (actualizado == null)
                {
                    _logger.LogWarning("Servicio no encontrado con ID: {Id} para el usuario con correo: {Correo}", id, correo);
                    return NotFound(new { message = "Servicio no encontrado" });
                }

                return Ok(actualizado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar servicio con ID: {Id} del usuario con correo: {Correo}", id, correo);
                return StatusCode(500, new { message = "Error al actualizar servicio" });
            }
        }

        // Eliminar un servicio
        [HttpDelete("{correo}/{id:guid}")]
        public async Task<IActionResult> Delete(string correo, Guid id)
        {
            try
            {
                _logger.LogInformation("Solicitud para eliminar servicio con ID: {Id} del usuario con correo: {Correo}", id, correo);
                var eliminado = await _service.DeleteAsync(correo, id);
                if (!eliminado)
                {
                    _logger.LogWarning("Servicio no encontrado para eliminar con ID: {Id} y correo: {Correo}", id, correo);
                    return NotFound(new { message = "Servicio no encontrado" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar servicio con ID: {Id} del usuario con correo: {Correo}", id, correo);
                return StatusCode(500, new { message = "Error al eliminar servicio" });
            }
        }
        
        // Endpoints para gestionar los items de un servicio
        
        // Obtener todos los items asociados a un servicio
        [HttpGet("{correo}/{servicioId:guid}/items")]
        public async Task<IActionResult> GetServicioItems(string correo, Guid servicioId)
        {
            try
            {
                _logger.LogInformation("Solicitud para obtener items del servicio con ID: {Id} del usuario con correo: {Correo}", servicioId, correo);
                var items = await _service.GetServicioItemsAsync(correo, servicioId);
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener items del servicio con ID: {Id} del usuario con correo: {Correo}", servicioId, correo);
                return StatusCode(500, new { message = "Error al obtener items del servicio" });
            }
        }
        
        // Agregar uno o varios items a un servicio
        [HttpPost("{correo}/{servicioId:guid}/items")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddItemsToServicio(string correo, Guid servicioId, [FromBody] List<ServicioItemCreateDTO> dtos)
        {
            try
            {
                _logger.LogInformation("Agregando items al servicio con ID: {Id} del usuario con correo: {Correo}", servicioId, correo);
                
                if (dtos == null || !dtos.Any())
                {
                    return BadRequest(new { message = "La lista de items no puede estar vacía" });
                }
                
                // Si solo hay un item, usar el método para un solo item
                if (dtos.Count == 1)
                {
                    var item = await _service.AddItemToServicioAsync(correo, servicioId, dtos[0]);
                    
                    if (item == null)
                    {
                        return BadRequest(new { message = "No se pudo agregar el item al servicio" });
                    }
                    
                    return Created($"api/servicios/{correo}/{servicioId}/items/{item.Id}", item);
                }
                else // Si hay múltiples items, usar el método para múltiples items
                {
                    var items = await _service.AddMultipleItemsToServicioAsync(correo, servicioId, dtos);
                    
                    if (items == null || !items.Any())
                    {
                        return BadRequest(new { message = "No se pudieron agregar items al servicio" });
                    }
                    
                    return Created($"api/servicios/{correo}/{servicioId}/items", items);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar items al servicio con ID: {Id} del usuario con correo: {Correo}", servicioId, correo);
                return StatusCode(500, new { message = "Error al agregar items al servicio" });
            }
        }
        
        // Actualizar un item de un servicio
        [HttpPut("{correo}/{servicioId:guid}/items/{itemId:guid}")]
        public async Task<IActionResult> UpdateServicioItem(string correo, Guid servicioId, Guid itemId, [FromBody] ServicioItemUpdateDTO dto)
        {
            try
            {
                _logger.LogInformation("Actualizando item con ID: {ItemId} del servicio con ID: {ServicioId}", itemId, servicioId);
                var item = await _service.UpdateServicioItemAsync(correo, servicioId, itemId, dto);
                
                if (item == null)
                {
                    return NotFound(new { message = "Item no encontrado en el servicio" });
                }
                
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar item con ID: {ItemId} del servicio con ID: {ServicioId}", itemId, servicioId);
                return StatusCode(500, new { message = "Error al actualizar item del servicio" });
            }
        }
        
        // Eliminar un item de un servicio
        [HttpDelete("{correo}/{servicioId:guid}/items/{itemId:guid}")]
        public async Task<IActionResult> RemoveItemFromServicio(string correo, Guid servicioId, Guid itemId)
        {
            try
            {
                _logger.LogInformation("Eliminando item con ID: {ItemId} del servicio con ID: {ServicioId}", itemId, servicioId);
                var eliminado = await _service.RemoveItemFromServicioAsync(correo, servicioId, itemId);
                
                if (!eliminado)
                {
                    return NotFound(new { message = "Item no encontrado en el servicio" });
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar item con ID: {ItemId} del servicio con ID: {ServicioId}", itemId, servicioId);
                return StatusCode(500, new { message = "Error al eliminar item del servicio" });
            }
        }
    }
}
