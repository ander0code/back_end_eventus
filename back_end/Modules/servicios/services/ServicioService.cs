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
        Task<ServicioResponseDTO?> UpdateAsync(string correo, Guid id, ServicioUpdateDTO dto);
        Task<bool> DeleteAsync(string correo, Guid id);
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
                NombreServicio = s.Nombre,
                Descripcion = s.Descripcion,
                PrecioBase = s.PrecioBase,
                TipoEvento = s.Categoria,
                Imagenes = s.Imagenes,
                FechaCreacion = DateTime.UtcNow // Usar valor por defecto ya que no existe en el modelo
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
                Nombre = dto.NombreServicio ?? "Servicio sin nombre",
                Descripcion = dto.Descripcion,
                PrecioBase = dto.PrecioBase,
                Categoria = dto.TipoEvento,
                Imagenes = dto.Imagenes
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
                NombreServicio = creado.Nombre,
                Descripcion = creado.Descripcion,
                PrecioBase = creado.PrecioBase,
                TipoEvento = creado.Categoria,
                Imagenes = creado.Imagenes,
                FechaCreacion = DateTime.UtcNow // Usar valor actual ya que no existe en el modelo
            };
        }

        public async Task<ServicioResponseDTO?> UpdateAsync(string correo, Guid id, ServicioUpdateDTO dto)
        {
            var existente = await _repository.GetByIdAndCorreoAsync(id, correo);
            if (existente == null)
            {
                return null;
            }

            // Actualiza solo si no es null
            existente.Nombre = dto.NombreServicio ?? existente.Nombre;
            existente.Descripcion = dto.Descripcion ?? existente.Descripcion;
            existente.PrecioBase = dto.PrecioBase ?? existente.PrecioBase;
            existente.Categoria = dto.TipoEvento ?? existente.Categoria;
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
                NombreServicio = actualizado.Nombre,
                Descripcion = actualizado.Descripcion,
                PrecioBase = actualizado.PrecioBase,
                TipoEvento = actualizado.Categoria,
                Imagenes = actualizado.Imagenes,
                FechaCreacion = DateTime.UtcNow // Usar valor actual ya que no existe en el modelo
            };
        }

        public async Task<bool> DeleteAsync(string correo, Guid id)
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
