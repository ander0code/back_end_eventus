using back_end.Modules.organizador.DTOs;
using back_end.Modules.organizador.Models;
using back_end.Modules.organizador.Repositories;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace back_end.Modules.organizador.services
{
    public interface IOrganizadorService
    {
        Task<List<OrganizadorResponseDTO>> GetAllAsync();
        Task<OrganizadorResponseDTO?> GetByIdAsync(string id);
        Task<OrganizadorResponseDTO?> GetByUsuarioIdAsync(string usuarioId);
        Task<OrganizadorResponseDTO?> CreateAsync(OrganizadorCreateDTO dto);
        Task<OrganizadorResponseDTO?> UpdateAsync(string id, OrganizadorUpdateDTO dto);
        Task<bool> DeleteAsync(string id);
    }

    public class OrganizadorService : IOrganizadorService
    {
        private readonly IOrganizadorRepository _repository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ILogger<OrganizadorService> _logger;

        public OrganizadorService(
            IOrganizadorRepository repository,
            IUsuarioRepository usuarioRepository,
            ILogger<OrganizadorService> logger)
        {
            _repository = repository;
            _usuarioRepository = usuarioRepository;
            _logger = logger;
        }

        public async Task<List<OrganizadorResponseDTO>> GetAllAsync()
        {
            try
            {
                var organizadores = await _repository.GetAllAsync();
                return organizadores.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los organizadores");
                return new List<OrganizadorResponseDTO>();
            }
        }

        public async Task<OrganizadorResponseDTO?> GetByIdAsync(string id)
        {
            try
            {
                var organizador = await _repository.GetByIdAsync(id);
                return organizador != null ? MapToDTO(organizador) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener organizador con ID {Id}", id);
                return null;
            }
        }

        public async Task<OrganizadorResponseDTO?> GetByUsuarioIdAsync(string usuarioId)
        {
            try
            {
                var organizador = await _repository.GetByUsuarioIdAsync(usuarioId);
                return organizador != null ? MapToDTO(organizador) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener organizador por usuario ID {UsuarioId}", usuarioId);
                return null;
            }
        }

        public async Task<OrganizadorResponseDTO?> CreateAsync(OrganizadorCreateDTO dto)
        {
            try
            {
                // Primero crear el usuario
                var usuario = new Usuario
                {
                    Id = Guid.NewGuid().ToString(),
                    Nombre = dto.Nombre,
                    Apellido = dto.Apellido,
                    Correo = dto.Correo,
                    Celular = dto.Celular
                };

                await _usuarioRepository.UpdateAsync(usuario); // Esto lo guarda si no existe

                // Luego crear el organizador asociado
                var organizador = new Organizador
                {
                    Id = Guid.NewGuid().ToString(),
                    NombreNegocio = dto.NombreNegocio,
                    Contrasena = HashPassword(dto.Contrasena),
                    UsuarioId = usuario.Id
                };

                var creado = await _repository.CreateAsync(organizador);
                creado.Usuario = usuario; // Para el mapeo
                return MapToDTO(creado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear organizador {Correo}", dto.Correo);
                return null;
            }
        }

        public async Task<OrganizadorResponseDTO?> UpdateAsync(string id, OrganizadorUpdateDTO dto)
        {
            try
            {
                var organizador = await _repository.GetByIdAsync(id);
                if (organizador == null) return null;

                organizador.NombreNegocio = dto.NombreNegocio ?? organizador.NombreNegocio;
                
                if (!string.IsNullOrEmpty(dto.Contrasena))
                {
                    organizador.Contrasena = HashPassword(dto.Contrasena);
                }

                var updated = await _repository.UpdateAsync(organizador);
                return MapToDTO(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar organizador con ID {Id}", id);
                return null;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var organizador = await _repository.GetByIdAsync(id);
                if (organizador == null) return false;

                return await _repository.DeleteAsync(organizador);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar organizador con ID {Id}", id);
                return false;
            }
        }

        private OrganizadorResponseDTO MapToDTO(Organizador organizador)
        {
            string nombreCompleto = "";
            if (organizador.Usuario != null)
            {
                nombreCompleto = $"{organizador.Usuario.Nombre} {organizador.Usuario.Apellido}".Trim();
            }

            return new OrganizadorResponseDTO
            {
                Id = organizador.Id,
                NombreNegocio = organizador.NombreNegocio,
                UsuarioId = organizador.UsuarioId,
                NombreCompleto = nombreCompleto,
                Correo = organizador.Usuario?.Correo,
                Celular = organizador.Usuario?.Celular
            };
        }

        private string HashPassword(string? password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            // Usa BCrypt para un hash seguro
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }
    }
}