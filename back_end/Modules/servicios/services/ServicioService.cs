using back_end.Modules.servicios.DTOs;
using back_end.Modules.servicios.Models;
using back_end.Modules.servicios.Repositories;
using back_end.Modules.usuarios.Repositories;

namespace back_end.Modules.servicios.Services
{
    public interface IServicioService
    {
        Task<List<ServicioResponseDTO>> GetByCorreoAsync(string correo);
        Task<ServicioResponseDTO?> CreateAsync(string correo, ServicioCreateDTO dto);
        Task<ServicioResponseDTO?> UpdateAsync(string correo, int id, ServicioUpdateDTO dto);
        Task<bool> DeleteAsync(string correo, int id);
    }

    public class ServicioService : IServicioService
    {
        private readonly IServicioRepository _repository;
        private readonly IUsuarioRepository _usuarioRepository;

        public ServicioService(IServicioRepository repository, IUsuarioRepository usuarioRepository)
        {
            _repository = repository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<List<ServicioResponseDTO>> GetByCorreoAsync(string correo)
        {
            var servicios = await _repository.GetByCorreoAsync(correo);

            if (servicios == null || !servicios.Any())
            {
                return new List<ServicioResponseDTO>();
            }

            return servicios.Select(s => new ServicioResponseDTO
            {
                Id = s.Id,
                NombreServicio = s.NombreServicio,
                Descripcion = s.Descripcion,
                PrecioBase = s.PrecioBase,
                TipoEvento = s.TipoEvento,
                Imagenes = s.Imagenes,
                FechaCreacion = s.FechaCreacion
            }).ToList();
        }

        public async Task<ServicioResponseDTO?> CreateAsync(string correo, ServicioCreateDTO dto)
        {
            var usuario = await _usuarioRepository.GetByCorreoAsync(correo);
            if (usuario == null)
            {
                return null;
            }

            var nuevo = new Servicio
            {
                UsuarioId = usuario.Id,
                NombreServicio = dto.NombreServicio,
                Descripcion = dto.Descripcion,
                PrecioBase = dto.PrecioBase,
                TipoEvento = dto.TipoEvento,
                Imagenes = dto.Imagenes,
                FechaCreacion = DateTime.UtcNow
            };

            var creado = await _repository.CreateAsync(nuevo);

            // Verificamos que creado no sea null antes de acceder a sus propiedades
            if (creado == null)
            {
                return null; // o manejar el caso de error
            }

            return new ServicioResponseDTO
            {
                Id = creado.Id,
                NombreServicio = creado.NombreServicio,
                Descripcion = creado.Descripcion,
                PrecioBase = creado.PrecioBase,
                TipoEvento = creado.TipoEvento,
                Imagenes = creado.Imagenes,
                FechaCreacion = creado.FechaCreacion
            };
        }

        public async Task<ServicioResponseDTO?> UpdateAsync(string correo, int id, ServicioUpdateDTO dto)
        {
            var existente = await _repository.GetByIdAndCorreoAsync(id, correo);
            if (existente == null)
            {
                return null;
            }

            // Actualiza solo si no es null
            existente.NombreServicio = dto.NombreServicio ?? existente.NombreServicio;
            existente.Descripcion = dto.Descripcion ?? existente.Descripcion;
            existente.PrecioBase = dto.PrecioBase ?? existente.PrecioBase;
            existente.TipoEvento = dto.TipoEvento ?? existente.TipoEvento;
            existente.Imagenes = dto.Imagenes ?? existente.Imagenes;

            var actualizado = await _repository.UpdateAsync(existente);

            // Verificamos que actualizado no sea null antes de acceder a sus propiedades
            if (actualizado == null)
            {
                return null; // o manejar el caso de error
            }

            return new ServicioResponseDTO
            {
                Id = actualizado.Id,
                NombreServicio = actualizado.NombreServicio,
                Descripcion = actualizado.Descripcion,
                PrecioBase = actualizado.PrecioBase,
                TipoEvento = actualizado.TipoEvento,
                Imagenes = actualizado.Imagenes,
                FechaCreacion = actualizado.FechaCreacion
            };
        }

        public async Task<bool> DeleteAsync(string correo, int id)
        {
            var servicio = await _repository.GetByIdAndCorreoAsync(id, correo);
            if (servicio == null)
            {
                return false;
            }

            return await _repository.DeleteAsync(servicio);
        }
    }
}
