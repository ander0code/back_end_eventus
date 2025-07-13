using back_end.Modules.servicios.DTOs;
using back_end.Modules.servicios.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("Solicitud para obtener todos los servicios");
                var servicios = await _service.GetAllAsync();
                return Ok(servicios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener servicios");
                return StatusCode(500, new { message = "Error al obtener servicios" });
            }
        }
        
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                _logger.LogInformation("Solicitud para obtener servicio con ID: {Id}", id);
                var servicio = await _service.GetByIdAsync(id);
                
                if (servicio == null)
                {
                    _logger.LogWarning("Servicio no encontrado con ID: {Id}", id);
                    return NotFound(new { message = "Servicio no encontrado" });
                }
                
                return Ok(servicio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener servicio con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al obtener servicio" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ServicioCreateDTO dto)
        {
            try
            {
                _logger.LogInformation("Solicitud para crear servicio");
                
                if (string.IsNullOrWhiteSpace(dto.NombreServicio))
                {
                    return BadRequest(new { message = "El nombre del servicio es requerido" });
                }
                
                var nuevoServicio = await _service.CreateAsync(dto);
                
                if (nuevoServicio == null)
                {
                    return BadRequest(new { message = "No se pudo crear el servicio" });
                }
                
                return CreatedAtAction(nameof(GetById), new { id = nuevoServicio.Id }, nuevoServicio);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Validación fallida al crear servicio: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear servicio");
                return StatusCode(500, new { message = "Error al crear servicio" });
            }
        }
        
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(string id, [FromBody] ServicioUpdateDTO dto)
        {
            try
            {
                _logger.LogInformation("Solicitud para actualizar servicio con ID: {Id}", id.ToString());
                var actualizado = await _service.UpdateAsync(id, dto);
                if (actualizado == null)
                {
                    _logger.LogWarning("Servicio no encontrado con ID: {Id}", id.ToString());
                    return NotFound(new { message = "Servicio no encontrado" });
                }

                return Ok(actualizado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar servicio con ID: {Id}", id.ToString());
                return StatusCode(500, new { message = "Error al actualizar servicio" });
            }
        }
        
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                _logger.LogInformation("Solicitud para eliminar servicio con ID: {Id}", id.ToString());
                var eliminado = await _service.DeleteAsync(id);
                if (!eliminado)
                {
                    _logger.LogWarning("Servicio no encontrado para eliminar con ID: {Id}", id.ToString());
                    return NotFound(new { message = "Servicio no encontrado" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar servicio con ID: {Id}", id.ToString());
                return StatusCode(500, new { message = "Error al eliminar servicio" });
            }
        }
    }
}
