using back_end.Core.Data;
using back_end.Modules.inventario.DTOs;
using back_end.Modules.inventario.Models;
using back_end.Modules.usuarios.Repositories;
using Microsoft.EntityFrameworkCore;

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
    }

    public class InventarioService : IInventarioService
    {
        private readonly DbEventusContext _context;
        private readonly IUsuarioRepository _usuarioRepository;

        public InventarioService(DbEventusContext context, IUsuarioRepository usuarioRepository)
        {
            _context = context;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<List<InventarioResponseDTO>> GetAllAsync()
        {
            var inventarios = await _context.Inventarios
                .Include(i => i.Usuario)
                .ToListAsync();
                
            return inventarios.Select(MapToDTO).ToList();
        }

        public async Task<InventarioResponseDTO?> GetByIdAsync(Guid id)
        {
            var inventario = await _context.Inventarios
                .Include(i => i.Usuario)
                .FirstOrDefaultAsync(i => i.Id == id);
                
            return inventario == null ? null : MapToDTO(inventario);
        }

        public async Task<List<InventarioResponseDTO>> GetByUsuarioIdAsync(Guid usuarioId)
        {
            var inventarios = await _context.Inventarios
                .Include(i => i.Usuario)
                .Where(i => i.UsuarioId == usuarioId)
                .ToListAsync();
                
            return inventarios.Select(MapToDTO).ToList();
        }
        
        public async Task<List<InventarioResponseDTO>> GetByCorreoAsync(string correo)
        {
            var inventarios = await _context.Inventarios
                .Include(i => i.Usuario)
                .Where(i => i.Usuario.CorreoElectronico == correo)
                .ToListAsync();
                
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
            
            _context.Inventarios.Add(inventario);
            await _context.SaveChangesAsync();
            
            // Recargar con el usuario incluido para el mapeo
            var creado = await _context.Inventarios
                .Include(i => i.Usuario)
                .FirstOrDefaultAsync(i => i.Id == inventario.Id);
                
            return creado == null ? null : MapToDTO(creado);
        }

        public async Task<InventarioResponseDTO?> UpdateAsync(Guid id, string correo, InventarioUpdateDTO dto)
        {
            var inventario = await _context.Inventarios
                .Include(i => i.Usuario)
                .FirstOrDefaultAsync(i => i.Id == id && i.Usuario.CorreoElectronico == correo);
                
            if (inventario == null) return null;
            
            // Actualizar solo las propiedades que no son nulas
            if (dto.Nombre != null) inventario.Nombre = dto.Nombre;
            if (dto.Descripcion != null) inventario.Descripcion = dto.Descripcion;
            if (dto.Stock != null) inventario.Stock = dto.Stock;
            if (dto.Categoria != null) inventario.Categoria = dto.Categoria;
            
            await _context.SaveChangesAsync();
            return MapToDTO(inventario);
        }

        public async Task<bool> DeleteAsync(Guid id, string correo)
        {
            var inventario = await _context.Inventarios
                .Include(i => i.Usuario)
                .FirstOrDefaultAsync(i => i.Id == id && i.Usuario.CorreoElectronico == correo);
                
            if (inventario == null) return false;
            
            _context.Inventarios.Remove(inventario);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ActualizarStockAsync(Guid id, string correo, int cantidad)
        {
            var inventario = await _context.Inventarios
                .Include(i => i.Usuario)
                .FirstOrDefaultAsync(i => i.Id == id && i.Usuario.CorreoElectronico == correo);
                
            if (inventario == null) return false;
            
            inventario.Stock = cantidad;
            return await _context.SaveChangesAsync() > 0;
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