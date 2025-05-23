using back_end.Core.Data;
using back_end.Modules.organizador.Models;
using Microsoft.EntityFrameworkCore;

namespace back_end.Modules.usuarios.Repositories
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> GetByCorreoAsync(string correo);
    }

    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly DbEventusContext _context;

        public UsuarioRepository(DbEventusContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> GetByCorreoAsync(string correo)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == correo);
        }
    }
}