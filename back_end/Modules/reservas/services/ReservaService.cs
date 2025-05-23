using back_end.Modules.reservas.DTOs;
using back_end.Modules.reservas.Models;
using back_end.Modules.reservas.Repositories;
using back_end.Modules.organizador.Repositories;
using back_end.Modules.servicios.Repositories;
using back_end.Modules.clientes.Models;
using back_end.Modules.clientes.Repositories;
using back_end.Core.Utils;

namespace back_end.Modules.reservas.Services
{    public interface IReservaService
    {
        Task<List<ReservaResponseDTO>> GetAllAsync();
        Task<ReservaResponseDTO?> GetByIdAsync(Guid id);
        Task<ReservaResponseDTO?> CreateAsync(ReservaCreateDTO dto);
        Task<ReservaResponseDTO?> UpdateAsync(Guid id, ReservaUpdateDTO dto);
        Task<bool> DeleteAsync(Guid id);
        
        // Servicios de la reserva
        Task<bool> AddServicioToReservaAsync(Guid reservaId, Guid servicioId);
        Task<bool> RemoveServicioFromReservaAsync(Guid reservaId, Guid servicioId);
        Task<bool> UpdateServicioInReservaAsync(Guid reservaId, Guid servicioId, int cantidad, decimal? precio);
        Task<decimal> CalcularTotalReservaAsync(Guid reservaId);
        Task<bool> ReplaceServicioInReservaAsync(Guid reservaId, Guid oldServicioId, Guid newServicioId, int? cantidad, decimal? precio);
        
        // Tipos de evento
        Task<List<TipoEventoResponseDTO>> GetAllTiposEventoAsync();
        Task<TipoEventoResponseDTO?> GetTipoEventoByIdAsync(Guid id);
    }    public class ReservaService : IReservaService
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

        public async Task<List<ReservaResponseDTO>> GetAllAsync()
        {
            var reservas = await _reservaRepo.GetAllAsync();
            return reservas.Select(MapToDTO).ToList();
        }
        
        public async Task<ReservaResponseDTO?> GetByIdAsync(Guid id)
        {
            var reserva = await _reservaRepo.GetByIdAsync(id.ToString());
            return reserva == null ? null : MapToDTO(reserva);
        }        public async Task<ReservaResponseDTO?> CreateAsync(ReservaCreateDTO dto)
        {
            // Determinar el ClienteId, debe ser proporcionado directamente
            string clienteId;
            
            if (!string.IsNullOrEmpty(dto.ClienteId))
            {
                // Usar el cliente existente
                clienteId = dto.ClienteId;
            }
            else
            {
                // No se proporcionó ClienteId y ahora es obligatorio
                _logger.LogWarning("No se proporcionó ClienteId en la solicitud de reserva");
                return null;
            }

            var reserva = new Reserva
            {
                Id = IdGenerator.GenerateId("Reserva"),
                ClienteId = clienteId,
                NombreEvento = dto.NombreEvento,
                FechaEjecucion = dto.FechaEjecucion,
                FechaRegistro = DateTime.Now,
                TiposEvento = dto.TipoEventoId,
                Descripcion = dto.Descripcion,
                Estado = dto.Estado ?? "Pendiente",
                PrecioTotal = dto.PrecioTotal ?? 0,
                ServicioId = dto.ServicioId,
                PrecioAdelanto = dto.PrecioAdelanto
            };

            var creada = await _reservaRepo.CreateAsync(reserva);
            
            var reservaCompleta = await _reservaRepo.GetByIdAsync(creada.Id);
            return reservaCompleta == null ? null : MapToDTO(reservaCompleta);
        }

        public async Task<ReservaResponseDTO?> UpdateAsync(Guid id, ReservaUpdateDTO dto)
        {
            var reserva = await _reservaRepo.GetByIdAsync(id.ToString());
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
            var reservaCompleta = await _reservaRepo.GetByIdAsync(actualizada.Id);
            return reservaCompleta == null ? null : MapToDTO(reservaCompleta);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var reserva = await _reservaRepo.GetByIdAsync(id.ToString());
            if (reserva == null) return false;
            
            return await _reservaRepo.DeleteAsync(reserva);
        }
        
        public async Task<bool> AddServicioToReservaAsync(Guid reservaId, Guid servicioId)
        {
            var reserva = await _reservaRepo.GetByIdAsync(reservaId.ToString());
            if (reserva == null) return false;
            
            // Como no hay ReservaServicios en el modelo actual, simplemente actualizamos el ServicioId
            reserva.ServicioId = servicioId;
            await _reservaRepo.UpdateAsync(reserva);
            
            return true;
        }
        
