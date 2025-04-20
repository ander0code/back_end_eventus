using back_end.Modules.clientes.DTOs;
using back_end.Modules.clientes.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace back_end.Modules.clientes.Controllers
{
    [ApiController]
    [Route("api/clientes")]
    [Authorize]
    public class ClienteController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClienteController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        // GET: api/clientes/usuario/{correo}
        [HttpGet("usuario/{correo}")]
        public async Task<IActionResult> GetByUsuarioCorreo(string correo)
        {
            var clientes = await _clienteService.GetByUsuarioCorreoAsync(correo);
            return Ok(clientes);
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
            var actualizado = await _clienteService.UpdateAsync(id, dto);
            if (actualizado == null) return NotFound();
            return Ok(actualizado);
        }

        // DELETE: api/clientes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var eliminado = await _clienteService.DeleteAsync(id);
            if (!eliminado) return NotFound();
            return NoContent();
        }
    }
}
