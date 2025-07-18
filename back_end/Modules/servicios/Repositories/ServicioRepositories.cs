using back_end.Core.Data;
using back_end.Modules.servicios.Models;
using back_end.Modules.Item.Models;
using Microsoft.EntityFrameworkCore;

namespace back_end.Modules.servicios.Repositories
{    
    public interface IServicioRepository
    {
        Task<List<Servicio>> GetAllAsync();
        Task<List<Servicio>> SearchServiciosAsync(string searchTerm);
        Task<Servicio?> GetByIdAsync(string id);
        
        Task<Servicio?> CreateAsync(Servicio servicio);
        Task<Servicio?> UpdateAsync(Servicio servicio);
        Task<bool> DeleteAsync(Servicio servicio);
        
        Task<DetalleServicio?> GetDetalleServicioByIdAsync(string id);
        Task<List<DetalleServicio>> GetDetalleServiciosByServicioIdAsync(string servicioId);
        Task<DetalleServicio?> AddDetalleServicioAsync(DetalleServicio detalle);
        Task<DetalleServicio?> UpdateDetalleServicioAsync(DetalleServicio detalle);
        Task<bool> RemoveDetalleServicioAsync(DetalleServicio detalle);
        Task<bool> RemoveMultipleDetalleServiciosAsync(IEnumerable<DetalleServicio> detalles);
        
