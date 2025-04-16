using back_end.Modules.Auth.DTOs;
using back_end.Modules.Auth.Services;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Modules.Auth.Controllers
{
    [ApiController]
    [Route("api/auth")] // Ruta explícita en minúsculas para consistencia
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger; // Agregar logging

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status401Unauthorized)]
        public IActionResult Login([FromBody] AuthRequestDTO request)
        {
            try
            {
                _logger.LogInformation("Intento de login para usuario: {Username}", request.Username);
                var response = _authService.Authenticate(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Autenticación fallida para usuario: {Username}", request.Username);
                return Unauthorized(new ErrorResponseDTO { Message = ex.Message });
            }
        }
    }
}

public class ErrorResponseDTO
{
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}