using back_end.Core.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.Core.Utils
{
    public class CounterPersistence
    {
        private readonly DbEventusContext _context;

        public CounterPersistence(DbEventusContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Inicializa los contadores al arrancar la aplicación
        /// </summary>
        public async Task InitializeCounters()
        {
            var counters = new Dictionary<string, int>();
              // Cargar los contadores máximos de cada tabla
            // Usuario - UAO (primera U, media A, última O)
            var maxUsuarioId = await GetMaxNumericIdPart(_context.Usuarios, "UAO");
            if (maxUsuarioId > 0)
                counters[$"Usuario_{DateTime.Now.Year}"] = maxUsuarioId;
            
            // Cliente - CLE (primera C, media L, última E)
            var maxClienteId = await GetMaxNumericIdPart(_context.Clientes, "CLE");
            if (maxClienteId > 0)
                counters[$"Cliente_{DateTime.Now.Year}"] = maxClienteId;
            
            // Organizador - ORR (primera O, media R, última R)
            var maxOrganizadorId = await GetMaxNumericIdPart(_context.Organizadors, "ORR");
            if (maxOrganizadorId > 0)
                counters[$"Organizador_{DateTime.Now.Year}"] = maxOrganizadorId;
            
            // Reserva - REA (primera R, media E, última A)
            var maxReservaId = await GetMaxNumericIdPart(_context.Reservas, "REA");
            if (maxReservaId > 0)
                counters[$"Reserva_{DateTime.Now.Year}"] = maxReservaId;
            
            // Pago - PGO (primera P, media G, última O)
            var maxPagoId = await GetMaxNumericIdPart(_context.Pagos, "PGO");
            if (maxPagoId > 0)
                counters[$"Pago_{DateTime.Now.Year}"] = maxPagoId;
            
            // TipoPago - TPO (primera T, media P, última O)
            var maxTipoPagoId = await GetMaxNumericIdPart(_context.TipoPagos, "TPO");
            if (maxTipoPagoId > 0)
                counters[$"TipoPago_{DateTime.Now.Year}"] = maxTipoPagoId;

            // Cargar los contadores en el IdGenerator
            IdGenerator.LoadCountersFromDatabase(counters);
        }

        /// <summary>
        /// Obtiene el valor numérico más alto de un ID existente
        /// </summary>
        private async Task<int> GetMaxNumericIdPart<T>(DbSet<T> dbSet, string prefix) where T : class
        {
            try
            {
                // Obtener todos los IDs de la tabla como strings
                var allIds = await dbSet.Select(e => EF.Property<string>(e, "Id")).ToListAsync();
                
                int maxId = 0;
                foreach (var id in allIds)
                {
                    if (string.IsNullOrEmpty(id))
                        continue;
                        
                    // Solo procesar IDs que comiencen con el prefijo
                    if (id.StartsWith(prefix))
                    {
                        // Intentar extraer la parte numérica (entre prefijo y año)
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
                return 0; // En caso de error, empezar desde 0
            }
        }
    }
}