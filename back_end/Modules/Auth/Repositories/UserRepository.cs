using back_end.Core.Data;
using back_end.Modules.Auth.DTOs;
using back_end.Modules.organizador.Models;
using Microsoft.EntityFrameworkCore;
using back_end.Core.Utils;

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
            // Primero buscamos el usuario por correo
            var usuario = _context.Usuarios
                .Include(u => u.Organizadors)
                .FirstOrDefault(u => u.Correo == email);
            
            if (usuario == null)
                return null;

            // Ahora buscamos el organizador asociado para obtener la contraseña
            var organizador = usuario.Organizadors.FirstOrDefault();
              if (organizador == null)
                return null;

            return new UsuarioAuthDTO
            {
                Id = usuario.Id,
                Correo = usuario.Correo ?? string.Empty,
                ContrasenaHash = organizador.Contrasena ?? string.Empty,
                Nombre = usuario.Nombre ?? string.Empty,
                Apellido = usuario.Apellido ?? string.Empty,
                NombreNegocio = organizador.NombreNegocio ?? string.Empty
            };
        }        public async Task<UsuarioAuthDTO> RegisterUser(RegisterRequestDTO request)
        {
            // Generar ID personalizado para Usuario
            string userId = IdGenerator.GenerateId("Usuario");
            
            // Primero creamos el usuario
            var usuario = new Usuario
            {
                Id = userId,
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Correo = request.Email,
                Celular = request.Telefono
            };

            _context.Usuarios.Add(usuario);
            
            // Luego creamos el organizador asociado con ID personalizado
            var organizador = new Organizador
            {
                Id = IdGenerator.GenerateId("Organizador"),
                NombreNegocio = $"{request.Nombre}'s Business",
                Contrasena = HashPassword(request.Password),
                UsuarioId = userId
            };
            
            _context.Organizadors.Add(organizador);
            
            await _context.SaveChangesAsync();            return new UsuarioAuthDTO
            {
                Id = usuario.Id,
                Correo = usuario.Correo ?? string.Empty,
                ContrasenaHash = organizador.Contrasena ?? string.Empty,
                Nombre = usuario.Nombre ?? string.Empty,
                Apellido = usuario.Apellido ?? string.Empty,
                NombreNegocio = organizador.NombreNegocio ?? string.Empty
            };
        }

        public async Task<bool> ExistsByEmail(string email)
        {
            return await _context.Usuarios.AnyAsync(u => u.Correo == email);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }
    }
}