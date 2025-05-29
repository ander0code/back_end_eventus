using back_end.Modules.reservas.DTOs;
using back_end.Modules.reservas.Models;
using back_end.Modules.reservas.Repositories;
using back_end.Modules.organizador.Repositories;
using back_end.Modules.organizador.Models; 
using back_end.Modules.servicios.Repositories;
using back_end.Modules.clientes.Models;
using back_end.Modules.clientes.Repositories;
using back_end.Core.Utils;

namespace back_end.Modules.reservas.Services
{    public interface IReservaService
    {
        Task<List<ReservaResponseDTO>> GetAllAsync();
        Task<ReservaResponseDTO?> GetByIdAsync(Guid id);
        Task<ReservaResponseDTO?> GetByIdStringAsync(string id);
        Task<ReservaResponseDTO?> CreateAsync(ReservaCreateDTO dto);
        Task<ReservaResponseDTO?> UpdateAsync(Guid id, ReservaUpdateDTO dto);
        Task<ReservaResponseDTO?> UpdateByStringAsync(string id, ReservaUpdateDTO dto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> DeleteByStringAsync(string id);
    }public class ReservaService : IReservaService
    {
        private readonly IReservaRepository _reservaRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IServicioRepository _servicioRepo;
        private readonly IClienteRepository _clienteRepo;
        private readonly ITipoEventoService _tipoEventoService;
        private readonly ILogger<ReservaService> _logger;

        public ReservaService(
            IReservaRepository reservaRepo, 
            IUsuarioRepository usuarioRepo,
            IServicioRepository servicioRepo,
            IClienteRepository clienteRepo,
            ITipoEventoService tipoEventoService,
            ILogger<ReservaService> logger)
        {
            _reservaRepo = reservaRepo;
            _usuarioRepo = usuarioRepo;
            _servicioRepo = servicioRepo;
            _clienteRepo = clienteRepo;
            _tipoEventoService = tipoEventoService;
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
            string clienteId;
            
            if (!string.IsNullOrEmpty(dto.ClienteId))
            {
                // Usar el cliente existente
                clienteId = dto.ClienteId;
                _logger.LogInformation("Usando cliente existente con ID: {ClienteId}", clienteId);
            }
            else if (!string.IsNullOrEmpty(dto.NombreCliente) && !string.IsNullOrEmpty(dto.CorreoElectronico))
            {
                // Crear nuevo cliente automáticamente
                _logger.LogInformation("Creando nuevo cliente para: {Nombre}, {Correo}", dto.NombreCliente, dto.CorreoElectronico);
                
                try
                {
                    // Verificar si ya existe un usuario con ese correo
                    var usuarioExistente = await _usuarioRepo.GetByCorreoAsync(dto.CorreoElectronico);
                    
                    if (usuarioExistente != null)
                    {
                        // Verificar si ya existe un cliente para este usuario
                        var clientesExistentes = await _clienteRepo.GetByCorreoUsuarioAsync(dto.CorreoElectronico);
                        if (clientesExistentes.Any())
                        {
                            clienteId = clientesExistentes.First().Id;
                            _logger.LogInformation("Cliente existente encontrado para el correo {Correo}: {ClienteId}", dto.CorreoElectronico, clienteId);
                        }
                        else
                        {
                            // Crear cliente para usuario existente
                            var nuevoCliente = new Cliente
                            {
                                Id = IdGenerator.GenerateId("Cliente"),
                                UsuarioId = usuarioExistente.Id,
                                TipoCliente = "INDIVIDUAL" // Valor por defecto
                            };
                            
                            var clienteCreado = await _clienteRepo.CreateAsync(nuevoCliente);
                            clienteId = clienteCreado.Id;
                            _logger.LogInformation("Nuevo cliente creado para usuario existente: {ClienteId}", clienteId);
                        }
                    }
                    else
                    {
                        // Crear nuevo usuario y cliente
                        var nuevoUsuario = new Usuario
                        {
                            Id = IdGenerator.GenerateId("Usuario"),
                            Nombre = dto.NombreCliente,
                            Correo = dto.CorreoElectronico,
                            Celular = dto.Telefono
                        };
                        
                        var usuarioCreado = await _usuarioRepo.CreateAsync(nuevoUsuario);
                        
                        var nuevoCliente = new Cliente
                        {
                            Id = IdGenerator.GenerateId("Cliente"),
                            UsuarioId = usuarioCreado.Id,
                            TipoCliente = "INDIVIDUAL" 
                        };
                        
                        var clienteCreado = await _clienteRepo.CreateAsync(nuevoCliente);
                        clienteId = clienteCreado.Id;
                        _logger.LogInformation("Nuevo usuario y cliente creados: UsuarioId={UsuarioId}, ClienteId={ClienteId}", usuarioCreado.Id, clienteId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear cliente automáticamente para {Nombre}, {Correo}", dto.NombreCliente, dto.CorreoElectronico);
                    return null;
                }
            }
            else
            {
                _logger.LogWarning("No se proporcionó ClienteId ni datos suficientes para crear un cliente (se requiere NombreCliente y CorreoElectronico)");
                return null;
            }

            // Obtener o crear el tipo de evento si se proporciona el nombre
            Guid? tipoEventoId = null;
            if (!string.IsNullOrEmpty(dto.TipoEventoNombre))
            {
                tipoEventoId = await _tipoEventoService.GetOrCreateTipoEventoAsync(dto.TipoEventoNombre);
            }

            var reserva = new Reserva
            {
                Id = IdGenerator.GenerateId("Reserva"),
                ClienteId = clienteId,
                NombreEvento = dto.NombreEvento,
                FechaEjecucion = dto.FechaEjecucion,
                FechaRegistro = DateTime.Now,
                TiposEvento = tipoEventoId,
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
        }        public async Task<bool> DeleteAsync(Guid id)
        {
            var reserva = await _reservaRepo.GetByIdAsync(id.ToString());
            if (reserva == null) return false;
            
            return await _reservaRepo.DeleteAsync(reserva);
        }

        public async Task<ReservaResponseDTO?> GetByIdStringAsync(string id)
        {
            var reserva = await _reservaRepo.GetByIdAsync(id);
            return reserva == null ? null : MapToDTO(reserva);
        }

        public async Task<ReservaResponseDTO?> UpdateByStringAsync(string id, ReservaUpdateDTO dto)
        {
            var reserva = await _reservaRepo.GetByIdAsync(id);
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

        public async Task<bool> DeleteByStringAsync(string id)
        {
            var reserva = await _reservaRepo.GetByIdAsync(id);
            if (reserva == null) return false;
            
            return await _reservaRepo.DeleteAsync(reserva);
        }

        private ReservaResponseDTO MapToDTO(Reserva r)
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
            
            // Obtener datos del cliente a través de la relación Usuario
            if (r.Cliente != null)
            {
                if (r.Cliente.Usuario != null)
                {
                    // Los datos están en la entidad Usuario
                    dto.NombreCliente = r.Cliente.Usuario.Nombre;
                    dto.CorreoCliente = r.Cliente.Usuario.Correo;
                    dto.TelefonoCliente = r.Cliente.Usuario.Celular;
                    
                    _logger.LogDebug("Datos del cliente obtenidos correctamente para reserva {ReservaId}: {Nombre}, {Correo}, {Telefono}", 
                        r.Id, dto.NombreCliente, dto.CorreoCliente, dto.TelefonoCliente);
                }
                else
                {
                    _logger.LogWarning("Usuario es null para el cliente {ClienteId} en la reserva {ReservaId}. UsuarioId: {UsuarioId}", 
                        r.ClienteId, r.Id, r.Cliente.UsuarioId);
                }
            }
            else
            {
                _logger.LogWarning("Cliente es null para la reserva {ReservaId} con ClienteId: {ClienteId}", r.Id, r.ClienteId);
            }
              
            return dto;
        }
    }
}
