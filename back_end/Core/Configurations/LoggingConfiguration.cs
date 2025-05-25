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
                
                loggingBuilder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
                loggingBuilder.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
                loggingBuilder.AddFilter("System", LogLevel.Warning);
                

                loggingBuilder.AddFilter("back_end", LogLevel.Information);
            });

            return services;
        }
    }
}