using back_end.Modules.clientes.DTOs;
using back_end.Modules.clientes.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace back_end.Modules.clientes.Controllers
{
    [ApiController]
    [Route("api/clientes")]
    [Authorize]
    public class ClienteController : ControllerBase
    {
        private readonly IClienteService _clienteService;
        private readonly ILogger<ClienteController> _logger;

        public ClienteController(IClienteService clienteService, ILogger<ClienteController> logger)
        {
            _clienteService = clienteService;
            _logger = logger;
        }

        [HttpGet("usuario/{correo}")]
        public async Task<IActionResult> GetByUsuarioCorreo(string correo)
        {
            var cliente = await _clienteService.GetByUsuarioCorreoAsync(correo); // Cambié a GetByUsuarioCorreoAsync
            if (cliente == null)
            {
                return NotFound(new ErrorResponseDTO { Message = "Cliente no encontrado", StatusCode = 404 });
            }

            return Ok(cliente);
        }

        // POST: api/clientes/{correo}
        [HttpPost("{correo}")]
        public async Task<IActionResult> Create(string correo, [FromBody] ClienteCreateDTO dto)
        {
            var cliente = await _clienteService.CreateAsync(correo, dto);
            if (cliente == null) return BadRequest("Usuario no encontrado");
            return CreatedAtAction(nameof(GetByUsuarioCorreo), new { correo = correo }, cliente);
        }

        // PUT: api/clientes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ClienteUpdateDTO dto)
        {
            try
            {
                _logger.LogInformation("Solicitud de actualización para cliente con ID: {Id}", id);
                var actualizado = await _clienteService.UpdateAsync(id, dto);
                if (actualizado == null)
                {
                    _logger.LogWarning("No se encontró el cliente para actualizar con ID: {Id}", id);
                    return NotFound(new ErrorResponseDTO { Message = "Cliente no encontrado", StatusCode = 404 });
                }

                return Ok(actualizado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cliente con ID: {Id}", id);
                return StatusCode(500, new ErrorResponseDTO { Message = "Error al actualizar cliente", StatusCode = 500 });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                _logger.LogInformation("Solicitud de eliminación para cliente con ID: {Id}", id);
                var eliminado = await _clienteService.DeleteAsync(id);
                if (!eliminado)
                {
                    _logger.LogWarning("No se encontró el cliente para eliminar con ID: {Id}", id);
                    return NotFound(new ErrorResponseDTO { Message = "Cliente no encontrado", StatusCode = 404 });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cliente con ID: {Id}", id);
                return StatusCode(500, new ErrorResponseDTO { Message = "Error al eliminar cliente", StatusCode = 500 });
            }
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> Search([FromQuery] string? query)
        {
            try
            {
                _logger.LogInformation("Solicitud para buscar clientes con el término: {Query}", query);
                var clientes = await _clienteService.SearchAsync(query);
                return Ok(clientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar clientes con el término: {Query}", query);
                return StatusCode(500, new ErrorResponseDTO { Message = "Error al buscar clientes", StatusCode = 500 });
            }
        }

        [HttpGet("filtrar")]
        public async Task<IActionResult> FilterByTipoCliente([FromQuery] string tipoCliente)
        {
            try
            {
                _logger.LogInformation("Solicitud para filtrar clientes por tipo: {TipoCliente}", tipoCliente);
                var clientes = await _clienteService.FilterByTipoClienteAsync(tipoCliente);
                return Ok(clientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al filtrar clientes por tipo: {TipoCliente}", tipoCliente);
                return StatusCode(500, new ErrorResponseDTO { Message = "Error al filtrar clientes", StatusCode = 500 });
            }
        }
    }
}