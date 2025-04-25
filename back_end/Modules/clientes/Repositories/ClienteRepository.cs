using back_end.Core.Data;
using back_end.Modules.clientes.Models;
using Microsoft.EntityFrameworkCore;

namespace back_end.Modules.clientes.Repositories
{
    public interface IClienteRepository
    {
        Task<List<Cliente>> GetByCorreoUsuarioAsync(string correoUsuario);
        Task<Cliente?> GetByIdAsync(Guid id);
        Task<Cliente> CreateAsync(Cliente cliente);
        Task<Cliente> UpdateAsync(Cliente cliente);
        Task<bool> DeleteAsync(Cliente cliente);
    }

    public class ClienteRepository : IClienteRepository
    {
        private readonly DbEventusContext _context;

        public ClienteRepository(DbEventusContext context)
        {
            _context = context;
        }

        public async Task<List<Cliente>> GetByCorreoUsuarioAsync(string correoUsuario)
        {
            return await _context.Clientes
                .Include(c => c.Usuario)
                .Include(c => c.Reservas) // <-- importante
                .Where(c => c.Usuario.CorreoElectronico == correoUsuario)
                .ToListAsync();
        }

        public async Task<Cliente?> GetByIdAsync(Guid id)
        {
            return await _context.Clientes.FindAsync(id);
        }

        public async Task<Cliente> CreateAsync(Cliente cliente)
        {
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return cliente;
        }

        public async Task<Cliente> UpdateAsync(Cliente cliente)
        {
            _context.Entry(cliente).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return cliente;
        }

        public async Task<bool> DeleteAsync(Cliente cliente)
        {
            _context.Clientes.Remove(cliente);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
