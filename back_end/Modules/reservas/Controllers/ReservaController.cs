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
        }

        // Endpoint para agregar servicio a una reserva
        [HttpPost("{reservaId}/servicios/{servicioId}")]
        public async Task<IActionResult> AddServicioToReserva(Guid reservaId, Guid servicioId)
        {
            try
            {
                _logger.LogInformation("Agregando servicio {ServicioID} a reserva {ReservaID}", servicioId, reservaId);
                var result = await _reservaService.AddServicioToReservaAsync(reservaId, servicioId);
                if (!result)
                    return NotFound(new { message = "No se pudo agregar el servicio a la reserva" });

                return Ok(new { message = "Servicio agregado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar servicio {ServicioID} a reserva {ReservaID}", servicioId, reservaId);
                return StatusCode(500, new { message = "Error interno al agregar el servicio" });
            }
        }

        // Endpoint para eliminar un servicio de una reserva
        [HttpDelete("{reservaId}/servicios/{servicioId}")]
        public async Task<IActionResult> RemoveServicioFromReserva(Guid reservaId, Guid servicioId)
        {
            try
            {
                _logger.LogInformation("Eliminando servicio {ServicioID} de reserva {ReservaID}", servicioId, reservaId);
                var result = await _reservaService.RemoveServicioFromReservaAsync(reservaId, servicioId);
                if (!result)
                    return NotFound(new { message = "No se pudo eliminar el servicio de la reserva" });

                return Ok(new { message = "Servicio eliminado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar servicio {ServicioID} de reserva {ReservaID}", servicioId, reservaId);
                return StatusCode(500, new { message = "Error interno al eliminar el servicio" });
            }
        }

        // Endpoint para actualizar un servicio en una reserva
        [HttpPut("{reservaId}/servicios/{servicioId}")]
        public async Task<IActionResult> UpdateServicioInReserva(Guid reservaId, Guid servicioId, [FromQuery] int cantidad, [FromQuery] decimal? precio)
        {
            try
            {
                _logger.LogInformation("Actualizando servicio {ServicioID} en reserva {ReservaID}", servicioId, reservaId);
                var result = await _reservaService.UpdateServicioInReservaAsync(reservaId, servicioId, cantidad, precio);
                if (!result)
                    return NotFound(new { message = "No se pudo actualizar el servicio en la reserva" });

                return Ok(new { message = "Servicio actualizado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar servicio {ServicioID} en reserva {ReservaID}", servicioId, reservaId);
                return StatusCode(500, new { message = "Error interno al actualizar el servicio" });
            }
        }

        // Endpoint para calcular el total de una reserva
        [HttpGet("{reservaId}/total")]
        public async Task<IActionResult> CalcularTotalReserva(Guid reservaId)
        {
            try
            {
                _logger.LogInformation("Calculando total para reserva {ReservaID}", reservaId);
                var total = await _reservaService.CalcularTotalReservaAsync(reservaId);
                return Ok(new { total });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular total para reserva {ReservaID}", reservaId);
                return StatusCode(500, new { message = "Error interno al calcular el total" });
            }
        }        // Endpoint para reemplazar un servicio por otro en una reserva
        [HttpPut("{reservaId}/servicios/{oldServicioId}/replace/{newServicioId}")]
        public async Task<IActionResult> ReplaceServicioInReserva(
            Guid reservaId, 
            Guid oldServicioId, 
            Guid newServicioId, 
            [FromQuery] int? cantidad, 
            [FromQuery] decimal? precio)
        {
            try
            {
                _logger.LogInformation("Reemplazando servicio {OldServicioID} por {NewServicioID} en reserva {ReservaID}", 
                    oldServicioId, newServicioId, reservaId);
                    
                var result = await _reservaService.ReplaceServicioInReservaAsync(
                    reservaId, oldServicioId, newServicioId, cantidad, precio);
                    
                if (!result)
                    return NotFound(new { message = "No se pudo reemplazar el servicio en la reserva" });

                return Ok(new { message = "Servicio reemplazado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reemplazar servicio en reserva {ReservaID}", reservaId);
                return StatusCode(500, new { message = "Error interno al reemplazar el servicio" });
            }
        }

        // Endpoint para obtener tipos de evento
        [HttpGet("tipos-evento")]
        public async Task<IActionResult> GetTiposEvento()
        {
            try
            {
                _logger.LogInformation("Obteniendo tipos de evento");
                var tiposEvento = await _reservaService.GetAllTiposEventoAsync();
                return Ok(tiposEvento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos de evento");
                return StatusCode(500, new { message = "Error interno al obtener tipos de evento" });
            }
        }

        // Endpoint para obtener tipo de evento por ID
        [HttpGet("tipos-evento/{id:guid}")]
        public async Task<IActionResult> GetTipoEventoById(Guid id)
        {
            try
            {
                _logger.LogInformation("Obteniendo tipo de evento con ID: {ID}", id);
                var tipoEvento = await _reservaService.GetTipoEventoByIdAsync(id);
                if (tipoEvento == null)
                    return NotFound(new { message = "Tipo de evento no encontrado" });

                return Ok(tipoEvento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipo de evento con ID: {ID}", id);
                return StatusCode(500, new { message = "Error interno al obtener tipo de evento" });
            }
        }
    }
}