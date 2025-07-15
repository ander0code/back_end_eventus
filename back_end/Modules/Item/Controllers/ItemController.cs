using back_end.Modules.Item.DTOs;
using back_end.Modules.Item.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Modules.Item.Controllers
{
    [ApiController]
    [Route("api/items")]
    [Authorize] 
    public class ItemController : ControllerBase
    {
        private readonly IItemService _service;
        private readonly ILogger<ItemController> _logger;

        public ItemController(IItemService service, ILogger<ItemController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// Obtener todos los items
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los items con disponibilidad");
                var items = await _service.GetAllWithAvailabilityAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los items");
                return StatusCode(500, new { message = "Error al obtener items", error = ex.Message });
            }
        }

        /// Crear item
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ItemCreateDTO dto)
        {
            try
            {
                _logger.LogInformation("Creando nuevo item");
                
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                    return BadRequest(new { message = "El nombre del item es requerido" });
                
                var creado = await _service.CreateAsync(dto);
                
                if (creado == null)
                    return StatusCode(500, new { message = "Error al crear el item" });
                
                return CreatedAtAction(nameof(GetAll), null, creado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear item");
                return StatusCode(500, new { message = "Error al crear item", error = ex.Message });
            }
        }

        /// Actualizar item por ID
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] ItemUpdateDTO dto)
        {
            try
            {
                _logger.LogInformation("Actualizando item con ID: {Id}", id);
                
                var actualizado = await _service.UpdateAsync(id, dto);
                
                if (actualizado == null)
                    return NotFound(new { message = "Item no encontrado" });
                
                return Ok(actualizado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar item con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar item", error = ex.Message });
            }
        }

        /// Eliminar item por ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                _logger.LogInformation("Eliminando item con ID: {Id}", id);
                
                var resultado = await _service.DeleteAsync(id);
                
                if (!resultado)
                    return BadRequest(new { message = "No se puede eliminar el item porque está relacionado con uno o más servicios." });
                
                return Ok(new { message = "Item eliminado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar item con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar item", error = ex.Message });
            }
        }
    }
}