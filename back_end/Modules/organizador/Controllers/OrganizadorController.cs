using back_end.Modules.organizador.DTOs;
using back_end.Modules.organizador.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Modules.organizador.Controllers
{
    [ApiController]
    [Route("api/organizadores")]
    [Authorize]
    public class OrganizadorController : ControllerBase
    {
        private readonly IOrganizadorService _service;
        private readonly ILogger<OrganizadorController> _logger;

        public OrganizadorController(IOrganizadorService service, ILogger<OrganizadorController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los organizadores");
                var organizadores = await _service.GetAllAsync();
                return Ok(organizadores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los organizadores");
                return StatusCode(500, new { message = "Error al obtener organizadores", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                _logger.LogInformation("Obteniendo organizador con ID: {Id}", id);
                var organizador = await _service.GetByIdAsync(id);
                
                if (organizador == null)
                {
                    _logger.LogWarning("Organizador no encontrado con ID: {Id}", id);
                    return NotFound(new { message = "Organizador no encontrado" });
                }
                
                return Ok(organizador);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener organizador con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al obtener organizador", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> GetByUsuarioId(string usuarioId)
        {
            try
            {
                _logger.LogInformation("Obteniendo organizador para usuario con ID: {UsuarioId}", usuarioId);
                var organizador = await _service.GetByUsuarioIdAsync(usuarioId);
                
                if (organizador == null)
                {
                    _logger.LogWarning("Organizador no encontrado para usuario con ID: {UsuarioId}", usuarioId);
                    return NotFound(new { message = "Organizador no encontrado" });
                }
                
                return Ok(organizador);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener organizador para usuario con ID: {UsuarioId}", usuarioId);
                return StatusCode(500, new { message = "Error al obtener organizador", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrganizadorCreateDTO dto)
        {
            try
            {
                _logger.LogInformation("Creando nuevo organizador");
                
                // Validación básica
                if (string.IsNullOrWhiteSpace(dto.Correo))
                    return BadRequest(new { message = "El correo es requerido" });
                
                if (string.IsNullOrWhiteSpace(dto.Contrasena))
                    return BadRequest(new { message = "La contraseña es requerida" });
                
                var creado = await _service.CreateAsync(dto);
                
                if (creado == null)
                {
                    _logger.LogWarning("Error al crear organizador");
                    return BadRequest(new { message = "No se pudo crear el organizador" });
                }
                
                return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear organizador");
                return StatusCode(500, new { message = "Error al crear organizador", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] OrganizadorUpdateDTO dto)
        {
            try
            {
                _logger.LogInformation("Actualizando organizador con ID: {Id}", id);
                var actualizado = await _service.UpdateAsync(id, dto);
                
                if (actualizado == null)
                {
                    _logger.LogWarning("Organizador no encontrado con ID: {Id}", id);
                    return NotFound(new { message = "Organizador no encontrado" });
                }
                
                return Ok(actualizado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar organizador con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar organizador", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                _logger.LogInformation("Eliminando organizador con ID: {Id}", id);
                var resultado = await _service.DeleteAsync(id);
                
                if (!resultado)
                {
                    _logger.LogWarning("Organizador no encontrado con ID: {Id}", id);
                    return NotFound(new { message = "Organizador no encontrado" });
                }
                
                return Ok(new { message = "Organizador eliminado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar organizador con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar organizador", error = ex.Message });
            }
        }
    }
}
