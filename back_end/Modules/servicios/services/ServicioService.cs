using back_end.Core.Data;
using back_end.Modules.servicios.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.Modules.servicios.services
{
    public interface IServicioService
    {
        Task<List<Servicio>> GetAllAsync();
        Task<Servicio?> GetByIdAsync(int id);
        Task<List<Servicio>> GetByUsuarioIdAsync(int usuarioId);
        Task<Servicio> CreateAsync(Servicio servicio);
        Task<Servicio> UpdateAsync(Servicio servicio);
        Task<bool> DeleteAsync(int id);
        Task<bool> AsociarInventario(int servicioId, int itemId, int cantidad);
        Task<bool> RemoverInventario(int servicioId, int itemId);
    }

    public class ServicioService : IServicioService
    {
        private readonly DbEventusContext _context;

        public ServicioService(DbEventusContext context)
        {
            _context = context;
        }

        public async Task<List<Servicio>> GetAllAsync()
        {
            return await _context.Servicios
                .Include(s => s.Usuario)
                .Include(s => s.ServicioInventarios)
                    .ThenInclude(si => si.Item)
                .ToListAsync();
        }

        public async Task<Servicio?> GetByIdAsync(int id)
        {
            return await _context.Servicios
                .Include(s => s.Usuario)
                .Include(s => s.ServicioInventarios)
                    .ThenInclude(si => si.Item)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<Servicio>> GetByUsuarioIdAsync(int usuarioId)
        {
            return await _context.Servicios
                .Include(s => s.ServicioInventarios)
                    .ThenInclude(si => si.Item)
                .Where(s => s.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<Servicio> CreateAsync(Servicio servicio)
        {
            servicio.FechaCreacion = DateTime.Now;
            
            _context.Servicios.Add(servicio);
            await _context.SaveChangesAsync();
            return servicio;
        }

        public async Task<Servicio> UpdateAsync(Servicio servicio)
        {
            _context.Entry(servicio).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return servicio;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var servicio = await _context.Servicios.FindAsync(id);
            if (servicio == null)
                return false;

            _context.Servicios.Remove(servicio);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AsociarInventario(int servicioId, int itemId, int cantidad)
        {
            var servicio = await _context.Servicios.FindAsync(servicioId);
            var item = await _context.Inventarios.FindAsync(itemId);
            
            if (servicio == null || item == null)
                return false;

            var asociacion = await _context.ServicioInventarios
                .FirstOrDefaultAsync(si => si.ServicioId == servicioId && si.ItemId == itemId);

            if (asociacion != null)
            {
                asociacion.CantidadUtilizada = cantidad;
            }
            else
            {
                asociacion = new ServicioInventario
                {
                    ServicioId = servicioId,
                    ItemId = itemId,
                    CantidadUtilizada = cantidad
                };
                _context.ServicioInventarios.Add(asociacion);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoverInventario(int servicioId, int itemId)
        {
            var asociacion = await _context.ServicioInventarios
                .FirstOrDefaultAsync(si => si.ServicioId == servicioId && si.ItemId == itemId);
                
            if (asociacion == null)
                return false;

            _context.ServicioInventarios.Remove(asociacion);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}