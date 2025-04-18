using back_end.Modules.servicios.DTOs;
using back_end.Modules.servicios.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Modules.servicios.Controllers
{
    [ApiController]
    [Route("api/servicios")]
    public class ServicioController : ControllerBase
    {
        private readonly IServicioService _service;
        private readonly ILogger<ServicioController> _logger;

        public ServicioController(IServicioService service, ILogger<ServicioController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [Authorize]
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

        [Authorize]
        [HttpPost("{correo}")]
        public async Task<IActionResult> Create(string correo, [FromBody] ServicioCreateDTO dto)
        {
            try
            {
                _logger.LogInformation("Solicitud para crear servicio del usuario con correo: {Correo}", correo);
                var nuevoServicio = await _service.CreateAsync(correo, dto);
                return Created("", nuevoServicio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear servicio para el usuario con correo: {Correo}", correo);
                return StatusCode(500, new { message = "Error al crear servicio" });
            }
        }

        [Authorize]
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

        [Authorize]
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
    }
}
