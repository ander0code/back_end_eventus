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
        [HttpGet("{correo}")]
        public async Task<IActionResult> GetByCorreo(string correo)
        {
            try
            {
                _logger.LogInformation("Solicitud para obtener usuario con correo: {Correo}", correo);
                var usuario = await _service.GetByCorreoAsync(correo);
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado con correo: {Correo}", correo);
                    return NotFound(new ErrorResponseDTO { Message = "Usuario no encontrado", StatusCode = 404 });
                }

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el usuario con correo: {Correo}", correo);
                return StatusCode(500, new ErrorResponseDTO { Message = "Error al obtener usuario", StatusCode = 500 });
            }
        }

        [Authorize]
        [HttpPut("{correo}")]
        public async Task<IActionResult> UpdateByCorreo(string correo, [FromBody] UsuarioUpdateDTO dto)
        {
            try
            {
                _logger.LogInformation("Solicitud de actualización para usuario con correo: {Correo}", correo);
                var actualizado = await _service.UpdateByCorreoAsync(correo, dto);
                if (actualizado == null)
                {
                    _logger.LogWarning("No se encontró el usuario para actualizar con correo: {Correo}", correo);
                    return NotFound(new ErrorResponseDTO { Message = "Usuario no encontrado", StatusCode = 404 });
                }

                return Ok(actualizado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el usuario con correo: {Correo}", correo);
                return StatusCode(500, new ErrorResponseDTO { Message = "Error al actualizar usuario", StatusCode = 500 });
            }
        }
    }
}
