using back_end.Core.Data;
using back_end.Modules.pagos.Models;
using Microsoft.EntityFrameworkCore;
using back_end.Core.Utils;

namespace back_end.Modules.pagos.Repositories
{
    public interface IPagosRepository
    {
        Task<List<Pago>> GetAllAsync();
        Task<Pago?> GetByIdAsync(string id);
        Task<List<Pago>> GetByReservaIdAsync(string reservaId);
        Task<Pago> CreateAsync(Pago pago);
        Task<Pago> UpdateAsync(Pago pago);
        Task<bool> DeleteAsync(Pago pago);
        Task<List<TipoPago>> GetAllTiposPagoAsync();
        Task<TipoPago?> GetTipoPagoByIdAsync(string id);
        Task<TipoPago?> GetTipoPagoByNombreAsync(string nombre);
        Task<TipoPago> CreateTipoPagoAsync(TipoPago tipoPago);
    }

    public class PagosRepository : IPagosRepository
    {
        private readonly DbEventusContext _context;

        public PagosRepository(DbEventusContext context)
        {
            _context = context;
        }

        public async Task<List<Pago>> GetAllAsync()
        {
            return await _context.Pagos
                .Include(p => p.IdReservaNavigation)
                .Include(p => p.IdTipoPagoNavigation)
                .ToListAsync();
        }

        public async Task<Pago?> GetByIdAsync(string id)
        {
            return await _context.Pagos
                .Include(p => p.IdReservaNavigation)
                .Include(p => p.IdTipoPagoNavigation)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Pago>> GetByReservaIdAsync(string reservaId)
        {
            return await _context.Pagos
                .Include(p => p.IdReservaNavigation)
                .Include(p => p.IdTipoPagoNavigation)
                .Where(p => p.IdReserva == reservaId)
                .ToListAsync();
        }        public async Task<Pago> CreateAsync(Pago pago)
        {
            // Generar ID personalizado si no se ha proporcionado uno
            if (string.IsNullOrEmpty(pago.Id))
            {
                pago.Id = IdGenerator.GenerateId("Pago");
            }
            
            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();
            return pago;
        }

        public async Task<Pago> UpdateAsync(Pago pago)
        {
            _context.Entry(pago).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return pago;
        }

        public async Task<bool> DeleteAsync(Pago pago)
        {
            _context.Pagos.Remove(pago);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<TipoPago>> GetAllTiposPagoAsync()
        {
            return await _context.TipoPagos.ToListAsync();
        }

        public async Task<TipoPago?> GetTipoPagoByIdAsync(string id)
        {
            return await _context.TipoPagos.FirstOrDefaultAsync(tp => tp.Id == id);
        }

        public async Task<TipoPago?> GetTipoPagoByNombreAsync(string nombre)
        {
            return await _context.TipoPagos
                .FirstOrDefaultAsync(tp => tp.Nombre != null && tp.Nombre.ToLower() == nombre.ToLower());
        }

        public async Task<TipoPago> CreateTipoPagoAsync(TipoPago tipoPago)
        {
            // Generar ID personalizado si no se ha proporcionado uno
            if (string.IsNullOrEmpty(tipoPago.Id))
            {
                tipoPago.Id = IdGenerator.GenerateId("TipoPago");
            }
            
            _context.TipoPagos.Add(tipoPago);
            await _context.SaveChangesAsync();
            return tipoPago;
        }
    }
}