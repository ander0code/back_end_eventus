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

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public AuthResponseDTO Authenticate(AuthRequestDTO request)
        {
            var user = _userRepository.GetUserByUsername(request.Username);
            if (user == null || !VerifyPasswordHash(request.Password, user.ContrasenaHash))
            {
                throw new UnauthorizedAccessException("Usuario o contraseña inválidos");
            }

            var token = GenerateJwtToken(user);

            return new AuthResponseDTO
            {
                Username = user.Correo,
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
                throw new InvalidOperationException("El correo electrónico ya está registrado");
            }

            // Registrar al usuario
            var newUser = await _userRepository.RegisterUser(request);

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
                // Primero intentamos verificar con BCrypt
                if (storedHash.StartsWith("$2a$") || storedHash.StartsWith("$2b$") || storedHash.StartsWith("$2y$"))
                {
                    // Es un hash BCrypt
                    return BCrypt.Net.BCrypt.Verify(password, storedHash);
                }
                
                // Método de transición - si el hash está en Base64 (hash antiguo)
                try
                {
                    byte[] hashBytes = Convert.FromBase64String(storedHash);
                    string decodedHash = Encoding.UTF8.GetString(hashBytes);
                    return password == decodedHash;
                }
                catch
                {
                    // Si no es un hash Base64 ni BCrypt, es probablemente texto plano (solo para desarrollo)
                    return password == storedHash;
                }
            }
            catch (Exception)
            {
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
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}