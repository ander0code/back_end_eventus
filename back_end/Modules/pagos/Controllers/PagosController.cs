using back_end.Modules.pagos.DTOs;
using back_end.Modules.pagos.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Modules.pagos.Controllers
{
    [ApiController]
    [Route("api/pagos")]
    [Authorize]
    public class PagosController : ControllerBase
    {
        private readonly IPagosService _service;
        private readonly ILogger<PagosController> _logger;

        public PagosController(IPagosService service, ILogger<PagosController> logger)
        {
            _service = service;
            _logger = logger;        }
        
        // GET: api/pagos
        /// <summary>
        /// Obtiene todos los pagos registrados en el sistema
        /// </summary>
        /// <returns>Lista de pagos</returns>
        /// <response code="200">Retorna la lista de pagos</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<PagoResponseDTO>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los pagos");
                var pagos = await _service.GetAllAsync();
                return Ok(pagos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los pagos");
                return StatusCode(500, new { message = "Error al obtener pagos", error = ex.Message });            }
        }
        
        /// <summary>
        /// Obtiene un pago específico por su ID
        /// </summary>
        /// <param name="id">Identificador único del pago</param>
        /// <returns>Información del pago solicitado</returns>
        /// <response code="200">Retorna el pago solicitado</response>
        /// <response code="404">El pago no fue encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        // GET: api/pagos/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PagoResponseDTO), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                _logger.LogInformation("Obteniendo pago con ID: {Id}", id);
                var pago = await _service.GetByIdAsync(id);
                
                if (pago == null)
                {
                    _logger.LogWarning("Pago no encontrado con ID: {Id}", id);
                    return NotFound(new { message = "Pago no encontrado" });
                }
                
                return Ok(pago);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pago con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al obtener pago", error = ex.Message });            }
        }
        
        /// <summary>
        /// Obtiene todos los pagos asociados a una reserva específica
        /// </summary>
        /// <param name="reservaId">Identificador de la reserva</param>
        /// <returns>Lista de pagos de la reserva</returns>
        /// <response code="200">Retorna la lista de pagos de la reserva</response>
        /// <response code="404">No se encontraron pagos para la reserva indicada</response>
        /// <response code="500">Error interno del servidor</response>
        // GET: api/pagos/reserva/{reservaId}
        [HttpGet("reserva/{reservaId}")]
        [ProducesResponseType(typeof(List<PagoResponseDTO>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetByReservaId(string reservaId)
        {
            try
            {
                _logger.LogInformation("Obteniendo pagos para reserva con ID: {ReservaId}", reservaId);
                var pagos = await _service.GetByReservaIdAsync(reservaId);
                
                if (pagos == null || !pagos.Any())
                {
                    _logger.LogWarning("No se encontraron pagos para la reserva con ID: {ReservaId}", reservaId);
                    return NotFound(new { message = "No se encontraron pagos para esta reserva" });
                }
                
                return Ok(pagos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pagos para reserva con ID: {ReservaId}", reservaId);
                return StatusCode(500, new { message = "Error al obtener pagos de la reserva", error = ex.Message });            }
        }
        
        /// <summary>
        /// Registra un nuevo pago en el sistema
        /// </summary>
        /// <param name="dto">Datos del nuevo pago</param>
        /// <returns>Pago creado con su ID generado</returns>
        /// <response code="201">Pago creado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="500">Error interno del servidor</response>
        // POST: api/pagos
        [HttpPost]
        [ProducesResponseType(typeof(PagoResponseDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Create([FromBody] PagoCreateDTO dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.IdReserva))
                {
                    return BadRequest(new { message = "El ID de la reserva es requerido" });
                }
                
                if (string.IsNullOrEmpty(dto.IdTipoPago))
                {
                    return BadRequest(new { message = "El tipo de pago es requerido" });
                }
                
                if (string.IsNullOrEmpty(dto.Monto) || !decimal.TryParse(dto.Monto, out _))
                {
                    return BadRequest(new { message = "El monto debe ser un valor numérico válido" });
                }
                
                _logger.LogInformation("Creando nuevo pago para reserva con ID: {IdReserva}", dto.IdReserva);
                var creado = await _service.CreateAsync(dto);
                
                if (creado == null)
                {
                    return StatusCode(500, new { message = "Error al crear el pago" });
                }
                
                return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear pago para reserva con ID: {IdReserva}", dto.IdReserva);
                return StatusCode(500, new { message = "Error al crear pago", error = ex.Message });            }
        }
        
        /// <summary>
        /// Actualiza un pago existente
        /// </summary>
        /// <param name="id">Identificador único del pago</param>
        /// <param name="dto">Datos actualizados del pago</param>
        /// <returns>Pago actualizado</returns>
        /// <response code="200">Pago actualizado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="404">Pago no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        // PUT: api/pagos/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PagoResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Update(string id, [FromBody] PagoUpdateDTO dto)
        {
            try
            {
                if (!string.IsNullOrEmpty(dto.Monto) && !decimal.TryParse(dto.Monto, out _))
                {
                    return BadRequest(new { message = "El monto debe ser un valor numérico válido" });
                }
                
                _logger.LogInformation("Actualizando pago con ID: {Id}", id);
                var actualizado = await _service.UpdateAsync(id, dto);
                
                if (actualizado == null)
                {
                    _logger.LogWarning("Pago no encontrado con ID: {Id}", id);
                    return NotFound(new { message = "Pago no encontrado" });
                }
                
                return Ok(actualizado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar pago con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar pago", error = ex.Message });            }
        }
        
        /// <summary>
        /// Elimina un pago existente
        /// </summary>
        /// <param name="id">Identificador único del pago</param>
        /// <returns>Sin contenido</returns>
        /// <response code="204">Pago eliminado exitosamente</response>
        /// <response code="404">Pago no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        // DELETE: api/pagos/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                _logger.LogInformation("Eliminando pago con ID: {Id}", id);
                var resultado = await _service.DeleteAsync(id);
                
                if (!resultado)
                {
                    _logger.LogWarning("Pago no encontrado con ID: {Id}", id);
                    return NotFound(new { message = "Pago no encontrado" });
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar pago con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar pago", error = ex.Message });            }
        }
        
        /// <summary>
        /// Obtiene todos los tipos de pago disponibles
        /// </summary>
        /// <returns>Lista de tipos de pago</returns>
        /// <response code="200">Lista de tipos de pago obtenida exitosamente</response>
        /// <response code="500">Error interno del servidor</response>
        // GET: api/pagos/tipos
        [HttpGet("tipos")]
        [ProducesResponseType(typeof(List<TipoPagoDTO>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllTiposPago()
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los tipos de pago");
                var tiposPago = await _service.GetAllTiposPagoAsync();
                return Ok(tiposPago);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos de pago");
                return StatusCode(500, new { message = "Error al obtener tipos de pago", error = ex.Message });            }
        }
        
        /// <summary>
        /// Obtiene un tipo de pago específico por su ID
        /// </summary>
        /// <param name="id">Identificador único del tipo de pago</param>
        /// <returns>Información del tipo de pago</returns>
        /// <response code="200">Tipo de pago encontrado exitosamente</response>
        /// <response code="404">Tipo de pago no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        // GET: api/pagos/tipos/{id}
        [HttpGet("tipos/{id}")]
        [ProducesResponseType(typeof(TipoPagoDTO), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetTipoPagoById(string id)
        {
            try
            {
                _logger.LogInformation("Obteniendo tipo de pago con ID: {Id}", id);
                var tipoPago = await _service.GetTipoPagoByIdAsync(id);
                
                if (tipoPago == null)
                {
                    _logger.LogWarning("Tipo de pago no encontrado con ID: {Id}", id);
                    return NotFound(new { message = "Tipo de pago no encontrado" });
                }
                
                return Ok(tipoPago);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipo de pago con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al obtener tipo de pago", error = ex.Message });
            }
        }
    }
}