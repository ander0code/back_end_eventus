using back_end.Modules.Auth.Repositories;
using back_end.Modules.Auth.DTOs;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using Microsoft.Extensions.Logging;

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
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository userRepository, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public AuthResponseDTO Authenticate(AuthRequestDTO request)
        {
            // Buscar el usuario por correo electrónico
            var user = _userRepository.GetUserByEmail(request.Email);
            
            // Si el usuario no existe, lanzar excepción específica para el correo
            if (user == null)
            {
                _logger.LogWarning("Intento de login fallido: Usuario no encontrado: {Email}", request.Email);
                throw new UnauthorizedAccessException("El correo electrónico no está registrado en el sistema");
            }
            
            // Si el usuario existe pero la contraseña no coincide
            if (!VerifyPasswordHash(request.Password, user.ContrasenaHash))
            {
                _logger.LogWarning("Intento de login fallido: Contraseña incorrecta para: {Email}", request.Email);
                throw new UnauthorizedAccessException("La contraseña ingresada es incorrecta");
            }

            // Si llegamos aquí, la autenticación fue exitosa
            var token = GenerateJwtToken(user);
            
            // Registramos el éxito
            _logger.LogInformation("Login exitoso para: {Email}", request.Email);

            return new AuthResponseDTO
            {
                Email = user.Correo,
                Token = token,
                UserId = user.Id,
                Nombre = user.Nombre,
                Apellido = user.Apellido
            };
        }

        public async Task<RegisterResponseDTO> Register(RegisterRequestDTO request)
        {
            // Verificar si el usuario ya existe
            if (await _userRepository.ExistsByEmail(request.Email))
            {
                _logger.LogWarning("Intento de registro fallido: Email ya existe: {Email}", request.Email);
                throw new InvalidOperationException("El correo electrónico ya está registrado");
            }

            // Registrar al usuario
            var newUser = await _userRepository.RegisterUser(request);
            _logger.LogInformation("Usuario registrado correctamente: {Email}", request.Email);

            // Retornar la respuesta
            return new RegisterResponseDTO
            {
                UserId = newUser.Id,
                Email = newUser.Correo
            };
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            try
            {
                _logger.LogDebug("Verificando contraseña para hash: {HashPrefix}...", 
                    storedHash.Length > 10 ? storedHash.Substring(0, 10) + "..." : storedHash);
                
                // Primero intentamos verificar con BCrypt
                if (storedHash.StartsWith("$2a$") || storedHash.StartsWith("$2b$") || storedHash.StartsWith("$2y$"))
                {
                    // Es un hash BCrypt
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
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "tu_clave_secreta_predeterminada_debe_ser_al_menos_16_caracteres"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Correo),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("name", $"{user.Nombre} {user.Apellido}")
            };
            
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: credentials
            );
            
            _logger.LogDebug("Token JWT generado para usuario: {UserId}", user.Id);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}