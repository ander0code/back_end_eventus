using back_end.Modules.reservas.DTOs;
using back_end.Modules.reservas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Modules.reservas.Controllers
{
    [ApiController]
    [Route("api/reservas")]
    public class ReservaController : ControllerBase
    {
        private readonly IReservaService _service;
        private readonly ILogger<ReservaController> _logger;

        public ReservaController(IReservaService service, ILogger<ReservaController> logger)
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
                _logger.LogInformation("Obteniendo reservas para usuario con correo {Correo}", correo);
                var reservas = await _service.GetByCorreoAsync(correo);
                return Ok(reservas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservas para correo {Correo}", correo);
                return StatusCode(500, new { message = "Error al obtener reservas", error = ex.Message });
            }
        }
        
        [Authorize]
        [HttpGet("{correo}/{id}")]
        public async Task<IActionResult> GetById(string correo, string id)
        {
            try
            {
                _logger.LogInformation("Obteniendo reserva con ID {Id} para usuario con correo {Correo}", id, correo);
                var reserva = await _service.GetByIdAsync(correo, id);
                
                if (reserva == null)
                {
                    _logger.LogWarning("Reserva no encontrada con ID {Id} para correo {Correo}", id, correo);
                    return NotFound(new { message = "Reserva no encontrada" });
                }
                
                return Ok(reserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reserva con ID {Id} para correo {Correo}", id, correo);
                return StatusCode(500, new { message = "Error al obtener reserva", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("{correo}")]
        public async Task<IActionResult> Create(string correo, [FromBody] ReservaCreateDTO dto)
        {
            try
            {
                _logger.LogInformation("Creando nueva reserva para usuario con correo {Correo}", correo);
                var creada = await _service.CreateAsync(correo, dto);
                
                if (creada == null)
                {
                    _logger.LogWarning("Error al crear reserva para correo {Correo}", correo);
                    return BadRequest(new { message = "Error al crear reserva" });
                }
                
                return CreatedAtAction(nameof(GetById), new { correo, id = creada.Id }, creada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear reserva para correo {Correo}", correo);
                return StatusCode(500, new { message = "Error al crear reserva", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("{correo}/{id}")]
        public async Task<IActionResult> Update(string correo, string id, [FromBody] ReservaUpdateDTO dto)
        {
            try
            {
                _logger.LogInformation("Actualizando reserva con ID {Id} para usuario con correo {Correo}", id, correo);
                var actualizada = await _service.UpdateAsync(correo, id, dto);
                
                if (actualizada == null)
                {
                    _logger.LogWarning("Reserva no encontrada con ID {Id} para correo {Correo}", id, correo);
                    return NotFound(new { message = "Reserva no encontrada" });
                }
                
                return Ok(actualizada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar reserva con ID {Id} para correo {Correo}", id, correo);
                return StatusCode(500, new { message = "Error al actualizar reserva", error = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("{correo}/{id}")]
        public async Task<IActionResult> Delete(string correo, string id)
        {
            try
            {
                _logger.LogInformation("Eliminando reserva con ID {Id} para usuario con correo {Correo}", id, correo);
                var resultado = await _service.DeleteAsync(correo, id);
                
                if (!resultado)
                {
                    _logger.LogWarning("Reserva no encontrada con ID {Id} para correo {Correo}", id, correo);
                    return NotFound(new { message = "Reserva no encontrada" });
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar reserva con ID {Id} para correo {Correo}", id, correo);
                return StatusCode(500, new { message = "Error al eliminar reserva", error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("tipos-evento")]
        public async Task<IActionResult> GetTiposEvento()
        {
            try
            {
                _logger.LogInformation("Obteniendo tipos de evento");
                var tiposEvento = await _service.GetAllTiposEventoAsync();
                return Ok(tiposEvento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos de evento");
                return StatusCode(500, new { message = "Error al obtener tipos de evento", error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("tipos-evento/{id:guid}")]
        public async Task<IActionResult> GetTipoEventoById(Guid id)
        {
            try
            {
                _logger.LogInformation("Obteniendo tipo de evento con ID {Id}", id);
                var tipoEvento = await _service.GetTipoEventoByIdAsync(id);
                
                if (tipoEvento == null)
                {
                    _logger.LogWarning("Tipo de evento no encontrado con ID {Id}", id);
                    return NotFound(new { message = "Tipo de evento no encontrado" });
                }
                
                return Ok(tipoEvento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipo de evento con ID {Id}", id);
                return StatusCode(500, new { message = "Error al obtener tipo de evento", error = ex.Message });
            }
        }
    }
}
