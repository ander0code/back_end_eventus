namespace back_end.Core.Configurations
{

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


        private string GetRequiredValue(string key)
        {
            var value = _configuration[key];
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"La configuración '{key}' no está definida. Por favor, configúrela en las variables de entorno o User Secrets.");
            }
            return value;
        }

  
        private string GetOptionalValue(string key, string defaultValue = "")
        {
            return _configuration[key] ?? defaultValue;
        }

        #endregion
    }


    public static class AppSettingsExtensions
    {
        public static IServiceCollection AddAppSettings(this IServiceCollection services)
        {
            return services.AddSingleton<AppSettings>();
        }
    }
}