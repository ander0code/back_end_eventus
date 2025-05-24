using back_end.Modules.reservas.DTOs;
using back_end.Modules.reservas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.Modules.reservas.Controllers
{
    [ApiController]
    [Route("api/reservas")]
    [Authorize]
    public class ReservaController : ControllerBase
    {
        private readonly IReservaService _reservaService;
        private readonly ILogger<ReservaController> _logger;

        public ReservaController(IReservaService reservaService, ILogger<ReservaController> logger)
        {
            _reservaService = reservaService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("Obteniendo todas las reservas");
                var reservas = await _reservaService.GetAllAsync();
                if (reservas == null || !reservas.Any())
                    return NotFound(new { message = "No se encontraron reservas" });

                return Ok(reservas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las reservas");
                return StatusCode(500, new { message = "Error interno al obtener las reservas" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                _logger.LogInformation("Obteniendo reserva con ID: {ID}", id);
                var reserva = await _reservaService.GetByIdAsync(id);
                if (reserva == null)
                    return NotFound(new { message = "Reserva no encontrada" });

                return Ok(reserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reserva con ID: {ID}", id);
                return StatusCode(500, new { message = "Error interno al obtener la reserva" });
            }
        }        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReservaCreateDTO dto)
        {
            try
            {
                _logger.LogInformation("Creando nueva reserva");
                var reserva = await _reservaService.CreateAsync(dto);
                if (reserva == null)
                    return BadRequest(new { message = "No se pudo crear la reserva. Verifica que el ClienteId existe y los datos son correctos." });

                // Corregir posible error de conversión, verificando si la ID es Guid o string
                object idParam;
                if (Guid.TryParse(reserva.Id, out Guid guidId))
                    idParam = guidId;
                else
                    idParam = reserva.Id;
                
                return CreatedAtAction(nameof(GetById), new { id = idParam }, reserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear reserva: {Message}", ex.Message);
                return StatusCode(500, new { message = "Error interno al crear la reserva" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ReservaUpdateDTO dto)
        {
            try
            {
                _logger.LogInformation("Actualizando reserva con ID: {ID}", id);
                var reserva = await _reservaService.UpdateAsync(id, dto);
                if (reserva == null)
                    return NotFound(new { message = "Reserva no encontrada" });

                return Ok(reserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar reserva con ID: {ID}", id);
                return StatusCode(500, new { message = "Error interno al actualizar la reserva" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                _logger.LogInformation("Eliminando reserva con ID: {ID}", id);
                var result = await _reservaService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { message = "Reserva no encontrada" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar reserva con ID: {ID}", id);
                return StatusCode(500, new { message = "Error interno al eliminar la reserva" });
            }
        }        // Los endpoints relacionados con servicios y tipos de evento han sido eliminados
        // Solo se mantienen los endpoints básicos de CRUD de reservas
    }
}