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
        [HttpGet("{correo}/{id:guid}")]
        public async Task<IActionResult> GetById(string correo, Guid id)
        {
            try
            {
                _logger.LogInformation("Solicitud para obtener reserva con ID: {Id} del usuario con correo: {Correo}", id, correo);
                var reserva = await _service.GetByIdAsync(correo, id);
                
                if (reserva == null)
                {
                    _logger.LogWarning("Reserva no encontrada con ID: {Id} para el correo: {Correo}", id, correo);
                    return NotFound(new { message = "Reserva no encontrada" });
                }
                
                return Ok(reserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reserva con ID: {Id} para correo: {Correo}", id, correo);
                return StatusCode(500, new { message = "Error al obtener reserva" });
            }
        }

        [Authorize]
        [HttpPost("{correo}")]
        public async Task<IActionResult> Create(string correo, [FromBody] ReservaCreateDTO dto)
        {
            try
            {
                _logger.LogInformation("Solicitud para crear reserva del usuario con correo: {Correo}", correo);
                
                // Validación de datos mínimos requeridos
                if (string.IsNullOrWhiteSpace(dto.NombreEvento))
                {
                    return BadRequest(new { message = "El nombre del evento es obligatorio" });
                }
                
                // Validar cliente - debe proporcionar un ClienteId existente o datos para crear uno nuevo
                if (!dto.ClienteId.HasValue && 
                    (string.IsNullOrWhiteSpace(dto.NombreCliente) || string.IsNullOrWhiteSpace(dto.CorreoCliente)))
                {
                    return BadRequest(new { 
                        message = "Debe proporcionar un ClienteId existente o los datos para crear un cliente nuevo (Nombre y Correo)" 
                    });
                }
                
                // Validar que se proporcionen servicios
                if (dto.Servicios == null || !dto.Servicios.Any())
                {
                    return BadRequest(new { message = "Debe proporcionar al menos un servicio para la reserva" });
                }
                
                var nuevaReserva = await _service.CreateAsync(correo, dto);
                
                if (nuevaReserva == null)
                {
                    if (dto.ClienteId.HasValue)
                    {
                        return NotFound(new { message = "No se pudo crear la reserva. Usuario o cliente no encontrado." });
                    }
                    return StatusCode(500, new { message = "Error al crear la reserva o el cliente" });
                }
                
                // Determinar si se creó un cliente nuevo o se usó uno existente
                string clienteInfo = dto.ClienteId.HasValue 
                    ? $"Cliente existente con ID: {dto.ClienteId}"
                    : $"Nuevo cliente creado: {dto.NombreCliente}";
                
                return Created($"api/reservas/{correo}/{nuevaReserva.Id}", new {
                    message = "Reserva creada correctamente",
                    clienteInfo,
                    reserva = nuevaReserva
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear reserva para el usuario con correo: {Correo}", correo);
                return StatusCode(500, new { message = "Error al crear reserva: " + ex.Message });
            }
        }

        [Authorize]
        [HttpPut("{correo}/{id:guid}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(string correo, Guid id, [FromBody] ReservaUpdateDTO dto)
        {
            try
            {
                _logger.LogInformation("Solicitud para actualizar reserva con ID: {Id} del usuario con correo: {Correo}", id, correo);
                
                // Validar operaciones de agregar/remover servicios
                if (dto.ItemsToAdd != null && dto.ItemsToAdd.Any())
                {
                    _logger.LogInformation("Se agregarán {Count} servicios a la reserva {Id}", dto.ItemsToAdd.Count, id);
                    
                    // Validar que no haya IDs de servicio inválidos (por ejemplo, Guid vacío)
                    if (dto.ItemsToAdd.Any(item => item.ServicioId == Guid.Empty))
                    {
                        return BadRequest(new { message = "Hay IDs de servicio inválidos en la lista de servicios a agregar" });
                    }
                }
                
                if (dto.ItemsToRemove != null && dto.ItemsToRemove.Any())
                {
                    _logger.LogInformation("Se eliminarán {Count} servicios de la reserva {Id}", dto.ItemsToRemove.Count, id);
                    
                    // Validar que no haya IDs de servicio inválidos (por ejemplo, Guid vacío)
                    if (dto.ItemsToRemove.Any(servicioId => servicioId == Guid.Empty))
                    {
                        return BadRequest(new { message = "Hay IDs de servicio inválidos en la lista de servicios a eliminar" });
                    }
                }
                
                var actualizada = await _service.UpdateAsync(correo, id, dto);
                
                if (actualizada == null)
                {
                    _logger.LogWarning("Reserva no encontrada con ID: {Id} para el usuario con correo: {Correo}", id, correo);
                    return NotFound(new { message = "Reserva no encontrada" });
                }

                return Ok(new {
                    message = "Reserva actualizada correctamente",
                    reserva = actualizada
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar reserva con ID: {Id} del usuario con correo: {Correo}", id, correo);
                return StatusCode(500, new { message = "Error al actualizar reserva: " + ex.Message });
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
