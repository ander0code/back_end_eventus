using back_end.Modules.reservas.DTOs;
using back_end.Modules.reservas.Models;
using back_end.Modules.reservas.Repositories;
using back_end.Modules.organizador.Repositories;
using back_end.Modules.organizador.Models; 
using back_end.Modules.servicios.Repositories;
using back_end.Modules.clientes.Models;
using back_end.Modules.clientes.Repositories;
using back_end.Modules.pagos.services;
using back_end.Modules.pagos.DTOs;
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
        private readonly IPagosService _pagosService;
        private readonly ILogger<ReservaService> _logger;

        public ReservaService(
            IReservaRepository reservaRepo, 
            IUsuarioRepository usuarioRepo,
            IServicioRepository servicioRepo,
            IClienteRepository clienteRepo,
            ITipoEventoService tipoEventoService,
            IPagosService pagosService,
            ILogger<ReservaService> logger)
        {
            _reservaRepo = reservaRepo;
            _usuarioRepo = usuarioRepo;
            _servicioRepo = servicioRepo;
            _clienteRepo = clienteRepo;
            _tipoEventoService = tipoEventoService;
            _pagosService = pagosService;
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
        }

        public async Task<ReservaResponseDTO?> CreateAsync(ReservaCreateDTO dto)
        {
            string clienteId;

            if (!string.IsNullOrEmpty(dto.ClienteId))
            {
                clienteId = dto.ClienteId;
            }
            else if (!string.IsNullOrEmpty(dto.NombreCliente) && !string.IsNullOrEmpty(dto.CorreoElectronico))
            {
                try
                {
                    var usuarioExistente = await _usuarioRepo.GetByCorreoAsync(dto.CorreoElectronico);

                    if (usuarioExistente != null)
                    {
                        var clientesExistentes = await _clienteRepo.GetByCorreoUsuarioAsync(dto.CorreoElectronico);
                        if (clientesExistentes.Any())
                        {
                            clienteId = clientesExistentes.First().Id;
                        }
                        else
                        {
                            var nuevoCliente = new Cliente
                            {
                                Id = IdGenerator.GenerateId("Cliente"),
                                UsuarioId = usuarioExistente.Id,
                                TipoCliente = "INDIVIDUAL"
                            };

                            var clienteCreado = await _clienteRepo.CreateAsync(nuevoCliente);
                            clienteId = clienteCreado.Id;
                        }
                    }
                    else
                    {
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

            // Crear pago de adelanto automáticamente
            if (dto.PrecioAdelanto.HasValue && dto.PrecioAdelanto.Value > 0)
            {
                try
                {
                    var pagoAdelantoDto = new PagoCreateDTO
                    {
                        IdReserva = creada.Id,
                        NombreTipoPago = "Adelanto",
                        Monto = dto.PrecioAdelanto.Value.ToString()
                    };

                    var pagoCreado = await _pagosService.CreateAsync(pagoAdelantoDto);
                    if (pagoCreado == null)
                    {
                        _logger.LogWarning("No se pudo crear el pago de adelanto para la reserva {ReservaId}", creada.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear pago de adelanto para reserva {ReservaId}", creada.Id);
                }
            }

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
            if (dto.ServicioId.HasValue) reserva.ServicioId = dto.ServicioId;
            if (dto.PrecioAdelanto.HasValue) reserva.PrecioAdelanto = dto.PrecioAdelanto;
            
            if (!string.IsNullOrEmpty(dto.TipoEventoNombre))
            {
                reserva.TiposEvento = await _tipoEventoService.GetOrCreateTipoEventoAsync(dto.TipoEventoNombre);
            }
            
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
            if (dto.ServicioId.HasValue) reserva.ServicioId = dto.ServicioId;
            if (dto.PrecioAdelanto.HasValue) reserva.PrecioAdelanto = dto.PrecioAdelanto;
            
            if (!string.IsNullOrEmpty(dto.TipoEventoNombre))
            {
                reserva.TiposEvento = await _tipoEventoService.GetOrCreateTipoEventoAsync(dto.TipoEventoNombre);
            }
            
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
            
            if (r.Cliente != null)
            {
                if (r.Cliente.Usuario != null)
                {
                    dto.NombreCliente = r.Cliente.Usuario.Nombre;
                    dto.CorreoCliente = r.Cliente.Usuario.Correo;
                    dto.TelefonoCliente = r.Cliente.Usuario.Celular;
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
