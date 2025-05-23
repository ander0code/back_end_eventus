using back_end.Modules.reservas.DTOs;
using back_end.Modules.reservas.Models;
using back_end.Modules.reservas.Repositories;
using back_end.Modules.organizador.Repositories;
using back_end.Modules.servicios.Repositories;
using back_end.Modules.clientes.Models;
using back_end.Modules.clientes.Repositories;
using Microsoft.Extensions.Logging;

namespace back_end.Modules.reservas.Services
{
    public interface IReservaService
    {
        Task<List<ReservaResponseDTO>> GetByCorreoAsync(string correo);
        Task<ReservaResponseDTO?> GetByIdAsync(string correo, string id);
        Task<ReservaResponseDTO?> CreateAsync(string correo, ReservaCreateDTO dto);
        Task<ReservaResponseDTO?> UpdateAsync(string correo, string id, ReservaUpdateDTO dto);
        Task<bool> DeleteAsync(string correo, string id);
        
        // Tipos de evento
        Task<List<TipoEventoDTO>> GetAllTiposEventoAsync();
        Task<TipoEventoDTO?> GetTipoEventoByIdAsync(Guid id);
    }

    public class ReservaService : IReservaService
    {
        private readonly IReservaRepository _reservaRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IServicioRepository _servicioRepo;
        private readonly IClienteRepository _clienteRepo;
        private readonly ILogger<ReservaService> _logger;

        public ReservaService(
            IReservaRepository reservaRepo, 
            IUsuarioRepository usuarioRepo, 
            IServicioRepository servicioRepo,
            IClienteRepository clienteRepo,
            ILogger<ReservaService> logger)
        {
            _reservaRepo = reservaRepo;
            _usuarioRepo = usuarioRepo;
            _servicioRepo = servicioRepo;
            _clienteRepo = clienteRepo;
            _logger = logger;
        }

        public async Task<List<ReservaResponseDTO>> GetByCorreoAsync(string correo)
        {
            var reservas = await _reservaRepo.GetByCorreoUsuarioAsync(correo);
            return reservas.Select(MapToDTO).ToList();
        }
        
        public async Task<ReservaResponseDTO?> GetByIdAsync(string correo, string id)
        {
            var reserva = await _reservaRepo.GetByIdAndCorreoAsync(id, correo);
            return reserva == null ? null : MapToDTO(reserva);
        }

        public async Task<ReservaResponseDTO?> CreateAsync(string correo, ReservaCreateDTO dto)
        {
            var usuario = await _usuarioRepo.GetByCorreoAsync(correo);
            if (usuario == null) return null;

            // Determinar el ClienteId, ya sea utilizando uno existente o creando uno nuevo
            string? clienteId = null;
            
            if (!string.IsNullOrEmpty(dto.ClienteId))
            {
                // Usar el cliente existente
                clienteId = dto.ClienteId;
            }

            // Validar si el cliente existe
            if (!string.IsNullOrEmpty(clienteId))
            {
                var cliente = await _clienteRepo.GetByIdAsync(clienteId);
                if (cliente == null)
                {
                    _logger.LogWarning("Cliente con ID {ClienteId} no encontrado", clienteId);
                    return null;
                }
            }
            else
            {
                _logger.LogWarning("No se proporcionó ID de cliente válido");
                return null;
            }

            var reserva = new Reserva
            {
                Id = Guid.NewGuid().ToString(), // Generar un ID único
                ClienteId = clienteId,
                NombreEvento = dto.NombreEvento,
                FechaEjecucion = dto.FechaEjecucion,
                FechaRegistro = DateTime.Now,
                Descripcion = dto.Descripcion,
                Estado = dto.Estado ?? "Pendiente",
                PrecioTotal = dto.PrecioTotal ?? 0,
                TiposEvento = dto.TipoEventoId,
                ServicioId = dto.ServicioId,
                PrecioAdelanto = dto.PrecioAdelanto
            };

            var creada = await _reservaRepo.CreateAsync(reserva);
            
            // Recargar la reserva con todas sus relaciones
            var reservaCompleta = await _reservaRepo.GetByIdAsync(creada.Id);
            return reservaCompleta == null ? null : MapToDTO(reservaCompleta);
        }

