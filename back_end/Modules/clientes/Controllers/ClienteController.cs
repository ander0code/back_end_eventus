using back_end.Modules.clientes.DTOs;
using back_end.Modules.clientes.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("Solicitud para obtener todos los clientes");
                var clientes = await _clienteService.GetAllAsync();
                if (clientes == null || !clientes.Any())
                {
                    return NotFound(new { Message = "No se encontraron clientes", StatusCode = 404 });
                }

                return Ok(clientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los clientes");
                return StatusCode(500, new { Message = "Error al obtener clientes", StatusCode = 500 });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ClienteCreateDTO dto)
        {
            try
            {
                _logger.LogInformation("Solicitud para crear un nuevo cliente");
                var cliente = await _clienteService.CreateAsync(dto);
                if (cliente == null) 
                {
                    return BadRequest(new { Message = "No se pudo crear el cliente", StatusCode = 400 });
                }
                
                return CreatedAtAction(nameof(GetAll), null, cliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el cliente");
                return StatusCode(500, new { Message = "Error al crear cliente", StatusCode = 500 });
            }
        }

        // PUT: api/clientes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] ClienteUpdateDTO dto)
        {
            try
            {
                _logger.LogInformation("Solicitud de actualización para cliente con ID: {Id}", id);
                var actualizado = await _clienteService.UpdateAsync(id, dto);
                if (actualizado == null)
                {
                    _logger.LogWarning("No se encontró el cliente para actualizar con ID: {Id}", id);
                    return NotFound(new { Message = "Cliente no encontrado", StatusCode = 404 });
                }

                return Ok(actualizado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cliente con ID: {Id}", id);
                return StatusCode(500, new { Message = "Error al actualizar cliente", StatusCode = 500 });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                _logger.LogInformation("Solicitud de eliminación para cliente con ID: {Id}", id);
                var eliminado = await _clienteService.DeleteAsync(id);
                if (!eliminado)
                {
                    _logger.LogWarning("No se encontró el cliente para eliminar con ID: {Id}", id);
                    return NotFound(new { Message = "Cliente no encontrado", StatusCode = 404 });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cliente con ID: {Id}", id);
                return StatusCode(500, new { Message = "Error al eliminar cliente", StatusCode = 500 });
            }
        }
    }
}