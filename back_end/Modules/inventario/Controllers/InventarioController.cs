using back_end.Modules.inventario.Models;
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
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

        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> GetByUsuarioId(int usuarioId)
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Inventario inventario)
        {
            try
            {
                _logger.LogInformation("Creando nuevo item de inventario: {NombreItem}", inventario.NombreItem);
                
                // Validación básica
                if (string.IsNullOrWhiteSpace(inventario.NombreItem))
                    return BadRequest(new { message = "El nombre del item es requerido" });
                
                var creado = await _service.CreateAsync(inventario);
                return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear item de inventario: {NombreItem}", inventario.NombreItem);
                return StatusCode(500, new { message = "Error al crear item", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Inventario inventario)
        {
            try
            {
                _logger.LogInformation("Actualizando item de inventario con ID: {Id}", id);
                
                // Asegurar que el ID en la URL coincida con el objeto
                if (id != inventario.Id)
                    return BadRequest(new { message = "ID no coincide" });
                
                // Verificar que el item exista
                var existente = await _service.GetByIdAsync(id);
                if (existente == null)
                {
                    _logger.LogWarning("Item de inventario no encontrado para actualizar con ID: {Id}", id);
                    return NotFound(new { message = "Item no encontrado" });
                }
                
                var actualizado = await _service.UpdateAsync(inventario);
                return Ok(actualizado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar item de inventario con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar item", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Eliminando item de inventario con ID: {Id}", id);
                
                // Verificar que el item exista
                var existente = await _service.GetByIdAsync(id);
                if (existente == null)
                {
                    _logger.LogWarning("Item de inventario no encontrado para eliminar con ID: {Id}", id);
                    return NotFound(new { message = "Item no encontrado" });
                }
                
                var resultado = await _service.DeleteAsync(id);
                return Ok(new { message = "Item eliminado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar item de inventario con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar item", error = ex.Message });
            }
        }

        [HttpPut("{id}/stock")]
        public async Task<IActionResult> ActualizarStock(int id, [FromBody] int cantidad)
        {
            try
            {
                _logger.LogInformation("Actualizando stock de item con ID: {Id} a {Cantidad}", id, cantidad);
                
                // Verificar que el item exista
                var existente = await _service.GetByIdAsync(id);
                if (existente == null)
                {
                    _logger.LogWarning("Item de inventario no encontrado para actualizar stock con ID: {Id}", id);
                    return NotFound(new { message = "Item no encontrado" });
                }
                
                var resultado = await _service.ActualizarStockAsync(id, cantidad);
                return Ok(new { message = "Stock actualizado correctamente", cantidad });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock de item con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar stock", error = ex.Message });
            }
        }
    }
}