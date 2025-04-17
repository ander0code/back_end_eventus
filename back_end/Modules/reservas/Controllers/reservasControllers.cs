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
            var reservas = await _service.GetByCorreoAsync(correo);
            return Ok(reservas);
        }

        [Authorize]
        [HttpPost("{correo}")]
        public async Task<IActionResult> Create(string correo, [FromBody] ReservaCreateDTO dto)
        {
            var nuevaReserva = await _service.CreateAsync(correo, dto);
            return Created("", nuevaReserva);
        }

        [Authorize]
        [HttpPut("{correo}/{id}")]
        public async Task<IActionResult> Update(string correo, int id, [FromBody] ReservaUpdateDTO dto)
        {
            var actualizada = await _service.UpdateAsync(correo, id, dto);
            if (actualizada == null) return NotFound();
            return Ok(actualizada);
        }

        [Authorize]
        [HttpDelete("{correo}/{id}")]
        public async Task<IActionResult> Delete(string correo, int id)
        {
            var eliminada = await _service.DeleteAsync(correo, id);
            if (!eliminada) return NotFound();
            return NoContent();
        }
    }
}
