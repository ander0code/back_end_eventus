using back_end.Modules.clientes.DTOs;
using back_end.Modules.clientes.Models;
using back_end.Modules.clientes.Repositories;
using back_end.Modules.usuarios.Repositories;

namespace back_end.Modules.clientes.Services
{
    public interface IClienteService
    {
        Task<List<ClienteResponseDTO>> GetByUsuarioCorreoAsync(string correo);
        Task<ClienteResponseDTO?> CreateAsync(ClienteCreateDTO dto);
        Task<ClienteResponseDTO?> UpdateAsync(Guid id, ClienteUpdateDTO dto);
        Task<bool> DeleteAsync(Guid id);
        Task<List<ClienteResponseDTO>> SearchAsync(string? query);
        Task<List<ClienteResponseDTO>> FilterByTipoClienteAsync(string tipoCliente);
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
        }

        public async Task<ClienteResponseDTO?> CreateAsync(ClienteCreateDTO dto)
        {
            var usuario = await _usuarioRepository.GetByCorreoAsync(dto.UsuarioCorreo);
            if (usuario == null) return null;

            var cliente = new Cliente
            {
                UsuarioId = usuario.Id,
                TipoCliente = dto.TipoCliente,
                Nombre = dto.Nombre,
                CorreoElectronico = dto.CorreoElectronico,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion,
                FechaRegistro = DateTime.UtcNow
            };

            var creado = await _repository.CreateAsync(cliente);
            return MapToDTO(creado);
        }

        public async Task<ClienteResponseDTO?> UpdateAsync(Guid id, ClienteUpdateDTO dto)
        {
            var cliente = await _repository.GetByIdAsync(id);
            if (cliente == null) return null;

            cliente.TipoCliente = dto.TipoCliente ?? cliente.TipoCliente;
            cliente.Nombre = dto.Nombre ?? cliente.Nombre;
            cliente.CorreoElectronico = dto.CorreoElectronico ?? cliente.CorreoElectronico;
            cliente.Telefono = dto.Telefono ?? cliente.Telefono;
            cliente.Direccion = dto.Direccion ?? cliente.Direccion;

            var actualizado = await _repository.UpdateAsync(cliente);
            return MapToDTO(actualizado);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var cliente = await _repository.GetByIdAsync(id);
            if (cliente == null) return false;

            return await _repository.DeleteAsync(cliente);
        }

        public async Task<List<ClienteResponseDTO>> SearchAsync(string? query)
        {
            var clientes = await _repository.SearchAsync(query);
            return clientes.Select(MapToDTO).ToList();
        }

        public async Task<List<ClienteResponseDTO>> FilterByTipoClienteAsync(string tipoCliente)
        {
            var clientes = await _repository.FilterByTipoClienteAsync(tipoCliente);
            return clientes.Select(MapToDTO).ToList();
        }

        private ClienteResponseDTO MapToDTO(Cliente c) => new ClienteResponseDTO
        {
            Id = c.Id,
            TipoCliente = c.TipoCliente,
            Nombre = c.Nombre,
            CorreoElectronico = c.CorreoElectronico,
            Telefono = c.Telefono,
            Direccion = c.Direccion,
            FechaRegistro = c.FechaRegistro
        };
    }
}
