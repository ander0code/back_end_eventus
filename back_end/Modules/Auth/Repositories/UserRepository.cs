using back_end.Core.Data;
using back_end.Modules.Auth.DTOs;
using back_end.Modules.usuarios.Models;
using Microsoft.EntityFrameworkCore;

namespace back_end.Modules.Auth.Repositories
{
    public interface IUserRepository
    {
        UsuarioAuthDTO? GetUserByEmail(string email);
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

        public UsuarioAuthDTO? GetUserByEmail(string email)
        {
            // Adaptamos la lógica para usar la entidad Usuario de nuestro modelo
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Correo == email);
            
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
            // Usar BCrypt para generar un hash seguro
            // WorkFactor 12 es un buen equilibrio entre seguridad y rendimiento
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        private string GenerateVerificationToken()
        {
            // Generar un token aleatorio para verificación de email
            return Guid.NewGuid().ToString();
        }
    }
}