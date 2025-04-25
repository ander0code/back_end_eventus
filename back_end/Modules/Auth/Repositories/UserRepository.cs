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

            var usuario = _context.Usuarios.FirstOrDefault(u => u.CorreoElectronico == email);
            
            if (usuario == null)
                return null;

            return new UsuarioAuthDTO
            {
                Id = usuario.Id,
                Correo = usuario.CorreoElectronico,
                ContrasenaHash = usuario.Contrasena,
                Nombre = usuario.Nombre ?? string.Empty,
                Apellido = usuario.Apellido ?? string.Empty,
                Verificado = usuario.Verificado
            };
        }

        public async Task<UsuarioAuthDTO> RegisterUser(RegisterRequestDTO request)
        {

            var usuario = new Usuario
            {
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                CorreoElectronico = request.Email,
                Celular = request.Telefono,
                Contrasena = HashPassword(request.Password),
                Verificado = false,
                TokenVerificacion = GenerateVerificationToken(),
                FechaRegistro = DateTime.Now
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return new UsuarioAuthDTO
            {
                Id = usuario.Id,
                Correo = usuario.CorreoElectronico,
                ContrasenaHash = usuario.Contrasena,
                Nombre = usuario.Nombre ?? string.Empty,
                Apellido = usuario.Apellido ?? string.Empty,
                Verificado = usuario.Verificado
            };
        }

        public async Task<bool> ExistsByEmail(string email)
        {
            return await _context.Usuarios.AnyAsync(u => u.CorreoElectronico == email);
        }

        private string HashPassword(string password)
        {

            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        private string GenerateVerificationToken()
        {

            return Guid.NewGuid().ToString();
        }
    }
}