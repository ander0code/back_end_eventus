using Microsoft.OpenApi.Models;

namespace back_end.Core.Configurations
{
    public static class SwaggerConfiguration
    {        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { 
                    Title = "Eventus API", 
                    Version = "v1",
                    Description = "API para gestionar eventos, reservas, servicios y pagos",
                    Contact = new OpenApiContact
                    {
                        Name = "Eventus Team",
                        Email = "info@eventus.com",
                    }
                });
                
                // Habilitar la lectura de comentarios XML para la documentación de Swagger
                var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
                foreach (var xmlFile in xmlFiles)
                {
                    try
                    {
                        c.IncludeXmlComments(xmlFile);
                    }
                    catch
                    {
                        // Ignorar archivos XML que no contienen documentación válida
                    }
                }
                
                // Definir el esquema de seguridad JWT
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Ingrese 'Bearer' [espacio] y luego su token JWT en el campo de texto"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            return app;
        }
    }
}