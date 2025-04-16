using back_end.Modules.usuarios.DTOs;
using back_end.Modules.usuarios.services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("Solicitando todos los usuarios.");
                var usuarios = await _service.GetAllAsync();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios.");
                return StatusCode(500, new ErrorResponseDTO { Message = "Error al obtener usuarios", StatusCode = 500 });
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Solicitud para obtener usuario con ID: {Id}", id);
                var usuario = await _service.GetByIdAsync(id);
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado con ID: {Id}", id);
                    return NotFound(new ErrorResponseDTO { Message = "Usuario no encontrado", StatusCode = 404 });
                }

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el usuario con ID: {Id}", id);
                return StatusCode(500, new ErrorResponseDTO { Message = "Error al obtener usuario", StatusCode = 500 });
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UsuarioUpdateDTO dto)
        {
            try
            {
                _logger.LogInformation("Solicitud de actualización para usuario ID: {Id}", id);
                var actualizado = await _service.UpdateAsync(id, dto);
                if (actualizado == null)
                {
                    _logger.LogWarning("No se encontró el usuario para actualizar con ID: {Id}", id);
                    return NotFound(new ErrorResponseDTO { Message = "Usuario no encontrado", StatusCode = 404 });
                }

                return Ok(actualizado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el usuario con ID: {Id}", id);
                return StatusCode(500, new ErrorResponseDTO { Message = "Error al actualizar usuario", StatusCode = 500 });
            }
        }
    }
}