        public async Task<bool> RemoveServicioFromReservaAsync(Guid reservaId, Guid servicioId)
        {
            var reserva = await _reservaRepo.GetByIdAsync(reservaId.ToString());
            if (reserva == null) return false;
            
            // Si el servicio actual coincide con el que queremos eliminar, lo quitamos
            if (reserva.ServicioId == servicioId)
            {
                reserva.ServicioId = null;
                await _reservaRepo.UpdateAsync(reserva);
            }
            
            return true;
        }
        
        public async Task<bool> UpdateServicioInReservaAsync(Guid reservaId, Guid servicioId, int cantidad, decimal? precio)
        {
            var reserva = await _reservaRepo.GetByIdAsync(reservaId.ToString());
            if (reserva == null) return false;
            
            // Como no hay cantidad en el modelo actual, solo podemos actualizar el ServicioId y tal vez el precio total
            reserva.ServicioId = servicioId;
            if (precio.HasValue)
            {
                reserva.PrecioTotal = precio.Value;
            }
            
            await _reservaRepo.UpdateAsync(reserva);
            return true;
        }
        
        public async Task<decimal> CalcularTotalReservaAsync(Guid reservaId)
        {
            var reserva = await _reservaRepo.GetByIdAsync(reservaId.ToString());
            if (reserva == null || !reserva.PrecioTotal.HasValue) return 0;
            
            return reserva.PrecioTotal.Value;
        }

        public async Task<bool> ReplaceServicioInReservaAsync(Guid reservaId, Guid oldServicioId, Guid newServicioId, int? cantidad, decimal? precio)
        {
            var reserva = await _reservaRepo.GetByIdAsync(reservaId.ToString());
            if (reserva == null) return false;

            // Si el servicio actual coincide con el que queremos reemplazar, actualizamos
            if (reserva.ServicioId == oldServicioId)
            {
                reserva.ServicioId = newServicioId;
                if (precio.HasValue)
                {
                    reserva.PrecioTotal = precio.Value;
                }
                
                await _reservaRepo.UpdateAsync(reserva);
                return true;
            }
            
            return false;
        }        private ReservaResponseDTO MapToDTO(Reserva r)
        {
            var dto = new ReservaResponseDTO
            {
                Id = r.Id,
                NombreEvento = r.NombreEvento,
                FechaEjecucion = r.FechaEjecucion,
                FechaRegistro = r.FechaRegistro,
                Descripcion = r.Descripcion,
                Estado = r.Estado,
                PrecioTotal = r.PrecioTotal,
                ClienteId = r.ClienteId,
                TipoEventoId = r.TiposEvento,
                TipoEventoNombre = r.TiposEventoNavigation?.Nombre,
                ServicioId = r.ServicioId,
                NombreServicio = r.Servicio?.Nombre,
                PrecioAdelanto = r.PrecioAdelanto
            };
            
            // Intentar acceder a los campos del cliente de manera segura
            if (r.Cliente != null)
            {
                try {
                    var propNombre = r.Cliente.GetType().GetProperty("Nombre");
                    if (propNombre != null) dto.NombreCliente = propNombre.GetValue(r.Cliente)?.ToString();
                    
                    var propCorreo = r.Cliente.GetType().GetProperty("CorreoElectronico");
                    if (propCorreo != null) dto.CorreoCliente = propCorreo.GetValue(r.Cliente)?.ToString();
                    
                    var propTelefono = r.Cliente.GetType().GetProperty("Telefono");
                    if (propTelefono != null) dto.TelefonoCliente = propTelefono.GetValue(r.Cliente)?.ToString();
                } catch (Exception) {
                    // Ignoramos errores de reflexión aquí
                }
            }
            
            return dto;
        }
        
        // Implementación de métodos para TiposEvento
        public async Task<List<TipoEventoResponseDTO>> GetAllTiposEventoAsync()
        {
            var tiposEvento = await _reservaRepo.GetAllTiposEventoAsync();
            return tiposEvento.Select(te => new TipoEventoResponseDTO
            {
                Id = te.Id,
                Nombre = te.Nombre,
                Descripcion = te.Descripcion
            }).ToList();
        }

        public async Task<TipoEventoResponseDTO?> GetTipoEventoByIdAsync(Guid id)
        {
            var tipoEvento = await _reservaRepo.GetTipoEventoByIdAsync(id);
            if (tipoEvento == null) return null;
            
            return new TipoEventoResponseDTO
            {
                Id = tipoEvento.Id,
                Nombre = tipoEvento.Nombre,
                Descripcion = tipoEvento.Descripcion
            };
        }
    }
}
