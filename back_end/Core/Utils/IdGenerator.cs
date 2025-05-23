using System;
using System.Collections.Generic;

namespace back_end.Core.Utils
{
    public static class IdGenerator
    {
        // Un diccionario para mantener el conteo por cada tipo de entidad
        private static readonly Dictionary<string, int> _contadores = new Dictionary<string, int>();
        private static readonly object _lock = new object();

        /// <summary>
        /// Genera un ID personalizado basado en el nombre de la tabla/entidad
        /// </summary>
        /// <param name="tableName">Nombre de la tabla/entidad</param>
        /// <returns>ID en formato XXX00001-YYYY</returns>
        public static string GenerateId(string tableName)
        {
            lock (_lock) // Para evitar problemas de concurrencia
            {
                // Obtener el prefijo de 3 letras
                string prefix = GetPrefix(tableName);
                
                // Año actual
                int year = DateTime.Now.Year;
                
                // Clave para este año y tabla
                string counterKey = $"{tableName}_{year}";
                
                // Incrementar o inicializar contador
                if (!_contadores.ContainsKey(counterKey))
                {
                    _contadores[counterKey] = 0;
                }
                _contadores[counterKey]++;
                
                // Formatear el contador con ceros a la izquierda (5 dígitos)
                string formattedCounter = _contadores[counterKey].ToString("D5");
                
                // Retornar el ID completo
                return $"{prefix}{formattedCounter}-{year}";
            }
        }        /// <summary>
        /// Obtiene un prefijo de 3 letras a partir del nombre de la tabla
        /// </summary>
        private static string GetPrefix(string tableName)
        {
            tableName = tableName.Trim();
            
            if (string.IsNullOrEmpty(tableName))
                return "GEN"; // Prefijo genérico si el nombre está vacío
            
            // Prefijos específicos para entidades conocidas
            switch (tableName.ToLower())
            {
                case "usuario":
                    return "UAO"; // primera (U), media (A), última (O)
                case "cliente":
                    return "CLE"; // primera (C), media (L), última (E)
                case "organizador":
                    return "ORR"; // primera (O), media (R), última (R)
                case "reserva":
                    return "REA"; // primera (R), media (E), última (A)
                case "pago":
                    return "PGO"; // primera (P), media (G), última (O)
                case "tipopago":
                    return "TPO"; // primera (T), media (P), última (O)
                case "item":
                    return "ITM"; // primera (I), media (T), última (M)
            }
                
            // Para otras entidades, mantener la lógica original
            if (tableName.Length >= 3)
            {
                // Primera, media y última letra
                char first = tableName[0];
                char middle = tableName[tableName.Length / 2];
                char last = tableName[tableName.Length - 1];
                
                return $"{first}{middle}{last}".ToUpper();
            }
            // Si el nombre tiene 2 caracteres
            else if (tableName.Length == 2)
            {
                return $"{tableName[0]}{tableName[1]}X".ToUpper();
            }
            // Si el nombre tiene 1 carácter
            else
            {
                return $"{tableName[0]}XX".ToUpper();
            }
        }

        /// <summary>
        /// Restablece los contadores para pruebas
        /// </summary>
        public static void ResetCounters()
        {
            lock (_lock)
            {
                _contadores.Clear();
            }
        }

        /// <summary>
        /// Carga contadores existentes desde la base de datos
        /// </summary>
        public static void LoadCountersFromDatabase(Dictionary<string, int> counters)
        {
            lock (_lock)
            {
                foreach (var pair in counters)
                {
                    _contadores[pair.Key] = pair.Value;
                }
            }
        }
    }
}