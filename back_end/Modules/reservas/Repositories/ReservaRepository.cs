using back_end.Core.Data;
using back_end.Modules.reservas.Models;
using Microsoft.EntityFrameworkCore;
using back_end.Core.Utils;
using Microsoft.Extensions.Logging;

namespace back_end.Modules.reservas.Repositories
{    
    public interface IReservaRepository
    {
        Task<List<Reserva>> GetAllAsync();
        Task<Reserva?> GetByIdAsync(string id);
        Task<Reserva> CreateAsync(Reserva reserva);
        Task<Reserva> UpdateAsync(Reserva reserva);
        Task<bool> DeleteAsync(Reserva reserva);
        Task<List<Reserva>> GetByClienteIdAsync(string clienteId);
    }

    public class ReservaRepository : IReservaRepository
    {
        private readonly DbEventusContext _context;
        private readonly ILogger<ReservaRepository> _logger;

        public ReservaRepository(DbEventusContext context, ILogger<ReservaRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Reserva>> GetAllAsync()
        {
            try
            {
                return await _context.Reservas
                    .Include(r => r.Cliente)
                        .ThenInclude(c => c!.Usuario)
                    .Include(r => r.Servicio) // No incluir subpropiedades que no existan
                    .Include(r => r.TiposEventoNavigation)
                    .OrderByDescending(r => r.FechaRegistro)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las reservas");
                throw;
            }
        }

        public async Task<List<Reserva>> GetByClienteIdAsync(string clienteId)
        {
            return await _context.Reservas
                .Include(r => r.Cliente)
                    .ThenInclude(c => c!.Usuario) 
                .Include(r => r.Servicio)
                .Include(r => r.TiposEventoNavigation)
                .Include(r => r.Pagos)
                .Where(r => r.ClienteId == clienteId)
                .ToListAsync();
        }

        public async Task<Reserva?> GetByIdAsync(string id)
        {
            return await _context.Reservas
                .Include(r => r.Cliente)
                    .ThenInclude(c => c!.Usuario) 
                .Include(r => r.Servicio)
                .Include(r => r.TiposEventoNavigation)
                .Include(r => r.Pagos)
                .AsSplitQuery() 
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Reserva> CreateAsync(Reserva reserva)
        {
            // Asignar un ID personalizado usando nuestro generador
            if (string.IsNullOrEmpty(reserva.Id))
            {
                reserva.Id = IdGenerator.GenerateId("Reserva");
            }
            
            // Establecer la fecha de registro
            reserva.FechaRegistro = DateTime.Now;
            
            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();
            return reserva;
        }

        public async Task<Reserva> UpdateAsync(Reserva reserva)
        {
            _context.Entry(reserva).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return reserva;
        }        public async Task<bool> DeleteAsync(Reserva reserva)
        {
            _context.Reservas.Remove(reserva);
            return (await _context.SaveChangesAsync()) > 0;
        }
    }
}
