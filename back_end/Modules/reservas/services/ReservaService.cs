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
using back_end.Modules.Item.Services;
using back_end.Modules.Item.Repositories;

namespace back_end.Modules.reservas.Services
{    
    public interface IReservaService
    {
        Task<List<ReservaResponseDTO>> GetAllAsync();
        Task<ReservaResponseDTO?> GetByIdAsync(string id);
        Task<ReservaResponseDTO?> GetByIdStringAsync(string id);
        Task<ReservaResponseDTO?> CreateAsync(ReservaCreateDTO dto);
        Task<ReservaResponseDTO?> UpdateAsync(string id, ReservaUpdateDTO dto);
        Task<ReservaResponseDTO?> UpdateByStringAsync(string id, ReservaUpdateDTO dto);
        Task<bool> DeleteAsync(string id);
        Task<bool> DeleteByStringAsync(string id);
    }
    
    public class ReservaService : IReservaService
    {
        private readonly IReservaRepository _reservaRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IServicioRepository _servicioRepo;
        private readonly IClienteRepository _clienteRepo;
        private readonly ITipoEventoService _tipoEventoService;
        private readonly IPagosService _pagosService;
        private readonly IItemService _itemService;
        private readonly IItemRepository _itemRepository;
        private readonly ILogger<ReservaService> _logger;

        public ReservaService(
            IReservaRepository reservaRepo, 
            IUsuarioRepository usuarioRepo,
            IServicioRepository servicioRepo,
            IClienteRepository clienteRepo,
            ITipoEventoService tipoEventoService,
            IPagosService pagosService,
            IItemService itemService,
            IItemRepository itemRepository,
            ILogger<ReservaService> logger)
        {
            _reservaRepo = reservaRepo;
            _usuarioRepo = usuarioRepo;
            _servicioRepo = servicioRepo;
            _clienteRepo = clienteRepo;
            _tipoEventoService = tipoEventoService;
            _pagosService = pagosService;
            _itemService = itemService;
            _itemRepository = itemRepository;
            _logger = logger;
        }

