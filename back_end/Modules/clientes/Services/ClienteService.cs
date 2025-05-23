using back_end.Modules.clientes.DTOs;
using back_end.Modules.clientes.Models;
using back_end.Modules.clientes.Repositories;
using back_end.Modules.organizador.Models;
using back_end.Modules.organizador.Repositories;
using back_end.Core.Utils;

namespace back_end.Modules.clientes.Services
{
    public interface IClienteService
    {
        Task<List<ClienteResponseDTO>> GetByUsuarioCorreoAsync(string correo);
        Task<ClienteResponseDTO?> CreateAsync(string correo, ClienteCreateDTO dto);
        Task<ClienteResponseDTO?> UpdateAsync(string id, ClienteUpdateDTO dto);
        Task<bool> DeleteAsync(string id);
    }

    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _repository;
        private readonly IUsuarioRepository _usuarioRepository;

        public ClienteService(IClienteRepository repository, IUsuarioRepository usuarioRepository)
        {
            _repository = repository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<List<ClienteResponseDTO>> GetByUsuarioCorreoAsync(string correo)
        {
            var clientes = await _repository.GetByCorreoUsuarioAsync(correo);
            return clientes.Select(MapToDTO).ToList();
        }        public async Task<ClienteResponseDTO?> CreateAsync(string correo, ClienteCreateDTO dto)
        {
            // Primero verificamos si existe el usuario organizador que está creando al cliente
            var organizador = await _usuarioRepository.GetByCorreoAsync(correo);
            if (organizador == null) return null;
            
            // Luego verificamos si ya existe un usuario con el correo proporcionado
            Usuario? clienteUsuario = null;
            if (!string.IsNullOrEmpty(dto.CorreoElectronico))
            {
                clienteUsuario = await _usuarioRepository.GetByCorreoAsync(dto.CorreoElectronico);
            }
            
            if (clienteUsuario == null)
            {
                // Si no existe, creamos un nuevo usuario para el cliente
                clienteUsuario = new Usuario
                {
                    Id = IdGenerator.GenerateId("Usuario"),
                    Nombre = dto.Nombre,
                    Apellido = null, // Como indicaste, lo dejamos nulo
                    Correo = dto.CorreoElectronico,
                    Celular = dto.Telefono
                };
                
                // Guardamos el nuevo usuario
                clienteUsuario = await _usuarioRepository.CreateAsync(clienteUsuario);
            }

            // Ahora creamos el cliente asociado al usuario
            var cliente = new Cliente
            {
                Id = IdGenerator.GenerateId("Cliente"),
                UsuarioId = clienteUsuario.Id,
                TipoCliente = dto.TipoCliente,
                Direccion = dto.Direccion,
                Ruc = dto.Ruc,
                RazonSocial = dto.RazonSocial
            };

            var creado = await _repository.CreateAsync(cliente);
            return MapToDTO(creado);
        }

        public async Task<ClienteResponseDTO?> UpdateAsync(string id, ClienteUpdateDTO dto)
        {
            var cliente = await _repository.GetByIdAsync(id);
            if (cliente == null) return null;

            cliente.TipoCliente = dto.TipoCliente ?? cliente.TipoCliente;
            cliente.Direccion = dto.Direccion ?? cliente.Direccion;
            cliente.Ruc = dto.Ruc ?? cliente.Ruc;
            cliente.RazonSocial = dto.RazonSocial ?? cliente.RazonSocial;

            var actualizado = await _repository.UpdateAsync(cliente);
            return MapToDTO(actualizado);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var cliente = await _repository.GetByIdAsync(id);
            if (cliente == null) return false;

            return await _repository.DeleteAsync(cliente);
        }

        private ClienteResponseDTO MapToDTO(Cliente c) => new ClienteResponseDTO
        {
            Id = c.Id,
            TipoCliente = c.TipoCliente,
            Direccion = c.Direccion,
            Ruc = c.Ruc,
            RazonSocial = c.RazonSocial,
            UsuarioId = c.UsuarioId,
            NombreUsuario = c.Usuario != null ? $"{c.Usuario.Nombre} {c.Usuario.Apellido}" : string.Empty,
            CorreoUsuario = c.Usuario?.Correo,
            TotalReservas = c.Reservas.Count,
            UltimaFechaReserva = c.Reservas
                .OrderByDescending(r => r.FechaEjecucion)
                .Select(r => r.FechaEjecucion)
                .FirstOrDefault()
        };
    }
}
