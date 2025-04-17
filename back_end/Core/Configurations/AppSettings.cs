using Microsoft.Extensions.Configuration;

namespace back_end.Core.Configurations
{
    /// <summary>
    /// Clase que centraliza el acceso a todas las configuraciones sensibles de la aplicación.
    /// Esta clase proporciona una capa de abstracción para acceder a las configuraciones 
    /// independientemente de dónde estén almacenadas (variables de entorno, User Secrets, 
    /// archivos de configuración, etc.).
    /// </summary>
    public class AppSettings
    {
        private readonly IConfiguration _configuration;

        public AppSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #region Database

        public string DatabaseConnection => GetRequiredValue("ConnectionStrings:DefaultConnection");

        #endregion

        #region JWT

        public string JwtKey => GetRequiredValue("Jwt:Key");
        public string JwtIssuer => GetRequiredValue("Jwt:Issuer");
        public string JwtAudience => GetRequiredValue("Jwt:Audience");

        #endregion

        #region Helpers

        /// <summary>
        /// Obtiene un valor de configuración y lanza una excepción si no está presente.
        /// </summary>
        private string GetRequiredValue(string key)
        {
            var value = _configuration[key];
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"La configuración '{key}' no está definida. Por favor, configúrela en las variables de entorno o User Secrets.");
            }
            return value;
        }

        /// <summary>
        /// Obtiene un valor de configuración opcional o devuelve un valor por defecto.
        /// </summary>
        private string GetOptionalValue(string key, string defaultValue = "")
        {
            return _configuration[key] ?? defaultValue;
        }

        #endregion
    }

    // Extensión para registrar AppSettings en la inyección de dependencias
    public static class AppSettingsExtensions
    {
        public static IServiceCollection AddAppSettings(this IServiceCollection services)
        {
            return services.AddSingleton<AppSettings>();
        }
    }
}