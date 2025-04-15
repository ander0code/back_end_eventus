using back_end.Core.Entities;
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
            if (user == null || !VerifyPasswordHash(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            return new AuthResponseDTO
            {
                Username = user.Username,
                Token = "dummy-jwt-token" // Aquí se generaría un JWT real
            };
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            using (var hmac = new HMACSHA512())
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(Convert.FromBase64String(storedHash));
            }
        }
    }
}