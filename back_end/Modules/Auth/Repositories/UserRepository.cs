using back_end.Core.Data;
using back_end.Modules.Auth.DTOs;
using back_end.Modules.usuarios.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace back_end.Modules.Auth.Repositories
{
    public interface IUserRepository
    {
        UsuarioAuthDTO? GetUserByUsername(string username);
        Task<UsuarioAuthDTO> RegisterUser(RegisterRequestDTO request);
        Task<bool> ExistsByEmail(string email);
    }

    public class UserRepository : IUserRepository
    {
        private readonly DbEventusContext _context;

        public UserRepository(DbEventusContext context)
        {
            _context = context;
        }

        public UsuarioAuthDTO? GetUserByUsername(string username)
        {
            // Adaptamos la lógica para usar la entidad Usuario de nuestro modelo
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Correo == username);
            
            if (usuario == null)
                return null;
                
            // Mapeamos de Usuario a UsuarioAuthDTO
            return new UsuarioAuthDTO
            {
                Id = usuario.Id,
                Correo = usuario.Correo,
                ContrasenaHash = usuario.ContrasenaHash,
                Nombre = usuario.Nombre ?? string.Empty,
                Apellido = usuario.Apellido ?? string.Empty,
                Verificado = usuario.Verificado
            };
        }

        public async Task<UsuarioAuthDTO> RegisterUser(RegisterRequestDTO request)
        {
            // Crear un nuevo usuario
            var usuario = new Usuario
            {
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Correo = request.Email,
                Telefono = request.Telefono,
                ContrasenaHash = HashPassword(request.Password),
                Verificado = false,
                TokenVerificacion = GenerateVerificationToken(),
                FechaRegistro = DateTime.Now
            };

            // Guardar en la base de datos
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Retornar el DTO del usuario
            return new UsuarioAuthDTO
            {
                Id = usuario.Id,
                Correo = usuario.Correo,
                ContrasenaHash = usuario.ContrasenaHash,
                Nombre = usuario.Nombre ?? string.Empty,
                Apellido = usuario.Apellido ?? string.Empty,
                Verificado = usuario.Verificado
            };
        }

        public async Task<bool> ExistsByEmail(string email)
        {
            return await _context.Usuarios.AnyAsync(u => u.Correo == email);
        }

        private string HashPassword(string password)
        {
            // En una app real, usarías BCrypt o similar
            using (var hmac = new HMACSHA512())
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(computedHash);
            }
        }

        private string GenerateVerificationToken()
        {
            // Generar un token aleatorio para verificación de email
            return Guid.NewGuid().ToString();
        }
    }
}