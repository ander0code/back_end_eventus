using back_end.Core.Configurations;
using back_end.Modules.Auth.Repositories;
using back_end.Modules.Auth.DTOs;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace back_end.Modules.Auth.Services
{
    public interface IAuthService
    {
        AuthResponseDTO Authenticate(AuthRequestDTO request);
        Task<RegisterResponseDTO> Register(RegisterRequestDTO request);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly AppSettings _appSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository userRepository, AppSettings appSettings, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _appSettings = appSettings;
            _logger = logger;
        }

        public AuthResponseDTO Authenticate(AuthRequestDTO request)
        {
            var user = _userRepository.GetUserByEmail(request.Email);

            if (user == null)
            {
                _logger.LogWarning("Intento de login fallido: Usuario no encontrado: {Email}", request.Email);
                throw new UnauthorizedAccessException("El correo electrónico no está registrado en el sistema");
            }
            
            if (!VerifyPasswordHash(request.Password, user.ContrasenaHash))
            {
                _logger.LogWarning("Intento de login fallido: Contraseña incorrecta para: {Email}", request.Email);
                throw new UnauthorizedAccessException("La contraseña ingresada es incorrecta");
            }

            var token = GenerateJwtToken(user);

            _logger.LogInformation("Login exitoso para: {Email}", request.Email);            return new AuthResponseDTO
            {
                Email = user.Correo,
                Token = token,
                UserId = user.Id, // Usamos el string directamente
                Nombre = user.Nombre,
                Apellido = user.Apellido
            };
        }

        public async Task<RegisterResponseDTO> Register(RegisterRequestDTO request)
        {
            if (await _userRepository.ExistsByEmail(request.Email))
            {
                _logger.LogWarning("Intento de registro fallido: Email ya existe: {Email}", request.Email);
                throw new InvalidOperationException("El correo electrónico ya está registrado");
            }

            var newUser = await _userRepository.RegisterUser(request);
            _logger.LogInformation("Usuario registrado correctamente: {Email}", request.Email);
            
            var token = GenerateJwtToken(newUser);
            _logger.LogInformation("Token generado para usuario nuevo: {Email}", request.Email);            return new RegisterResponseDTO
            {
                UserId = newUser.Id, // Ahora usamos directamente el string
                Email = newUser.Correo,
                Token = token,
                Nombre = newUser.Nombre,
                Apellido = newUser.Apellido
            };
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            try
            {
                _logger.LogDebug("Verificando contraseña para hash: {HashPrefix}...", 
                    storedHash.Length > 10 ? storedHash.Substring(0, 10) + "..." : storedHash);
                
                if (storedHash.StartsWith("$2a$") || storedHash.StartsWith("$2b$") || storedHash.StartsWith("$2y$"))
                {
                    bool result = BCrypt.Net.BCrypt.Verify(password, storedHash);
                    _logger.LogDebug("Verificación BCrypt: {Result}", result ? "Exitosa" : "Fallida");
                    return result;
                }
                
                // Método de transición - si el hash está en Base64 (hash antiguo)
                try
                {
                    byte[] hashBytes = Convert.FromBase64String(storedHash);
                    string decodedHash = Encoding.UTF8.GetString(hashBytes);
                    bool result = password == decodedHash;
                    _logger.LogDebug("Verificación Base64: {Result}", result ? "Exitosa" : "Fallida");
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("Error en decodificación Base64: {Message}", ex.Message);
                    // Si no es un hash Base64 ni BCrypt, es probablemente texto plano (solo para desarrollo)
                    bool result = password == storedHash;
                    _logger.LogDebug("Verificación texto plano: {Result}", result ? "Exitosa" : "Fallida");
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en verificación de contraseña");
                return false;
            }
        }

        private string GenerateJwtToken(UsuarioAuthDTO user)
        {
            // Usando la clase AppSettings para acceder a las configuraciones
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Correo),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("name", $"{user.Nombre} {user.Apellido}")
            };
            
            var token = new JwtSecurityToken(
                issuer: _appSettings.JwtIssuer,
                audience: _appSettings.JwtAudience,
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: credentials
            );
            
            _logger.LogDebug("Token JWT generado para usuario: {UserId}", user.Id);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}