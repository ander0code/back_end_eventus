using System;
using System.Collections.Generic;

namespace back_end.Core.Utils
{
    public static class IdGenerator
    {
        // Un diccionario para mantener el conteo por cada tipo de entidad
        private static readonly Dictionary<string, int> _contadores = new Dictionary<string, int>();
        private static readonly object _lock = new object();

        public static string GenerateId(string tableName)
        {
            lock (_lock) // Para evitar problemas de concurrencia
            {

                string prefix = GetPrefix(tableName);
                
                int year = DateTime.Now.Year;
                
                string counterKey = $"{tableName}_{year}";
                
                if (!_contadores.ContainsKey(counterKey))
                {
                    _contadores[counterKey] = 0;
                }
                _contadores[counterKey]++;
                
                string formattedCounter = _contadores[counterKey].ToString("D5");

                return $"{prefix}{formattedCounter}-{year}";
            }
        }  

        private static string GetPrefix(string tableName)
        {
            tableName = tableName.Trim();
            
            if (string.IsNullOrEmpty(tableName))
                return "GEN";
            
            switch (tableName.ToLower())
            {
                case "usuario":
                    return "UAO";
                case "cliente":
                    return "CLE";
                case "organizador":
                    return "ORR";
                case "reserva":
                    return "REA";
                case "pago":
                    return "PGO";
                case "tipopago":
                    return "TPO";
                case "item":
                    return "ITM";
                case "detalleservicio":
                    return "DSO";
                case "items":
                    return "ITS";
                case "servicios":
                    return "SVO";
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