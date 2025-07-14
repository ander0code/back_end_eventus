using back_end.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace back_end.Core.Utils
{
    public class CounterPersistence
    {
        private readonly DbEventusContext _context;

        public CounterPersistence(DbEventusContext context)
        {
            _context = context;
        }

        public async Task InitializeCounters()
        {
            var counters = new Dictionary<string, int>();

            var maxUsuarioId = await GetMaxNumericIdPart(_context.Usuarios, "UAO");
            if (maxUsuarioId > 0)
                counters[$"Usuario_{DateTime.Now.Year}"] = maxUsuarioId;

            var maxClienteId = await GetMaxNumericIdPart(_context.Clientes, "CLE");
            if (maxClienteId > 0)
                counters[$"Cliente_{DateTime.Now.Year}"] = maxClienteId;
            
            var maxOrganizadorId = await GetMaxNumericIdPart(_context.Organizadors, "ORR");
            if (maxOrganizadorId > 0)
                counters[$"Organizador_{DateTime.Now.Year}"] = maxOrganizadorId;

            var maxReservaId = await GetMaxNumericIdPart(_context.Reservas, "REA");
            if (maxReservaId > 0)
                counters[$"Reserva_{DateTime.Now.Year}"] = maxReservaId;
            
            var maxPagoId = await GetMaxNumericIdPart(_context.Pagos, "PGO");
            if (maxPagoId > 0)
                counters[$"Pago_{DateTime.Now.Year}"] = maxPagoId;

            var maxItemId = await GetMaxNumericIdPart(_context.Items, "ITM");
            if (maxItemId > 0)
                counters[$"Item_{DateTime.Now.Year}"] = maxItemId;
            
            var maxTipoPagoId = await GetMaxNumericIdPart(_context.TipoPagos, "TPO");
            if (maxTipoPagoId > 0)
                counters[$"TipoPago_{DateTime.Now.Year}"] = maxTipoPagoId;

                        // AGREGAR ESTAS LÍNEAS
            var maxServicioId = await GetMaxNumericIdPart(_context.Servicios, "SVO");
            if (maxServicioId > 0)
                counters[$"Servicios_{DateTime.Now.Year}"] = maxServicioId;

            var maxDetalleServicioId = await GetMaxNumericIdPart(_context.DetalleServicios, "DSO");
            if (maxDetalleServicioId > 0)
                counters[$"DetalleServicio_{DateTime.Now.Year}"] = maxDetalleServicioId;

            IdGenerator.LoadCountersFromDatabase(counters);
        }

        private async Task<int> GetMaxNumericIdPart<T>(DbSet<T> dbSet, string prefix) where T : class
        {
            try
            {
                var allIds = await dbSet.Select(e => EF.Property<string>(e, "Id")).ToListAsync();
                
                int maxId = 0;
                foreach (var id in allIds)
                {
                    if (string.IsNullOrEmpty(id))
                        continue;
                        

                    if (id.StartsWith(prefix))
                    {
 
                        var numericPart = id.Substring(prefix.Length).Split('-')[0];
                        if (int.TryParse(numericPart, out int currentId) && currentId > maxId)
                        {
                            maxId = currentId;
                        }
                    }
                }
                return maxId;
            }
            catch
            {
                return 0;
            }
        }
    }
}