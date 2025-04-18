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
                _logger.LogInformation("Solicitud para obtener reservas del usuario con correo: {Correo}", correo);
                var reservas = await _service.GetByCorreoAsync(correo);
                return Ok(reservas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservas para el usuario con correo: {Correo}", correo);
                return StatusCode(500, new { message = "Error al obtener reservas" });
            }
        }

        [Authorize]
        [HttpPost("{correo}")]
        public async Task<IActionResult> Create(string correo, [FromBody] ReservaCreateDTO dto)
        {
            try
            {
                _logger.LogInformation("Solicitud para crear reserva del usuario con correo: {Correo}", correo);
                var nuevaReserva = await _service.CreateAsync(correo, dto);
                return Created("", nuevaReserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear reserva para el usuario con correo: {Correo}", correo);
                return StatusCode(500, new { message = "Error al crear reserva" });
            }
        }

        [Authorize]
        [HttpPut("{correo}/{id:guid}")]
        public async Task<IActionResult> Update(string correo, Guid id, [FromBody] ReservaUpdateDTO dto)
        {
            try
            {
                _logger.LogInformation("Solicitud para actualizar reserva con ID: {Id} del usuario con correo: {Correo}", id, correo);
                var actualizada = await _service.UpdateAsync(correo, id, dto);
                if (actualizada == null)
                {
                    _logger.LogWarning("Reserva no encontrada con ID: {Id} para el usuario con correo: {Correo}", id, correo);
                    return NotFound(new { message = "Reserva no encontrada" });
                }

                return Ok(actualizada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar reserva con ID: {Id} del usuario con correo: {Correo}", id, correo);
                return StatusCode(500, new { message = "Error al actualizar reserva" });
            }
        }

        [Authorize]
        [HttpDelete("{correo}/{id:guid}")]
        public async Task<IActionResult> Delete(string correo, Guid id)
        {
            try
            {
                _logger.LogInformation("Solicitud para eliminar reserva con ID: {Id} del usuario con correo: {Correo}", id, correo);
                var eliminada = await _service.DeleteAsync(correo, id);
                if (!eliminada)
                {
                    _logger.LogWarning("Reserva no encontrada para eliminar con ID: {Id} y correo: {Correo}", id, correo);
                    return NotFound(new { message = "Reserva no encontrada" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar reserva con ID: {Id} del usuario con correo: {Correo}", id, correo);
                return StatusCode(500, new { message = "Error al eliminar reserva" });
            }
        }
    }
}
