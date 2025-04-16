using back_end.Core.Data;
using back_end.Modules.inventario.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.Modules.inventario.services
{
    public interface IInventarioService
    {
        Task<List<Inventario>> GetAllAsync();
        Task<Inventario?> GetByIdAsync(int id);
        Task<List<Inventario>> GetByUsuarioIdAsync(int usuarioId);
        Task<Inventario> CreateAsync(Inventario inventario);
        Task<Inventario> UpdateAsync(Inventario inventario);
        Task<bool> DeleteAsync(int id);
        Task<bool> ActualizarStockAsync(int id, int cantidad);
    }

    public class InventarioService : IInventarioService
    {
        private readonly DbEventusContext _context;

        public InventarioService(DbEventusContext context)
        {
            _context = context;
        }

        public async Task<List<Inventario>> GetAllAsync()
        {
            return await _context.Inventarios
                .Include(i => i.Usuario)
                .ToListAsync();
        }

        public async Task<Inventario?> GetByIdAsync(int id)
        {
            return await _context.Inventarios
                .Include(i => i.Usuario)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<Inventario>> GetByUsuarioIdAsync(int usuarioId)
        {
            return await _context.Inventarios
                .Where(i => i.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<Inventario> CreateAsync(Inventario inventario)
        {
            inventario.FechaRegistro = DateTime.Now;
            
            _context.Inventarios.Add(inventario);
            await _context.SaveChangesAsync();
            return inventario;
        }

        public async Task<Inventario> UpdateAsync(Inventario inventario)
        {
            _context.Entry(inventario).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return inventario;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var inventario = await _context.Inventarios.FindAsync(id);
            if (inventario == null)
                return false;

            _context.Inventarios.Remove(inventario);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActualizarStockAsync(int id, int cantidad)
        {
            var inventario = await _context.Inventarios.FindAsync(id);
            if (inventario == null)
                return false;

            inventario.StockActual = cantidad;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}