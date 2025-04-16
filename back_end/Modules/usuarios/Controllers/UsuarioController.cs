using back_end.Modules.usuarios.DTOs;
using back_end.Modules.usuarios.services;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Modules.usuarios.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _service;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(IUsuarioService service, ILogger<UsuarioController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<UsuarioResponseDTO>>> GetAll()
        {
            var usuarios = await _service.GetAllAsync();
            return Ok(usuarios);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioResponseDTO>> GetById(int id)
        {
            var usuario = await _service.GetByIdAsync(id);
            if (usuario == null)
                return NotFound();

            return Ok(usuario);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UsuarioResponseDTO>> Update(int id, [FromBody] UsuarioUpdateDTO dto)
        {
            var actualizado = await _service.UpdateAsync(id, dto);
            if (actualizado == null)
                return NotFound();

            return Ok(actualizado);
        }
    }
}
