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
        
        // Endpoints para gestionar servicios dentro de una reserva
        
        [Authorize]
        [HttpPost("{correo}/{reservaId:guid}/servicios/{servicioId:guid}")]
        public async Task<IActionResult> AddServicio(string correo, Guid reservaId, Guid servicioId, [FromBody] ServicioReservaDTO dto)
        {
            try
            {
                _logger.LogInformation("Agregando servicio {ServicioId} a la reserva {ReservaId}", servicioId, reservaId);
                
                int cantidad = dto.Cantidad ?? 1;
                decimal? precio = dto.Precio;
                
                var resultado = await _service.AddServicioToReservaAsync(correo, reservaId, servicioId, cantidad, precio);
                
                if (!resultado)
                {
                    return NotFound(new { message = "Reserva o servicio no encontrado" });
                }
                
                return Ok(new { 
                    message = "Servicio agregado correctamente a la reserva",
                    reservaId,
                    servicioId,
                    cantidad,
                    precio
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar servicio {ServicioId} a la reserva {ReservaId}", servicioId, reservaId);
                return StatusCode(500, new { message = "Error al agregar servicio a la reserva" });
            }
        }
        
        [Authorize]
        [HttpPut("{correo}/{reservaId:guid}/servicios/{servicioId:guid}")]
        public async Task<IActionResult> UpdateServicio(string correo, Guid reservaId, Guid servicioId, [FromBody] ServicioReservaDTO dto)
        {
            try
            {
                _logger.LogInformation("Actualizando servicio {ServicioId} en la reserva {ReservaId}", servicioId, reservaId);
                
                if (dto.Cantidad <= 0)
                {
                    return BadRequest(new { message = "La cantidad debe ser mayor a cero" });
                }
                
                var resultado = await _service.UpdateServicioInReservaAsync(correo, reservaId, servicioId, dto.Cantidad ?? 1, dto.Precio);
                
                if (!resultado)
                {
                    return NotFound(new { message = "Reserva o servicio no encontrado" });
                }
                
                // Recalcular y obtener el total actualizado
                var total = await _service.CalcularTotalReservaAsync(correo, reservaId);
                
                return Ok(new { 
                    message = "Servicio actualizado correctamente en la reserva",
                    reservaId,
                    servicioId,
                    cantidad = dto.Cantidad,
                    precio = dto.Precio,
                    totalReserva = total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar servicio {ServicioId} en la reserva {ReservaId}", servicioId, reservaId);
                return StatusCode(500, new { message = "Error al actualizar servicio en la reserva" });
            }
        }
        
        [Authorize]
        [HttpDelete("{correo}/{reservaId:guid}/servicios/{servicioId:guid}")]
        public async Task<IActionResult> RemoveServicio(string correo, Guid reservaId, Guid servicioId)
        {
            try
            {
                _logger.LogInformation("Eliminando servicio {ServicioId} de la reserva {ReservaId}", servicioId, reservaId);
                
                var resultado = await _service.RemoveServicioFromReservaAsync(correo, reservaId, servicioId);
                
                if (!resultado)
                {
                    return NotFound(new { message = "Reserva o servicio no encontrado" });
                }
                
                // Recalcular y obtener el total actualizado
                var total = await _service.CalcularTotalReservaAsync(correo, reservaId);
                
                return Ok(new { 
                    message = "Servicio eliminado correctamente de la reserva",
                    totalReserva = total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar servicio {ServicioId} de la reserva {ReservaId}", servicioId, reservaId);
                return StatusCode(500, new { message = "Error al eliminar servicio de la reserva" });
            }
        }
        
        [Authorize]
        [HttpGet("{correo}/{reservaId:guid}/total")]
        public async Task<IActionResult> CalcularTotal(string correo, Guid reservaId)
        {
            try
            {
                _logger.LogInformation("Calculando total para la reserva {ReservaId}", reservaId);
                
                var total = await _service.CalcularTotalReservaAsync(correo, reservaId);
                
                return Ok(new { 
                    reservaId,
                    total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular total para la reserva {ReservaId}", reservaId);
                return StatusCode(500, new { message = "Error al calcular total de la reserva" });
            }
        }
    }
    
    // DTO para operaciones con servicios en reservas
    public class ServicioReservaDTO
    {
        public int? Cantidad { get; set; } = 1;
        public decimal? Precio { get; set; }
    }
}
