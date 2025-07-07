using back_end.Modules.pagos.DTOs;
using back_end.Modules.pagos.Models;
using back_end.Modules.pagos.Repositories;
using back_end.Modules.pagos.Services;
using back_end.Core.Utils;
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
        Task<decimal> GetTotalPagadoByReservaIdAsync(string reservaId);
        Task<DateTime?> GetUltimoPagoFechaByReservaIdAsync(string reservaId);
    }

    public class PagosService : IPagosService
    {
        private readonly IPagosRepository _repository;
        private readonly ITipoPagoService _tipoPagoService;
        private readonly ILogger<PagosService> _logger;

        public PagosService(IPagosRepository repository, ITipoPagoService tipoPagoService, ILogger<PagosService> logger)
        {
            _repository = repository;
            _tipoPagoService = tipoPagoService;
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
                // Validar que el nombre del tipo de pago no sea nulo o vacío
                if (string.IsNullOrWhiteSpace(dto.NombreTipoPago))
                {
                    _logger.LogError("El nombre del tipo de pago es requerido");
                    return null;
                }

                // Obtener o crear el tipo de pago
                var tipoPago = await _tipoPagoService.GetOrCreateTipoPagoAsync(dto.NombreTipoPago);
                if (tipoPago == null)
                {
                    _logger.LogError("No se pudo crear o encontrar el tipo de pago: {NombreTipoPago}", dto.NombreTipoPago);
                    return null;
                }

                var pago = new Pago
                {
                    Id = IdGenerator.GenerateId("Pago"), // Usar IdGenerator en lugar de Guid
                    IdReserva = dto.IdReserva,
                    IdTipoPago = tipoPago.Id,
                    Monto = dto.Monto
                    // No establecer FechaPago - se hace automáticamente en el repositorio
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

                // Si se proporciona un nombre de tipo de pago, obtener o crear el tipo
                if (!string.IsNullOrWhiteSpace(dto.NombreTipoPago))
                {
                    var tipoPago = await _tipoPagoService.GetOrCreateTipoPagoAsync(dto.NombreTipoPago);
                    if (tipoPago == null)
                    {
                        _logger.LogError("No se pudo crear o encontrar el tipo de pago: {NombreTipoPago}", dto.NombreTipoPago);
                        return null;
                    }
                    pago.IdTipoPago = tipoPago.Id;
                }

                if (dto.Monto != null) pago.Monto = dto.Monto;
                // Remover actualización de FechaPago - es inmutable después de la creación

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

        public async Task<decimal> GetTotalPagadoByReservaIdAsync(string reservaId)
        {
            try
            {
                var pagos = await _repository.GetByReservaIdAsync(reservaId);
                
                decimal totalPagado = 0;
                if (pagos != null && pagos.Any())
                {
                    totalPagado = pagos.Sum(p => 
                    {
                        if (decimal.TryParse(p.Monto, out decimal monto))
                            return monto;
                        return 0;
                    });
                }

                return totalPagado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular total pagado para reserva {ReservaId}", reservaId);
                return 0;
            }
        }

        public async Task<DateTime?> GetUltimoPagoFechaByReservaIdAsync(string reservaId)
        {
            try
            {
                var pagos = await _repository.GetByReservaIdAsync(reservaId);
                return pagos?.OrderByDescending(p => p.FechaPago).FirstOrDefault()?.FechaPago;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener fecha del último pago para reserva {ReservaId}", reservaId);
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
                NombreReserva = pago.IdReservaNavigation?.NombreEvento,
                FechaPago = pago.FechaPago
            };
        }
    }
}

