using back_end.Core.Data;
using back_end.Modules.Auth.DTOs;
using System.Linq;

namespace back_end.Modules.Auth.Repositories
{
    public interface IUserRepository
    {
        UsuarioAuthDTO? GetUserByUsername(string username);
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
                Correo = usuario.Correo ?? string.Empty,
                ContrasenaHash = usuario.ContrasenaHash ?? string.Empty,
                Nombre = usuario.Nombre ?? string.Empty,
                Apellido = usuario.Apellido ?? string.Empty,
                Verificado = usuario.Verificado
            };
        }
    }
}