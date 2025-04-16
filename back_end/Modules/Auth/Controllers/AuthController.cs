using back_end.Modules.Auth.DTOs;
using back_end.Modules.Auth.Services;
using Microsoft.AspNetCore.Mvc;
using back_end.Core.Helpers;

namespace back_end.Modules.Auth.Controllers
{
    [ApiController]
    [Route("api/auth")] // Ruta explícita en minúsculas para consistencia
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

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
                _logger.LogInformation("Intento de login para usuario: {Email}", request.Email);
                var response = _authService.Authenticate(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Autenticación fallida para usuario: {Email}", request.Email);
                return Unauthorized(new ErrorResponseDTO { Message = ex.Message, StatusCode = 401 });
            }
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseDTO), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            try
            {
                _logger.LogInformation("Intento de registro para correo: {Email}", request.Email);
                var response = await _authService.Register(request);
                return StatusCode(201, response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Registro fallido para correo: {Email}", request.Email);
                return BadRequest(new ErrorResponseDTO { Message = ex.Message, StatusCode = 400 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el registro para correo: {Email}", request.Email);
                return StatusCode(500, new ErrorResponseDTO { Message = "Error al procesar el registro", StatusCode = 500 });
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