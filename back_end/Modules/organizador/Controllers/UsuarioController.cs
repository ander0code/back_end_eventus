using back_end.Modules.organizador.DTOs;
using back_end.Modules.organizador.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Modules.organizador.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    [Authorize]
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
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los usuarios");
                var usuarios = await _service.GetAllAsync();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios");
                return StatusCode(500, new { message = "Error al obtener usuarios", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                _logger.LogInformation("Obteniendo usuario con ID: {Id}", id);
                var usuario = await _service.GetByIdAsync(id);
                
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado con ID: {Id}", id);
                    return NotFound(new { message = "Usuario no encontrado" });
                }
                
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al obtener usuario", error = ex.Message });
            }
        }

        [HttpGet("correo/{correo}")]
        public async Task<IActionResult> GetByCorreo(string correo)
        {
            try
            {
                _logger.LogInformation("Obteniendo usuario con correo: {Correo}", correo);
                var usuario = await _service.GetByCorreoAsync(correo);
                
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado con correo: {Correo}", correo);
                    return NotFound(new { message = "Usuario no encontrado" });
                }
                
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario con correo: {Correo}", correo);
                return StatusCode(500, new { message = "Error al obtener usuario", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UsuarioUpdateDTO dto)
        {
            try
            {
                _logger.LogInformation("Actualizando usuario con ID: {Id}", id);
                var actualizado = await _service.UpdateAsync(id, dto);
                
                if (actualizado == null)
                {
                    _logger.LogWarning("Usuario no encontrado con ID: {Id}", id);
                    return NotFound(new { message = "Usuario no encontrado" });
                }
                
                return Ok(actualizado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar usuario", error = ex.Message });
            }
        }
    }
}