        public async Task<ReservaResponseDTO?> UpdateAsync(string correo, string id, ReservaUpdateDTO dto)
        {
            var reserva = await _reservaRepo.GetByIdAndCorreoAsync(id, correo);
            if (reserva == null) return null;

            if (dto.NombreEvento != null) reserva.NombreEvento = dto.NombreEvento;
            if (dto.FechaEjecucion.HasValue) reserva.FechaEjecucion = dto.FechaEjecucion;
            if (dto.Descripcion != null) reserva.Descripcion = dto.Descripcion;
            if (dto.Estado != null) reserva.Estado = dto.Estado;
            if (dto.PrecioTotal.HasValue) reserva.PrecioTotal = dto.PrecioTotal;
            if (dto.TipoEventoId.HasValue) reserva.TiposEvento = dto.TipoEventoId;
            if (dto.ServicioId.HasValue) reserva.ServicioId = dto.ServicioId;
            if (dto.PrecioAdelanto.HasValue) reserva.PrecioAdelanto = dto.PrecioAdelanto;
            
            var actualizada = await _reservaRepo.UpdateAsync(reserva);
            
            // Recargar la reserva con todas sus relaciones
            var reservaCompleta = await _reservaRepo.GetByIdAsync(actualizada.Id);
            return reservaCompleta == null ? null : MapToDTO(reservaCompleta);
        }

        public async Task<bool> DeleteAsync(string correo, string id)
        {
            var reserva = await _reservaRepo.GetByIdAndCorreoAsync(id, correo);
            if (reserva == null) return false;
            
            return await _reservaRepo.DeleteAsync(reserva);
        }
        
        public async Task<List<TipoEventoDTO>> GetAllTiposEventoAsync()
        {
            var tiposEvento = await _reservaRepo.GetAllTiposEventoAsync();
            return tiposEvento.Select(te => new TipoEventoDTO
            {
                Id = te.Id,
                Nombre = te.Nombre,
                Descripcion = te.Descripcion
            }).ToList();
        }
        
        public async Task<TipoEventoDTO?> GetTipoEventoByIdAsync(Guid id)
        {
            var tipoEvento = await _reservaRepo.GetTipoEventoByIdAsync(id);
            if (tipoEvento == null) return null;
            
            return new TipoEventoDTO
            {
                Id = tipoEvento.Id,
                Nombre = tipoEvento.Nombre,
                Descripcion = tipoEvento.Descripcion
            };
        }

        private ReservaResponseDTO MapToDTO(Reserva r)
        {            var dto = new ReservaResponseDTO
            {
                Id = r.Id,
                NombreEvento = r.NombreEvento,
                FechaEjecucion = r.FechaEjecucion,
                FechaRegistro = r.FechaRegistro,
                Descripcion = r.Descripcion,
                Estado = r.Estado,
                PrecioTotal = r.PrecioTotal,
                ClienteId = r.ClienteId,
                NombreCliente = r.Cliente?.Usuario?.Nombre,
                CorreoCliente = r.Cliente?.Usuario?.Correo,
                TelefonoCliente = r.Cliente?.Usuario?.Celular,
                TipoEventoId = r.TiposEvento,
                TipoEventoNombre = r.TiposEventoNavigation?.Nombre,
                ServicioId = r.ServicioId,
                NombreServicio = r.Servicio?.Nombre,
                PrecioAdelanto = r.PrecioAdelanto
            };            if (r.Pagos != null && r.Pagos.Any())
            {
                // Sumar los montos de los pagos
                decimal totalPagado = 0;
                
                foreach (var pago in r.Pagos)
                {
                    // Intentar parsear el monto a decimal si es un string
                    if (pago.Monto != null && decimal.TryParse(pago.Monto, out decimal montoDecimal))
                    {
                        totalPagado += montoDecimal;
                    }
                }
                
                dto.TotalPagado = totalPagado;
                
                // No hay campo de fecha en el modelo Pago, así que usamos la fecha de registro de la reserva
                // como último pago, ya que no tenemos otra referencia temporal
                dto.UltimoPago = r.FechaRegistro;
            }

            return dto;
        }
    }
}
