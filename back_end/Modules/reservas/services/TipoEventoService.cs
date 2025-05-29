using back_end.Core.Data;
using back_end.Modules.reservas.Models;
using Microsoft.EntityFrameworkCore;

namespace back_end.Modules.reservas.Services
{
    public interface ITipoEventoService
    {
        Task<Guid> GetOrCreateTipoEventoAsync(string nombre);
    }

    public class TipoEventoService : ITipoEventoService
    {
        private readonly DbEventusContext _context;
        private readonly ILogger<TipoEventoService> _logger;

        public TipoEventoService(DbEventusContext context, ILogger<TipoEventoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Guid> GetOrCreateTipoEventoAsync(string nombre)
        {
            try
            {
                // Buscar si ya existe un tipo de evento con ese nombre (case insensitive)
                var tipoExistente = await _context.TiposEventos
                    .FirstOrDefaultAsync(t => t.Nombre!.ToLower() == nombre.ToLower());

                if (tipoExistente != null)
                {
                    _logger.LogInformation("Tipo de evento existente encontrado: {Nombre} con ID: {Id}", nombre, tipoExistente.Id);
                    return tipoExistente.Id;
                }

                // Si no existe, crear uno nuevo
                var nuevoTipo = new TiposEvento
                {
                    Id = Guid.NewGuid(),
                    Nombre = nombre,
                    Descripcion = $"Tipo de evento: {nombre}"
                };

                _context.TiposEventos.Add(nuevoTipo);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Nuevo tipo de evento creado: {Nombre} con ID: {Id}", nombre, nuevoTipo.Id);
                return nuevoTipo.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener o crear tipo de evento: {Nombre}", nombre);
                throw;
            }
        }
    }
}
