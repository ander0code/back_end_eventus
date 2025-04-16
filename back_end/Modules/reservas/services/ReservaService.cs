using back_end.Core.Data;
using back_end.Modules.reservas.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.Modules.reservas.services
{
    public interface IReservaService
    {
        Task<List<Reserva>> GetAllAsync();
        Task<Reserva?> GetByIdAsync(int id);
        Task<List<Reserva>> GetByUsuarioIdAsync(int usuarioId);
        Task<Reserva> CreateAsync(Reserva reserva);
        Task<Reserva> UpdateAsync(Reserva reserva);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateEstadoAsync(int id, string estado);
    }

    public class ReservaService : IReservaService
    {
        private readonly DbEventusContext _context;

        public ReservaService(DbEventusContext context)
        {
            _context = context;
        }

        public async Task<List<Reserva>> GetAllAsync()
        {
            return await _context.Reservas
                .Include(r => r.Servicio)
                .Include(r => r.Usuario)
                .ToListAsync();
        }

        public async Task<Reserva? > GetByIdAsync(int id)
        {
            return await _context.Reservas
                .Include(r => r.Servicio)
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Reserva>> GetByUsuarioIdAsync(int usuarioId)
        {
            return await _context.Reservas
                .Include(r => r.Servicio)
                .Include(r => r.Usuario)
                .Where(r => r.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<Reserva> CreateAsync(Reserva reserva)
        {
            reserva.FechaReserva = DateTime.Now;
            reserva.Estado = "pendiente";
            
            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();
            return reserva;
        }

        public async Task<Reserva> UpdateAsync(Reserva reserva)
        {
            _context.Entry(reserva).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return reserva;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
                return false;

            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateEstadoAsync(int id, string estado)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
                return false;

            reserva.Estado = estado;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}