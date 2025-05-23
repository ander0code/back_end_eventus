using back_end.Core.Data;
using back_end.Modules.organizador.Models;
using Microsoft.EntityFrameworkCore;
using back_end.Core.Utils;

namespace back_end.Modules.organizador.Repositories
{    public interface IUsuarioRepository
    {
        Task<List<Usuario>> GetAllAsync();
        Task<Usuario?> GetByIdAsync(string id);
        Task<Usuario?> GetByCorreoAsync(string correo);
        Task<Usuario> UpdateAsync(Usuario usuario);
        Task<Usuario> CreateAsync(Usuario usuario);
    }

    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly DbEventusContext _context;

        public UsuarioRepository(DbEventusContext context)
        {
            _context = context;
        }

        public async Task<List<Usuario>> GetAllAsync()
        {
            return await _context.Usuarios.ToListAsync();
        }

        public async Task<Usuario?> GetByIdAsync(string id)
        {
            return await _context.Usuarios.FindAsync(id);
        }

        public async Task<Usuario?> GetByCorreoAsync(string correo)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
        }        public async Task<Usuario> UpdateAsync(Usuario usuario)
        {
            _context.Entry(usuario).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return usuario;
        }
        
        public async Task<Usuario> CreateAsync(Usuario usuario)
        {
            // Generar ID personalizado si no se ha proporcionado uno
            if (string.IsNullOrEmpty(usuario.Id))
            {
                usuario.Id = IdGenerator.GenerateId("Usuario");
            }
            
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }
    }
}
