using back_end.Modules.organizador.DTOs;
using back_end.Modules.organizador.Models;
using back_end.Modules.organizador.Repositories;
using Microsoft.Extensions.Logging;

namespace back_end.Modules.organizador.services
{
    public interface IUsuarioService
    {
        Task<List<UsuarioResponseDTO>> GetAllAsync();
        Task<UsuarioResponseDTO?> GetByIdAsync(string id);
        Task<UsuarioResponseDTO?> GetByCorreoAsync(string correo);
        Task<UsuarioResponseDTO?> UpdateAsync(string id, UsuarioUpdateDTO dto);
    }

    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repository;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(IUsuarioRepository repository, ILogger<UsuarioService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<UsuarioResponseDTO>> GetAllAsync()
        {
            try
            {
                var usuarios = await _repository.GetAllAsync();
                return usuarios.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios");
                return new List<UsuarioResponseDTO>();
            }
        }

        public async Task<UsuarioResponseDTO?> GetByIdAsync(string id)
        {
            try
            {
                var usuario = await _repository.GetByIdAsync(id);
                return usuario != null ? MapToDTO(usuario) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario con ID {Id}", id);
                return null;
            }
        }

        public async Task<UsuarioResponseDTO?> GetByCorreoAsync(string correo)
        {
            try
            {
                var usuario = await _repository.GetByCorreoAsync(correo);
                return usuario != null ? MapToDTO(usuario) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario con correo {Correo}", correo);
                return null;
            }
        }

        public async Task<UsuarioResponseDTO?> UpdateAsync(string id, UsuarioUpdateDTO dto)
        {
            try
            {
                var usuario = await _repository.GetByIdAsync(id);
                if (usuario == null) return null;

                usuario.Nombre = dto.Nombre ?? usuario.Nombre;
                usuario.Apellido = dto.Apellido ?? usuario.Apellido;
                usuario.Celular = dto.Celular ?? usuario.Celular;

                var updated = await _repository.UpdateAsync(usuario);
                return MapToDTO(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario con ID {Id}", id);
                return null;
            }
        }

        private UsuarioResponseDTO MapToDTO(Usuario usuario)
        {
            return new UsuarioResponseDTO
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Correo = usuario.Correo,
                Celular = usuario.Celular
            };
        }
    }
}