        private async Task<ReservaResponseDTO> MapToDTOAsync(Reserva r)
        {
            // Obtener total pagado usando el servicio de pagos
            var totalPagado = await _pagosService.GetTotalPagadoByReservaIdAsync(r.Id);
            
            // Obtener fecha del último pago
            var ultimoPago = await _pagosService.GetUltimoPagoFechaByReservaIdAsync(r.Id);

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
                PrecioAdelanto = r.PrecioAdelanto,
                TotalPagado = totalPagado,
                UltimoPago = ultimoPago
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

        public async Task<List<ReservaResponseDTO>> GetAllAsync()
        {
            var reservas = await _reservaRepo.GetAllAsync();
            var result = new List<ReservaResponseDTO>();
            
            foreach (var reserva in reservas)
            {
                result.Add(await MapToDTOAsync(reserva));
            }
            
            return result;
        }
        
        public async Task<ReservaResponseDTO?> GetByIdAsync(string id)
        {
            var reserva = await _reservaRepo.GetByIdAsync(id.ToString());
            return reserva == null ? null : await MapToDTOAsync(reserva);
        }

        public async Task<ReservaResponseDTO?> CreateAsync(ReservaCreateDTO dto)
        {
            // Validar stock del servicio antes de crear la reserva
            if (!string.IsNullOrEmpty(dto.ServicioId))
            {
                var stockValido = await ValidarStockServicioAsync(dto.ServicioId);
                if (!stockValido.esValido)
                {
                    _logger.LogWarning("No se puede crear la reserva. Stock insuficiente para el servicio {ServicioId}: {Mensaje}", 
                        dto.ServicioId, stockValido.mensaje);
                    throw new InvalidOperationException($"Stock insuficiente: {stockValido.mensaje}");
                }
            }

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

            // Actualizar stock disponible de los items del servicio
            if (!string.IsNullOrEmpty(dto.ServicioId))
            {
                await ActualizarStockServicioEnUsoAsync(dto.ServicioId);
            }

            // Crear pago de adelanto automáticamente
            if (dto.PrecioAdelanto.HasValue && dto.PrecioAdelanto.Value > 0)
            {
                try
                {

                    var pagoAdelantoDto = new PagoCreateDTO
                    {
                        IdReserva = creada.Id,
                        NombreTipoPago = "adelanto",
                        Monto = dto.PrecioAdelanto.Value.ToString(),
                        NombreReserva = creada.NombreEvento
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
            return reservaCompleta == null ? null : await MapToDTOAsync(reservaCompleta);
        }

        private async Task<(bool esValido, string mensaje)> ValidarStockServicioAsync(string servicioId)
        {
            try
            {
                // Usar el método optimizado del repositorio
                var resultadoValidacion = await _servicioRepo.ValidarStockServicioAsync(servicioId);
                return resultadoValidacion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar stock del servicio {ServicioId}", servicioId);
                return (false, "Error al validar stock del servicio");
            }
        }

        private async Task ActualizarStockServicioEnUsoAsync(string servicioId)
        {
            try
            {
                // Obtener todos los items del servicio de una vez
                var itemsIds = await _servicioRepo.GetItemsIdsFromServicioAsync(servicioId);
                
                if (itemsIds.Any())
                {
                    // Recalcular stock en lote para todos los items del servicio
                    await _itemService.RecalcularStockDisponibleBatchAsync(itemsIds);
                    
                    _logger.LogInformation("Stock actualizado en lote para {Count} items del servicio {ServicioId}", 
                        itemsIds.Count, servicioId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock en uso para servicio {ServicioId}", servicioId);
            }
        }

        public async Task<ReservaResponseDTO?> UpdateAsync(string id, ReservaUpdateDTO dto)
        {
            var reserva = await _reservaRepo.GetByIdAsync(id);
            if (reserva == null) return null;

            var estadoAnterior = reserva.Estado;
            var servicioIdAnterior = reserva.ServicioId;

            // Si está cambiando de Finalizado/Cancelado a Pendiente/Confirmado, validar stock primero
            if (!string.IsNullOrEmpty(dto.Estado) && dto.Estado != estadoAnterior)
            {
                var estadosLiberadores = new[] { "Finalizado", "Cancelada", "Cancelado" };
                var estadosQueUsan = new[] { "Pendiente", "Confirmado" };
                
                if (estadosQueUsan.Contains(dto.Estado, StringComparer.OrdinalIgnoreCase))
                {
                    // Verificar si viene de un estado liberador o es un cambio entre estados que usan stock
                    var servicioAValidar = !string.IsNullOrEmpty(dto.ServicioId) ? dto.ServicioId : servicioIdAnterior;
                    
                    if (!string.IsNullOrEmpty(servicioAValidar))
                    {
                        _logger.LogInformation("Validando stock para cambio de estado a {Estado} en reserva {ReservaId} con servicio {ServicioId}", 
                            dto.Estado, id, servicioAValidar);
                            
                        var stockValido = await ValidarStockServicioAsync(servicioAValidar);
                        if (!stockValido.esValido)
                        {
                            _logger.LogWarning("Cambio de estado rechazado por stock insuficiente: {Mensaje}", stockValido.mensaje);
                            throw new InvalidOperationException($"No se puede cambiar el estado a {dto.Estado} porque {stockValido.mensaje}");
                        }
                    }
                }
            }

            if (dto.NombreEvento != null) reserva.NombreEvento = dto.NombreEvento;
            if (dto.FechaEjecucion.HasValue) reserva.FechaEjecucion = dto.FechaEjecucion;
            if (dto.Descripcion != null) reserva.Descripcion = dto.Descripcion;
            if (dto.Estado != null) reserva.Estado = dto.Estado;
            if (dto.PrecioTotal.HasValue) reserva.PrecioTotal = dto.PrecioTotal;
            if (!string.IsNullOrEmpty(dto.ServicioId)) reserva.ServicioId = dto.ServicioId;
            if (dto.PrecioAdelanto.HasValue) reserva.PrecioAdelanto = dto.PrecioAdelanto;
            
            if (!string.IsNullOrEmpty(dto.TipoEventoNombre))
            {
                reserva.TiposEvento = await _tipoEventoService.GetOrCreateTipoEventoAsync(dto.TipoEventoNombre);
            }
            
            var actualizada = await _reservaRepo.UpdateAsync(reserva);

            // Recalcular stock después de la actualización si cambió el servicio o estado
            if (!string.IsNullOrEmpty(reserva.ServicioId))
            {
                await ActualizarStockServicioEnUsoAsync(reserva.ServicioId);
            }

            var reservaCompleta = await _reservaRepo.GetByIdAsync(actualizada.Id);
            return reservaCompleta == null ? null : await MapToDTOAsync(reservaCompleta);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var reserva = await _reservaRepo.GetByIdAsync(id.ToString());
            if (reserva == null) return false;

            return await _reservaRepo.DeleteAsync(reserva);
        }

        public async Task<ReservaResponseDTO?> GetByIdStringAsync(string id)
        {
            var reserva = await _reservaRepo.GetByIdAsync(id);
            return reserva == null ? null : await MapToDTOAsync(reserva);
        }

        public async Task<ReservaResponseDTO?> UpdateByStringAsync(string id, ReservaUpdateDTO dto)
        {
            var reserva = await _reservaRepo.GetByIdAsync(id);
            if (reserva == null) return null;

            var estadoAnterior = reserva.Estado;
            var servicioIdAnterior = reserva.ServicioId;

            // Si está cambiando de Finalizado/Cancelado a Pendiente/Confirmado, validar stock primero
            if (!string.IsNullOrEmpty(dto.Estado) && dto.Estado != estadoAnterior)
            {
                var estadosLiberadores = new[] { "Finalizado", "Cancelada", "Cancelado" };
                var estadosQueUsan = new[] { "Pendiente", "Confirmado" };
                
                if (estadosQueUsan.Contains(dto.Estado, StringComparer.OrdinalIgnoreCase))
                {
                    // Verificar si viene de un estado liberador o es un cambio entre estados que usan stock
                    var servicioAValidar = !string.IsNullOrEmpty(dto.ServicioId) ? dto.ServicioId : servicioIdAnterior;
                    
                    if (!string.IsNullOrEmpty(servicioAValidar))
                    {
                        _logger.LogInformation("Validando stock para cambio de estado a {Estado} en reserva {ReservaId} con servicio {ServicioId}", 
                            dto.Estado, id, servicioAValidar);
                            
                        var stockValido = await ValidarStockServicioAsync(servicioAValidar);
                        if (!stockValido.esValido)
                        {
                            _logger.LogWarning("Cambio de estado rechazado por stock insuficiente: {Mensaje}", stockValido.mensaje);
                            throw new InvalidOperationException($"No se puede cambiar el estado a {dto.Estado} porque {stockValido.mensaje}");
                        }
                    }
                }
            }

            if (dto.NombreEvento != null) reserva.NombreEvento = dto.NombreEvento;
            if (dto.FechaEjecucion.HasValue) reserva.FechaEjecucion = dto.FechaEjecucion;
            if (dto.Descripcion != null) reserva.Descripcion = dto.Descripcion;
            if (dto.Estado != null) reserva.Estado = dto.Estado;
            if (dto.PrecioTotal.HasValue) reserva.PrecioTotal = dto.PrecioTotal;
            if (!string.IsNullOrEmpty(dto.ServicioId)) reserva.ServicioId = dto.ServicioId;
            if (dto.PrecioAdelanto.HasValue) reserva.PrecioAdelanto = dto.PrecioAdelanto;
            
            if (!string.IsNullOrEmpty(dto.TipoEventoNombre))
            {
                reserva.TiposEvento = await _tipoEventoService.GetOrCreateTipoEventoAsync(dto.TipoEventoNombre);
            }
            
            var actualizada = await _reservaRepo.UpdateAsync(reserva);

            // Recalcular stock después de la actualización si cambió el servicio o estado
            if (!string.IsNullOrEmpty(reserva.ServicioId))
            {
                await ActualizarStockServicioEnUsoAsync(reserva.ServicioId);
            }

            var reservaCompleta = await _reservaRepo.GetByIdAsync(actualizada.Id);
            return reservaCompleta == null ? null : await MapToDTOAsync(reservaCompleta);
        }

        public async Task<bool> DeleteByStringAsync(string id)
        {
            var reserva = await _reservaRepo.GetByIdAsync(id);
            if (reserva == null) return false;
            
            // Antes de eliminar, recalcular el stock de los items del servicio
            if (!string.IsNullOrEmpty(reserva.ServicioId))
            {
                var servicio = await _servicioRepo.GetByIdAsync(reserva.ServicioId);
                if (servicio?.DetalleServicios != null)
                {
                    // Eliminar la reserva primero
                    var resultado = await _reservaRepo.DeleteAsync(reserva);
                    
                    if (resultado)
                    {
                        // Después recalcular el stock de todos los items del servicio
                        foreach (var detalle in servicio.DetalleServicios)
                        {
                            if (!string.IsNullOrEmpty(detalle.InventarioId))
                            {
                                await _itemService.RecalcularStockDisponibleAsync(detalle.InventarioId);
                            }
                        }
                    }
                    
                    return resultado;
                }
            }
            
            return await _reservaRepo.DeleteAsync(reserva);
        }
    }
}
