using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace back_end.Core.Configurations
{
    public static class LoggingConfiguration
    {
        public static IServiceCollection AddLoggingConfiguration(this IServiceCollection services)
        {
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
                
                // Configurar niveles de log específicos para diferentes namespaces
                loggingBuilder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
                loggingBuilder.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
                loggingBuilder.AddFilter("System", LogLevel.Warning);
                
                // Configuración para los logs de la aplicación
                loggingBuilder.AddFilter("back_end", LogLevel.Information);
            });

            return services;
        }
    }
}