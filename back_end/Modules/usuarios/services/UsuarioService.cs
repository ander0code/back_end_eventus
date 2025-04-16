using back_end.Modules.usuarios.DTOs;
using back_end.Modules.usuarios.Models;
using back_end.Modules.usuarios.Repositories;

namespace back_end.Modules.usuarios.services
{
    public interface IUsuarioService
    {
        Task<List<UsuarioResponseDTO>> GetAllAsync();
        Task<UsuarioResponseDTO?> GetByIdAsync(int id);
        Task<UsuarioResponseDTO?> UpdateAsync(int id, UsuarioUpdateDTO dto);
    }

    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repository;

        public UsuarioService(IUsuarioRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UsuarioResponseDTO>> GetAllAsync()
        {
            var usuarios = await _repository.GetAllAsync();
            return usuarios.Select(u => MapToDTO(u)).ToList();
        }

        public async Task<UsuarioResponseDTO?> GetByIdAsync(int id)
        {
            var usuario = await _repository.GetByIdAsync(id);
            return usuario == null ? null : MapToDTO(usuario);
        }

        public async Task<UsuarioResponseDTO?> UpdateAsync(int id, UsuarioUpdateDTO dto)
        {
            var usuario = await _repository.GetByIdAsync(id);
            if (usuario == null)
                return null;

            // Actualizar campos permitidos
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
