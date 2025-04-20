using back_end.Modules.reservas.DTOs;
using back_end.Modules.reservas.Models;
using back_end.Modules.reservas.Repositories;
using back_end.Modules.usuarios.Repositories;
using back_end.Modules.servicios.Repositories;
using back_end.Modules.clientes.Models;
using back_end.Modules.clientes.Repositories;

namespace back_end.Modules.reservas.Services
{
    public interface IReservaService
    {
        Task<List<ReservaResponseDTO>> GetByCorreoAsync(string correo);
        Task<ReservaResponseDTO?> GetByIdAsync(string correo, Guid id);
        Task<ReservaResponseDTO?> CreateAsync(string correo, ReservaCreateDTO dto);
        Task<ReservaResponseDTO?> UpdateAsync(string correo, Guid id, ReservaUpdateDTO dto);
        Task<bool> DeleteAsync(string correo, Guid id);
        
        // Servicios de la reserva
        Task<bool> AddServicioToReservaAsync(string correo, Guid reservaId, Guid servicioId, int cantidad, decimal? precio);
        Task<bool> RemoveServicioFromReservaAsync(string correo, Guid reservaId, Guid servicioId);
        Task<bool> UpdateServicioInReservaAsync(string correo, Guid reservaId, Guid servicioId, int cantidad, decimal? precio);
        Task<decimal> CalcularTotalReservaAsync(string correo, Guid reservaId);
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
        
        public async Task<ReservaResponseDTO?> GetByIdAsync(string correo, Guid id)
        {
            var reserva = await _reservaRepo.GetByIdAndCorreoAsync(id, correo);
            return reserva == null ? null : MapToDTO(reserva);
        }

        public async Task<ReservaResponseDTO?> CreateAsync(string correo, ReservaCreateDTO dto)
        {
            var usuario = await _usuarioRepo.GetByCorreoAsync(correo);
            if (usuario == null) return null;

            // Determinar el ClienteId, ya sea utilizando uno existente o creando uno nuevo
            Guid clienteId;
            
            if (dto.ClienteId.HasValue)
            {
                // Usar el cliente existente
                clienteId = dto.ClienteId.Value;
            }
            else if (!string.IsNullOrWhiteSpace(dto.NombreCliente) && !string.IsNullOrWhiteSpace(dto.CorreoCliente))
            {
                // Crear un nuevo cliente
                _logger.LogInformation("Creando cliente nuevo para la reserva: {NombreCliente}, {CorreoCliente}", 
                    dto.NombreCliente, dto.CorreoCliente);
                
                var nuevoCliente = new Cliente
                {
                    Nombre = dto.NombreCliente,
                    CorreoElectronico = dto.CorreoCliente,
                    Telefono = dto.TelefonoCliente,
                    UsuarioId = usuario.Id,
                    TipoCliente = "Cliente de reserva", // Valor por defecto
                    FechaRegistro = DateTime.UtcNow
                };
                
                var clienteCreado = await _clienteRepo.CreateAsync(nuevoCliente);
                clienteId = clienteCreado.Id;
                
                _logger.LogInformation("Cliente creado con ID: {ClienteId}", clienteId);
            }
            else
            {
                // No se proporcionó ClienteId ni datos para crear uno nuevo
                _logger.LogWarning("No se proporcionó ClienteId ni datos suficientes para crear cliente");
                return null;
            }

            var reserva = new Reserva
            {
                UsuarioId = usuario.Id,
                ClienteId = clienteId,
                NombreEvento = dto.NombreEvento,
                FechaEvento = dto.FechaEvento,
                HoraEvento = dto.HoraEvento,
                TipoEvento = dto.TipoEvento,
                Descripcion = dto.Descripcion,
                Estado = dto.Estado ?? "Pendiente",
                PrecioTotal = dto.PrecioTotal ?? 0
            };

            var creada = await _reservaRepo.CreateAsync(reserva);
            
            // Si se especificó un servicio, agregarlo a la reserva
            if (dto.ServicioId.HasValue)
            {
                await AddServicioToReservaAsync(correo, creada.Id, dto.ServicioId.Value, 1, null);
                
                // Recalcular el precio total
                var total = await CalcularTotalReservaAsync(correo, creada.Id);
                creada.PrecioTotal = total;
                await _reservaRepo.UpdateAsync(creada);
            }
            
            // Volvemos a cargar la reserva completa con sus relaciones
            var reservaCompleta = await _reservaRepo.GetByIdAndCorreoAsync(creada.Id, correo);
            return reservaCompleta == null ? null : MapToDTO(reservaCompleta);
        }

        public async Task<ReservaResponseDTO?> UpdateAsync(string correo, Guid id, ReservaUpdateDTO dto)
        {
            var reserva = await _reservaRepo.GetByIdAndCorreoAsync(id, correo);
            if (reserva == null) return null;

            // Actualizar solo los campos proporcionados
            if (dto.Estado != null) reserva.Estado = dto.Estado;
            if (dto.Descripcion != null) reserva.Descripcion = dto.Descripcion;
            if (dto.PrecioTotal != null) reserva.PrecioTotal = dto.PrecioTotal;
            
            // "Observaciones" no está en el modelo de base de datos, se omite

            var actualizada = await _reservaRepo.UpdateAsync(reserva);
            return MapToDTO(actualizada);
        }