        // Métodos optimizados
        Task<(bool esValido, string mensaje)> ValidarStockServicioAsync(string servicioId);
        Task<List<string>> GetItemsIdsFromServicioAsync(string servicioId);
        Task<(bool esValido, string mensaje, back_end.Modules.Item.Models.Item? item, int stockDisponibleActual)> ValidarStockParaDetalleAsync(string itemId, double? cantidad);
    }    public class ServicioRepository : IServicioRepository
    {
        private readonly DbEventusContext _context;

        public ServicioRepository(DbEventusContext context)
        {
            _context = context;
        }        
        // Método para obtener todos los servicios
        public async Task<List<Servicio>> GetAllAsync()
        {
            return await _context.Servicios
                .Include(s => s.DetalleServicios)
                    .ThenInclude(ds => ds.Inventario)
                .OrderByDescending(s => s.Id) // agregado
                .ToListAsync();
        }
        
        // Método para buscar servicios
        public async Task<List<Servicio>> SearchServiciosAsync(string searchTerm)
        {
            return await _context.Servicios
                .Include(s => s.DetalleServicios)
                    .ThenInclude(ds => ds.Inventario)
                .Where(s => s.Nombre != null && s.Nombre.Contains(searchTerm) || 
                           (s.Descripcion != null && s.Descripcion.Contains(searchTerm)))
                .ToListAsync();
        }
        public async Task<Servicio?> GetByIdAsync(string id)
        {
            return await _context.Servicios
                .Include(s => s.DetalleServicios)
                    .ThenInclude(ds => ds.Inventario)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Servicio?> CreateAsync(Servicio servicio)
        {
            _context.Servicios.Add(servicio);
            await _context.SaveChangesAsync();
            return servicio;
        }

        public async Task<Servicio?> UpdateAsync(Servicio servicio)
        {
            _context.Entry(servicio).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return servicio;
        }

        public async Task<bool> DeleteAsync(Servicio servicio)
        {
            try
            {
                var detalles = await _context.DetalleServicios
                    .Where(ds => ds.ServicioId == servicio.Id)
                    .ToListAsync();
                
                _context.DetalleServicios.RemoveRange(detalles);

                _context.Servicios.Remove(servicio);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<DetalleServicio?> GetDetalleServicioByIdAsync(string id)
        {
            return await _context.DetalleServicios
                .Include(ds => ds.Inventario)
                .Include(ds => ds.Servicio)
                .FirstOrDefaultAsync(ds => ds.Id == id);
        }

        public async Task<List<DetalleServicio>> GetDetalleServiciosByServicioIdAsync(string servicioId)
        {
            return await _context.DetalleServicios
                .Include(ds => ds.Inventario)
                .Where(ds => ds.ServicioId == servicioId)
                .ToListAsync();
        }

        public async Task<DetalleServicio?> AddDetalleServicioAsync(DetalleServicio detalle)
        {
            _context.DetalleServicios.Add(detalle);
            await _context.SaveChangesAsync();
            return detalle;
        }

        public async Task<DetalleServicio?> UpdateDetalleServicioAsync(DetalleServicio detalle)
        {
            _context.Entry(detalle).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return detalle;
        }

        public async Task<bool> RemoveDetalleServicioAsync(DetalleServicio detalle)
        {
            _context.DetalleServicios.Remove(detalle);
            return await _context.SaveChangesAsync() > 0;
        }
        
        public async Task<bool> RemoveMultipleDetalleServiciosAsync(IEnumerable<DetalleServicio> detalles)
        {
            _context.DetalleServicios.RemoveRange(detalles);
            return await _context.SaveChangesAsync() > 0;
        }
        
        // Métodos optimizados para validación de stock
        public async Task<(bool esValido, string mensaje)> ValidarStockServicioAsync(string servicioId)
        {
            try
            {
                // Una sola consulta para obtener toda la información necesaria
                var servicioInfo = await _context.Servicios
                    .Where(s => s.Id == servicioId)
                    .Select(s => new
                    {
                        s.Id,
                        DetalleServicios = s.DetalleServicios.Select(ds => new
                        {
                            ds.InventarioId,
                            ds.Cantidad,
                            Item = new
                            {
                                ds.Inventario!.Id,
                                ds.Inventario.Nombre,
                                ds.Inventario.Stock,
                                ds.Inventario.StockDisponible
                            }
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (servicioInfo == null)
                {
                    return (false, "El servicio especificado no existe");
                }

                if (!servicioInfo.DetalleServicios.Any())
                {
                    return (true, "Servicio sin items, válido para usar");
                }

                var itemsInsuficientes = new List<string>();

                foreach (var detalle in servicioInfo.DetalleServicios)
                {
                    if (!string.IsNullOrEmpty(detalle.InventarioId))
                    {
                        var cantidadRequerida = detalle.Cantidad ?? 0;
                        var stockDisponible = detalle.Item.StockDisponible;
                        
                        if (stockDisponible < cantidadRequerida)
                        {
                            itemsInsuficientes.Add($"'{detalle.Item.Nombre}' (requerido: {cantidadRequerida}, disponible: {stockDisponible})");
                        }
                    }
                }

                if (itemsInsuficientes.Any())
                {
                    var mensaje = $"Stock insuficiente para los siguientes items: {string.Join(", ", itemsInsuficientes)}";
                    return (false, mensaje);
                }

                return (true, "Stock suficiente para todos los items del servicio");
            }
            catch (Exception)
            {
                return (false, "Error al validar stock del servicio");
            }
        }

        public async Task<List<string>> GetItemsIdsFromServicioAsync(string servicioId)
        {
            try
            {
                return await _context.DetalleServicios
                    .Where(ds => ds.ServicioId == servicioId && !string.IsNullOrEmpty(ds.InventarioId))
                    .Select(ds => ds.InventarioId!)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        public async Task<(bool esValido, string mensaje, back_end.Modules.Item.Models.Item? item, int stockDisponibleActual)> ValidarStockParaDetalleAsync(string itemId, double? cantidad)
        {
            try
            {
                var item = await _context.Items
                    .FirstOrDefaultAsync(i => i.Id == itemId);

                if (item == null)
                {
                    return (false, "El item especificado no existe", null, 0);
                }

                var stockDisponible = item.StockDisponible;
                var cantidadRequerida = cantidad ?? 0;

                if (stockDisponible < cantidadRequerida)
                {
                    return (false, $"Stock insuficiente. Disponible: {stockDisponible}, Requerido: {cantidadRequerida}", item, stockDisponible);
                }

                return (true, "Stock suficiente", item, stockDisponible);
            }
            catch (Exception)
            {
                return (false, "Error al validar stock", null, 0);
            }
        }
    }
}