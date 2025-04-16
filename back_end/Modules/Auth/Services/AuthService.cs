using back_end.Modules.Auth.Repositories;
using back_end.Modules.Auth.DTOs;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace back_end.Modules.Auth.Services
{
    public interface IAuthService
    {
        AuthResponseDTO Authenticate(AuthRequestDTO request);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public AuthResponseDTO Authenticate(AuthRequestDTO request)
        {
            var user = _userRepository.GetUserByUsername(request.Username);
            if (user == null || !VerifyPasswordHash(request.Password, user.ContrasenaHash))
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            return new AuthResponseDTO
            {
                Username = user.Correo,
                Token = GenerateJwtToken(user) // Implementar la generación real de JWT
            };
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            // Nota: Este es un ejemplo simplificado. En una app real, necesitarías implementar
            // una verificación de contraseña más segura, posiblemente usando BCrypt o similar
            using (var hmac = new HMACSHA512())
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(Convert.FromBase64String(storedHash));
            }
        }

        private string GenerateJwtToken(UsuarioAuthDTO user)
        {
            // Aquí implementarías la generación real del token JWT
            // Por ahora devolvemos un token de ejemplo
            return $"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.{user.Id}.{DateTime.UtcNow.AddDays(7).Ticks}";
        }
    }
}