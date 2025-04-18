using back_end.Modules.inventario.DTOs;
using back_end.Modules.inventario.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Modules.inventario.Controllers
{
    [ApiController]
    [Route("api/inventario")]
    [Authorize] // Proteger todos los endpoints por defecto
    public class InventarioController : ControllerBase
    {
        private readonly IInventarioService _service;
        private readonly ILogger<InventarioController> _logger;

        public InventarioController(IInventarioService service, ILogger<InventarioController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("Solicitando todos los items de inventario");
                var inventario = await _service.GetAllAsync();
                return Ok(inventario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los items de inventario");
                return StatusCode(500, new { message = "Error al obtener inventario", error = ex.Message });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                _logger.LogInformation("Solicitando item de inventario con ID: {Id}", id);
                var item = await _service.GetByIdAsync(id);
                
                if (item == null)
                {
                    _logger.LogWarning("Item de inventario no encontrado con ID: {Id}", id);
                    return NotFound(new { message = "Item no encontrado" });
                }
                
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener item de inventario con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al obtener item", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId:guid}")]
        public async Task<IActionResult> GetByUsuarioId(Guid usuarioId)
        {
            try
            {
                _logger.LogInformation("Solicitando items de inventario para usuario con ID: {UsuarioId}", usuarioId);
                var items = await _service.GetByUsuarioIdAsync(usuarioId);
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener items de inventario para usuario con ID: {UsuarioId}", usuarioId);
                return StatusCode(500, new { message = "Error al obtener inventario", error = ex.Message });
            }
        }
        
        [HttpGet("correo/{correo}")]
        public async Task<IActionResult> GetByCorreo(string correo)
        {
            try
            {
                _logger.LogInformation("Solicitando items de inventario para usuario con correo: {Correo}", correo);
                var items = await _service.GetByCorreoAsync(correo);
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener items de inventario para usuario con correo: {Correo}", correo);
                return StatusCode(500, new { message = "Error al obtener inventario", error = ex.Message });
            }
        }

        [HttpPost("{correo}")]
        public async Task<IActionResult> Create(string correo, [FromBody] InventarioCreateDTO dto)
        {
            try
            {
                _logger.LogInformation("Creando nuevo item de inventario para usuario con correo: {Correo}", correo);
                
                // Validación básica
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                    return BadRequest(new { message = "El nombre del item es requerido" });
                
                var creado = await _service.CreateAsync(correo, dto);
                
                if (creado == null)
                {
                    _logger.LogWarning("Usuario no encontrado con correo: {Correo}", correo);
                    return NotFound(new { message = "Usuario no encontrado" });
                }
                
                return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear item de inventario para usuario con correo: {Correo}", correo);
                return StatusCode(500, new { message = "Error al crear item", error = ex.Message });
            }
        }

        [HttpPut("{correo}/{id:guid}")]
        public async Task<IActionResult> Update(string correo, Guid id, [FromBody] InventarioUpdateDTO dto)
        {
            try
            {
                _logger.LogInformation("Actualizando item de inventario con ID: {Id} para usuario con correo: {Correo}", id, correo);
                
                var actualizado = await _service.UpdateAsync(id, correo, dto);
                
                if (actualizado == null)
                {
                    _logger.LogWarning("Item de inventario no encontrado con ID: {Id} para correo: {Correo}", id, correo);
                    return NotFound(new { message = "Item no encontrado o no pertenece al usuario" });
                }
                
                return Ok(actualizado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar item de inventario con ID: {Id} para correo: {Correo}", id, correo);
                return StatusCode(500, new { message = "Error al actualizar item", error = ex.Message });
            }
        }

        [HttpDelete("{correo}/{id:guid}")]
        public async Task<IActionResult> Delete(string correo, Guid id)
        {
            try
            {
                _logger.LogInformation("Eliminando item de inventario con ID: {Id} para usuario con correo: {Correo}", id, correo);
                
                var resultado = await _service.DeleteAsync(id, correo);
                
                if (!resultado)
                {
                    _logger.LogWarning("Item de inventario no encontrado con ID: {Id} para correo: {Correo}", id, correo);
                    return NotFound(new { message = "Item no encontrado o no pertenece al usuario" });
                }
                
                return Ok(new { message = "Item eliminado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar item de inventario con ID: {Id} para correo: {Correo}", id, correo);
                return StatusCode(500, new { message = "Error al eliminar item", error = ex.Message });
            }
        }

        [HttpPut("{correo}/{id:guid}/stock")]
        public async Task<IActionResult> ActualizarStock(string correo, Guid id, [FromBody] int cantidad)
        {
            try
            {
                _logger.LogInformation("Actualizando stock de item con ID: {Id} a {Cantidad} para usuario con correo: {Correo}", id, cantidad, correo);
                
                var resultado = await _service.ActualizarStockAsync(id, correo, cantidad);
                
                if (!resultado)
                {
                    _logger.LogWarning("Item de inventario no encontrado con ID: {Id} para correo: {Correo}", id, correo);
                    return NotFound(new { message = "Item no encontrado o no pertenece al usuario" });
                }
                
                return Ok(new { message = "Stock actualizado correctamente", cantidad });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock de item con ID: {Id} para correo: {Correo}", id, correo);
                return StatusCode(500, new { message = "Error al actualizar stock", error = ex.Message });
            }
        }
    }
}