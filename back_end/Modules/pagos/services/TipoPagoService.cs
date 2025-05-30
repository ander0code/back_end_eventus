using back_end.Modules.pagos.DTOs;
using back_end.Modules.pagos.Models;
using back_end.Modules.pagos.Repositories;
using Microsoft.Extensions.Logging;

namespace back_end.Modules.pagos.Services
{
    public interface ITipoPagoService
    {
        Task<List<TipoPagoDTO>> GetAllAsync();
        Task<TipoPagoDTO?> GetByIdAsync(string id);
        Task<TipoPago?> GetOrCreateTipoPagoAsync(string nombre);
    }

    public class TipoPagoService : ITipoPagoService
    {
        private readonly IPagosRepository _repository;
        private readonly ILogger<TipoPagoService> _logger;

        public TipoPagoService(IPagosRepository repository, ILogger<TipoPagoService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<TipoPagoDTO>> GetAllAsync()
        {
            try
            {
                var tiposPago = await _repository.GetAllTiposPagoAsync();
                return tiposPago.Select(t => new TipoPagoDTO
                {
                    Id = t.Id,
                    Nombre = t.Nombre
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los tipos de pago");
                return new List<TipoPagoDTO>();
            }
        }

        public async Task<TipoPagoDTO?> GetByIdAsync(string id)
        {
            try
            {
                var tipoPago = await _repository.GetTipoPagoByIdAsync(id);
                if (tipoPago == null) return null;

                return new TipoPagoDTO
                {
                    Id = tipoPago.Id,
                    Nombre = tipoPago.Nombre
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipo de pago con ID {Id}", id);
                return null;
            }
        }

        public async Task<TipoPago?> GetOrCreateTipoPagoAsync(string nombre)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    _logger.LogWarning("Nombre de tipo de pago no puede estar vacío");
                    return null;
                }

                // Buscar tipo de pago existente
                var tipoPagoExistente = await _repository.GetTipoPagoByNombreAsync(nombre.Trim());
                if (tipoPagoExistente != null)
                {
                    _logger.LogInformation("Tipo de pago encontrado: {Nombre}", nombre);
                    return tipoPagoExistente;
                }

                // Crear nuevo tipo de pago
                var nuevoTipoPago = new TipoPago
                {
                    Nombre = nombre.Trim()
                };

                var creado = await _repository.CreateTipoPagoAsync(nuevoTipoPago);
                _logger.LogInformation("Nuevo tipo de pago creado: {Nombre} con ID: {Id}", creado.Nombre, creado.Id);
                return creado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener o crear tipo de pago: {Nombre}", nombre);
                return null;
            }
        }
    }
}
