using back_end.Core.Data;
using back_end.Modules.organizador.Models;
using Microsoft.EntityFrameworkCore;
using back_end.Core.Utils;

namespace back_end.Modules.organizador.Repositories
{
    public interface IOrganizadorRepository
    {
        Task<List<Organizador>> GetAllAsync();
        Task<Organizador?> GetByIdAsync(string id);
        Task<Organizador?> GetByUsuarioIdAsync(string usuarioId);
        Task<Organizador> CreateAsync(Organizador organizador);
        Task<Organizador> UpdateAsync(Organizador organizador);
        Task<bool> DeleteAsync(Organizador organizador);
    }

    public class OrganizadorRepository : IOrganizadorRepository
    {
        private readonly DbEventusContext _context;

        public OrganizadorRepository(DbEventusContext context)
        {
            _context = context;
        }

        public async Task<List<Organizador>> GetAllAsync()
        {
            return await _context.Organizadors
                .Include(o => o.Usuario)
                .ToListAsync();
        }

        public async Task<Organizador?> GetByIdAsync(string id)
        {
            return await _context.Organizadors
                .Include(o => o.Usuario)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Organizador?> GetByUsuarioIdAsync(string usuarioId)
        {
            return await _context.Organizadors
                .Include(o => o.Usuario)
                .FirstOrDefaultAsync(o => o.UsuarioId == usuarioId);
        }        public async Task<Organizador> CreateAsync(Organizador organizador)
        {
            // Generar ID personalizado si no se ha proporcionado uno
            if (string.IsNullOrEmpty(organizador.Id))
            {
                organizador.Id = IdGenerator.GenerateId("Organizador");
            }
            
            _context.Organizadors.Add(organizador);
            await _context.SaveChangesAsync();
            return organizador;
        }

        public async Task<Organizador> UpdateAsync(Organizador organizador)
        {
            _context.Entry(organizador).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return organizador;
        }

        public async Task<bool> DeleteAsync(Organizador organizador)
        {
            _context.Organizadors.Remove(organizador);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}