        public async Task<bool> DeleteAsync(string correo, Guid id)
        {
            var reserva = await _reservaRepo.GetByIdAndCorreoAsync(id, correo);
            if (reserva == null) return false;
            
            // Primero eliminar los servicios relacionados
            foreach (var reservaServicio in reserva.ReservaServicios.ToList())
            {
                await _reservaRepo.RemoveReservaServicioAsync(reservaServicio);
            }

            return await _reservaRepo.DeleteAsync(reserva);
        }
        
        public async Task<bool> AddServicioToReservaAsync(string correo, Guid reservaId, Guid servicioId, int cantidad, decimal? precio)
        {
            // Verificar que la reserva pertenezca al usuario
            var reserva = await _reservaRepo.GetByIdAndCorreoAsync(reservaId, correo);
            if (reserva == null) return false;
            
            // Verificar que el servicio exista
            var servicio = await _servicioRepo.GetByIdAsync(servicioId);
            if (servicio == null) return false;
            
            // Verificar si ya existe el servicio en la reserva
            var existingServicio = reserva.ReservaServicios.FirstOrDefault(rs => rs.ServicioId == servicioId);
            if (existingServicio != null)
            {
                // Actualizar la cantidad
                existingServicio.CantidadItems = cantidad;
                existingServicio.Precio = precio ?? servicio.PrecioBase;
                await _reservaRepo.UpdateReservaServicioAsync(existingServicio);
            }
            else
            {
                // Crear nueva relación
                var reservaServicio = new ReservaServicio
                {
                    ReservaId = reservaId,
                    ServicioId = servicioId,
                    CantidadItems = cantidad,
                    Precio = precio ?? servicio.PrecioBase
                };
                
                await _reservaRepo.AddReservaServicioAsync(reservaServicio);
            }
            
            // Recalcular el total
            var total = await CalcularTotalReservaAsync(correo, reservaId);
            reserva.PrecioTotal = total;
            await _reservaRepo.UpdateAsync(reserva);
            
            return true;
        }
        
        public async Task<bool> RemoveServicioFromReservaAsync(string correo, Guid reservaId, Guid servicioId)
        {
            // Verificar que la reserva pertenezca al usuario
            var reserva = await _reservaRepo.GetByIdAndCorreoAsync(reservaId, correo);
            if (reserva == null) return false;
            
            // Buscar el servicio en la reserva
            var reservaServicio = reserva.ReservaServicios.FirstOrDefault(rs => rs.ServicioId == servicioId);
            if (reservaServicio == null) return false;
            
            // Eliminar la relación
            var result = await _reservaRepo.RemoveReservaServicioAsync(reservaServicio);
            
            if (result)
            {
                // Recalcular el total
                var total = await CalcularTotalReservaAsync(correo, reservaId);
                reserva.PrecioTotal = total;
                await _reservaRepo.UpdateAsync(reserva);
            }
            
            return result;
        }
        
        public async Task<bool> UpdateServicioInReservaAsync(string correo, Guid reservaId, Guid servicioId, int cantidad, decimal? precio)
        {
            // Verificar que la reserva pertenezca al usuario
            var reserva = await _reservaRepo.GetByIdAndCorreoAsync(reservaId, correo);
            if (reserva == null) return false;
            
            // Buscar el servicio en la reserva
            var reservaServicio = reserva.ReservaServicios.FirstOrDefault(rs => rs.ServicioId == servicioId);
            if (reservaServicio == null) return false;
            
            // Actualizar la cantidad y precio
            reservaServicio.CantidadItems = cantidad;
            if (precio.HasValue)
            {
                reservaServicio.Precio = precio.Value;
            }
            
            var result = await _reservaRepo.UpdateReservaServicioAsync(reservaServicio);
            
            // Recalcular el total
            var total = await CalcularTotalReservaAsync(correo, reservaId);
            reserva.PrecioTotal = total;
            await _reservaRepo.UpdateAsync(reserva);
            
            return result != null;
        }
        
        public async Task<decimal> CalcularTotalReservaAsync(string correo, Guid reservaId)
        {
            // Verificar que la reserva pertenezca al usuario
            var reserva = await _reservaRepo.GetByIdAndCorreoAsync(reservaId, correo);
            if (reserva == null) return 0;
            
            decimal total = 0;
            
            // Sumar el precio de cada servicio por su cantidad
            foreach (var rs in reserva.ReservaServicios)
            {
                if (rs.Precio.HasValue && rs.CantidadItems.HasValue)
                {
                    total += rs.Precio.Value * rs.CantidadItems.Value;
                }
            }
            
            return total;
        }

        private ReservaResponseDTO MapToDTO(Reserva r)
        {
            var servicios = r.ReservaServicios.FirstOrDefault();
            
            return new ReservaResponseDTO
            {
                Id = r.Id,
                NombreEvento = r.NombreEvento,
                FechaEvento = r.FechaEvento,
                HoraEvento = r.HoraEvento,
                TipoEvento = r.TipoEvento,
                Descripcion = r.Descripcion,
                Estado = r.Estado,
                PrecioTotal = r.PrecioTotal,
                // Obtener datos del cliente desde la relación
                NombreCliente = r.Cliente?.Nombre,
                CorreoCliente = r.Cliente?.CorreoElectronico,
                TelefonoCliente = r.Cliente?.Telefono,
                // Si hay al menos un servicio, mostrar su ID
                ServicioId = servicios?.ServicioId,
                FechaReserva = DateTime.UtcNow // No está en el modelo, usamos la fecha actual
            };
        }
    }
}
