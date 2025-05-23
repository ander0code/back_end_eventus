using back_end.Modules.pagos.DTOs;
using back_end.Modules.pagos.Models;
using back_end.Modules.pagos.Repositories;
using Microsoft.Extensions.Logging;

namespace back_end.Modules.pagos.services
{
    public interface IPagosService
    {
        Task<List<PagoResponseDTO>> GetAllAsync();
        Task<PagoResponseDTO?> GetByIdAsync(string id);
        Task<List<PagoResponseDTO>> GetByReservaIdAsync(string reservaId);
        Task<PagoResponseDTO?> CreateAsync(PagoCreateDTO dto);
        Task<PagoResponseDTO?> UpdateAsync(string id, PagoUpdateDTO dto);
        Task<bool> DeleteAsync(string id);
        
        // Métodos para tipos de pago
        Task<List<TipoPagoDTO>> GetAllTiposPagoAsync();
        Task<TipoPagoDTO?> GetTipoPagoByIdAsync(string id);
    }

    public class PagosService : IPagosService
    {
        private readonly IPagosRepository _repository;
        private readonly ILogger<PagosService> _logger;

        public PagosService(IPagosRepository repository, ILogger<PagosService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<PagoResponseDTO>> GetAllAsync()
        {
            try
            {
                var pagos = await _repository.GetAllAsync();
                return pagos.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los pagos");
                return new List<PagoResponseDTO>();
            }
        }

        public async Task<PagoResponseDTO?> GetByIdAsync(string id)
        {
            try
            {
                var pago = await _repository.GetByIdAsync(id);
                return pago != null ? MapToDTO(pago) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pago con ID {Id}", id);
                return null;
            }
        }

        public async Task<List<PagoResponseDTO>> GetByReservaIdAsync(string reservaId)
        {
            try
            {
                var pagos = await _repository.GetByReservaIdAsync(reservaId);
                return pagos.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pagos para la reserva con ID {ReservaId}", reservaId);
                return new List<PagoResponseDTO>();
            }
        }

        public async Task<PagoResponseDTO?> CreateAsync(PagoCreateDTO dto)
        {
            try
            {
                var pago = new Pago
                {
                    Id = Guid.NewGuid().ToString(), // Generar un ID único
                    IdReserva = dto.IdReserva,
                    IdTipoPago = dto.IdTipoPago,
                    Monto = dto.Monto
                };

                var creado = await _repository.CreateAsync(pago);
                return creado != null ? MapToDTO(creado) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear pago para reserva {IdReserva}", dto.IdReserva);
                return null;
            }
        }

        public async Task<PagoResponseDTO?> UpdateAsync(string id, PagoUpdateDTO dto)
        {
            try
            {
                var pago = await _repository.GetByIdAsync(id);
                if (pago == null) return null;

                if (dto.IdTipoPago != null) pago.IdTipoPago = dto.IdTipoPago;
                if (dto.Monto != null) pago.Monto = dto.Monto;

                var actualizado = await _repository.UpdateAsync(pago);
                return actualizado != null ? MapToDTO(actualizado) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar pago con ID {Id}", id);
                return null;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var pago = await _repository.GetByIdAsync(id);
                if (pago == null) return false;

                return await _repository.DeleteAsync(pago);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar pago con ID {Id}", id);
                return false;
            }
        }

        public async Task<List<TipoPagoDTO>> GetAllTiposPagoAsync()
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

        public async Task<TipoPagoDTO?> GetTipoPagoByIdAsync(string id)
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

        private PagoResponseDTO MapToDTO(Pago pago)
        {
            return new PagoResponseDTO
            {
                Id = pago.Id,
                IdReserva = pago.IdReserva,
                IdTipoPago = pago.IdTipoPago,
                Monto = pago.Monto,
                TipoPagoNombre = pago.IdTipoPagoNavigation?.Nombre,
                NombreReserva = pago.IdReservaNavigation?.NombreEvento
            };
        }
    }
}

