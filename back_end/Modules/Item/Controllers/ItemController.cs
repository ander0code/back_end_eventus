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

        [HttpGet("usuario/{usuarioId:guid}")]
        public async Task<IActionResult> GetByUsuarioId(Guid usuarioId)
        {
            try
            {
                _logger.LogInformation("Solicitando items para usuario con ID: {UsuarioId}", usuarioId);
                var items = await _service.GetByUsuarioIdAsync(usuarioId);
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener items para usuario con ID: {UsuarioId}", usuarioId);
                return StatusCode(500, new { message = "Error al obtener items", error = ex.Message });
            }
        }

        [HttpPost("{correo}")]
        public async Task<IActionResult> Create(string correo, [FromBody] ItemCreateDTO dto)
        {
            try
            {
                _logger.LogInformation("Creando nuevo item para usuario con correo: {Correo}", correo);
                
                // Validación básica
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                    return BadRequest(new { message = "El nombre del item es requerido" });
                
                var creado = await _service.CreateAsync(correo, dto);
                
                if (creado == null)
                {
                    _logger.LogWarning("Usuario no encontrado con correo: {Correo}", correo);
                    return NotFound(new { message = "Usuario no encontrado" });
                }
                
                return CreatedAtAction(nameof(GetByUsuarioId), new { usuarioId = creado.Id }, creado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear item para usuario con correo: {Correo}", correo);
                return StatusCode(500, new { message = "Error al crear item", error = ex.Message });
            }
        }

        [HttpPut("{correo}/{id:guid}")]
        public async Task<IActionResult> Update(string correo, Guid id, [FromBody] ItemUpdateDTO dto)
        {
            try
            {
                _logger.LogInformation("Actualizando item con ID: {Id} para usuario con correo: {Correo}", id, correo);
                
                var actualizado = await _service.UpdateAsync(id, correo, dto);
                
                if (actualizado == null)
                {
                    _logger.LogWarning("Item no encontrado con ID: {Id} para correo: {Correo}", id, correo);
                    return NotFound(new { message = "Item no encontrado o no pertenece al usuario" });
                }
                
                return Ok(actualizado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar item con ID: {Id} para correo: {Correo}", id, correo);
                return StatusCode(500, new { message = "Error al actualizar item", error = ex.Message });
            }
        }

        [HttpDelete("{correo}/{id:guid}")]
        public async Task<IActionResult> Delete(string correo, Guid id)
        {
            try
            {
                _logger.LogInformation("Eliminando item con ID: {Id} para usuario con correo: {Correo}", id, correo);
                
                var resultado = await _service.DeleteAsync(id, correo);
                
                if (!resultado)
                {
                    _logger.LogWarning("Item no encontrado con ID: {Id} para correo: {Correo}", id, correo);
                    return NotFound(new { message = "Item no encontrado o no pertenece al usuario" });
                }
                
                return Ok(new { message = "Item eliminado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar item con ID: {Id} para correo: {Correo}", id, correo);
                return StatusCode(500, new { message = "Error al eliminar item", error = ex.Message });
            }
        }
    }
}