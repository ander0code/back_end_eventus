using back_end.Modules.usuarios.DTOs;
using back_end.Modules.usuarios.Models;
using back_end.Modules.usuarios.Repositories;

namespace back_end.Modules.usuarios.services
{
    public interface IUsuarioService
    {
        Task<UsuarioResponseDTO?> GetByCorreoAsync(string correo);
        Task<UsuarioResponseDTO?> UpdateByCorreoAsync(string correo, UsuarioUpdateDTO dto);
    }

    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repository;

        public UsuarioService(IUsuarioRepository repository)
        {
            _repository = repository;
        }

        public async Task<UsuarioResponseDTO?> GetByCorreoAsync(string correo)
        {
            var usuario = await _repository.GetByCorreoAsync(correo);
            return usuario == null ? null : MapToDTO(usuario);
        }

        public async Task<UsuarioResponseDTO?> UpdateByCorreoAsync(string correo, UsuarioUpdateDTO dto)
        {
            var usuario = await _repository.GetByCorreoAsync(correo);
            if (usuario == null)
                return null;

            usuario.Nombre = dto.Nombre ?? usuario.Nombre;
            usuario.Apellido = dto.Apellido ?? usuario.Apellido;
            usuario.Telefono = dto.Telefono ?? usuario.Telefono;
            usuario.Verificado = dto.Verificado ?? usuario.Verificado;

            var actualizado = await _repository.UpdateAsync(usuario);
            return MapToDTO(actualizado);
        }

        private UsuarioResponseDTO MapToDTO(Usuario u) => new UsuarioResponseDTO
        {
            Id = u.Id,
            Nombre = u.Nombre,
            Apellido = u.Apellido,
            Correo = u.Correo,
            Telefono = u.Telefono,
            Verificado = u.Verificado,
            FechaRegistro = u.FechaRegistro
        };
    }
}

