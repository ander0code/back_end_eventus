using back_end.Modules.inventario.DTOs;
using back_end.Modules.inventario.Models;
using back_end.Modules.inventario.Repositories;
using back_end.Modules.usuarios.Repositories;

namespace back_end.Modules.inventario.services
{
    public interface IInventarioService
    {
        Task<List<InventarioResponseDTO>> GetAllAsync();
        Task<InventarioResponseDTO?> GetByIdAsync(Guid id);
        Task<List<InventarioResponseDTO>> GetByUsuarioIdAsync(Guid usuarioId);
        Task<List<InventarioResponseDTO>> GetByCorreoAsync(string correo);
        Task<InventarioResponseDTO?> CreateAsync(string correo, InventarioCreateDTO dto);
        Task<InventarioResponseDTO?> UpdateAsync(Guid id, string correo, InventarioUpdateDTO dto);
        Task<bool> DeleteAsync(Guid id, string correo);
        Task<bool> ActualizarStockAsync(Guid id, string correo, int cantidad);
        Task<List<InventarioResponseDTO>> SearchByNameOrCategoryAsync(string correo, string searchTerm);
        Task<List<InventarioResponseDTO>> GetByStockBelowMinAsync(string correo, int minStock);
    }

    public class InventarioService : IInventarioService
    {
        private readonly IInventarioRepository _inventarioRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public InventarioService(IInventarioRepository inventarioRepository, IUsuarioRepository usuarioRepository)
        {
            _inventarioRepository = inventarioRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<List<InventarioResponseDTO>> GetAllAsync()
        {
            var inventarios = await _inventarioRepository.GetAllAsync();
            return inventarios.Select(MapToDTO).ToList();
        }

        public async Task<InventarioResponseDTO?> GetByIdAsync(Guid id)
        {
            var inventario = await _inventarioRepository.GetByIdAsync(id);
            return inventario == null ? null : MapToDTO(inventario);
        }

        public async Task<List<InventarioResponseDTO>> GetByUsuarioIdAsync(Guid usuarioId)
        {
            var inventarios = await _inventarioRepository.GetByUsuarioIdAsync(usuarioId);
            return inventarios.Select(MapToDTO).ToList();
        }
        
        public async Task<List<InventarioResponseDTO>> GetByCorreoAsync(string correo)
        {
            var inventarios = await _inventarioRepository.GetByCorreoAsync(correo);
            return inventarios.Select(MapToDTO).ToList();
        }

        public async Task<InventarioResponseDTO?> CreateAsync(string correo, InventarioCreateDTO dto)
        {
            var usuario = await _usuarioRepository.GetByCorreoAsync(correo);
            if (usuario == null) return null;
            
            var inventario = new Inventario
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Stock = dto.Stock,
                Categoria = dto.Categoria,
                FechaRegistro = DateTime.Now,
                UsuarioId = usuario.Id
            };
            
            var creado = await _inventarioRepository.CreateAsync(inventario);
            return creado == null ? null : MapToDTO(creado);
        }

        public async Task<InventarioResponseDTO?> UpdateAsync(Guid id, string correo, InventarioUpdateDTO dto)
        {

            var inventarios = await _inventarioRepository.GetByCorreoAsync(correo);
            var inventario = inventarios.FirstOrDefault(i => i.Id == id);
            
            if (inventario == null) return null;

            if (dto.Nombre != null) inventario.Nombre = dto.Nombre;
            if (dto.Descripcion != null) inventario.Descripcion = dto.Descripcion;
            if (dto.Stock != null) inventario.Stock = dto.Stock;
            if (dto.Categoria != null) inventario.Categoria = dto.Categoria;
            
            var actualizado = await _inventarioRepository.UpdateAsync(inventario);
            return actualizado == null ? null : MapToDTO(actualizado);
        }

        public async Task<bool> DeleteAsync(Guid id, string correo)
        {
            try
            {

                var inventarios = await _inventarioRepository.GetByCorreoAsync(correo);
                var inventario = inventarios.FirstOrDefault(i => i.Id == id);
                
                if (inventario == null) return false;
                
                return await _inventarioRepository.DeleteAsync(inventario);
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error al eliminar inventario: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ActualizarStockAsync(Guid id, string correo, int cantidad)
        {

            var inventarios = await _inventarioRepository.GetByCorreoAsync(correo);
            var inventario = inventarios.FirstOrDefault(i => i.Id == id);
            
            if (inventario == null) return false;
            
            return await _inventarioRepository.ActualizarStockAsync(id, cantidad);
        }
        
        public async Task<List<InventarioResponseDTO>> SearchByNameOrCategoryAsync(string correo, string searchTerm)
        {

            var allResults = await _inventarioRepository.SearchByNameOrCategoryAsync(searchTerm);

            var usuario = await _usuarioRepository.GetByCorreoAsync(correo);
            if (usuario == null) return new List<InventarioResponseDTO>();
            
            var filtered = allResults.Where(i => i.UsuarioId == usuario.Id).ToList();
            return filtered.Select(MapToDTO).ToList();
        }
        
        public async Task<List<InventarioResponseDTO>> GetByStockBelowMinAsync(string correo, int minStock)
        {

            var allResults = await _inventarioRepository.GetByStockBelowMinAsync(minStock);

            var usuario = await _usuarioRepository.GetByCorreoAsync(correo);
            if (usuario == null) return new List<InventarioResponseDTO>();
            
            var filtered = allResults.Where(i => i.UsuarioId == usuario.Id).ToList();
            return filtered.Select(MapToDTO).ToList();
        }
        
        private InventarioResponseDTO MapToDTO(Inventario inventario)
        {
            return new InventarioResponseDTO
            {
                Id = inventario.Id,
                Nombre = inventario.Nombre,
                Descripcion = inventario.Descripcion,
                Stock = inventario.Stock,
                Categoria = inventario.Categoria,
                FechaRegistro = inventario.FechaRegistro,
                NombreUsuario = inventario.Usuario?.Nombre
            };
        }
    }
}