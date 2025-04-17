using back_end.Core.Data;
using back_end.Core.Configurations;
using back_end.Middleware;
using back_end.Modules.Auth.Repositories;
using back_end.Modules.Auth.Services;
using back_end.Modules.inventario.services;
using back_end.Modules.reservas.Services;
using back_end.Modules.reservas.Repositories;
using back_end.Modules.servicios.Services;
using back_end.Modules.servicios.Repositories;
using back_end.Modules.usuarios.services;
using back_end.Modules.usuarios.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurar Swagger utilizando la clase de configuración
builder.Services.AddSwaggerConfiguration();

// Configurar Logging
builder.Services.AddLoggingConfiguration();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Add DbContext configuration
builder.Services.AddDbContext<DbEventusContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Agregar configuración JWT
builder.Services.AddJwtConfiguration(builder.Configuration);

// Register services for each module
// Auth Module
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Usuarios Module
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Reservas Module
builder.Services.AddScoped<IReservaRepository, ReservaRepository>();
builder.Services.AddScoped<IReservaService, ReservaService>();

// Servicios Module
builder.Services.AddScoped<IServicioRepository, ServicioRepository>();
builder.Services.AddScoped<IServicioService, ServicioService>();

// Inventario Module
builder.Services.AddScoped<IInventarioService, InventarioService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerConfiguration();
}
else
{
    // Solo aplica HTTPS Redirection en producción
    app.UseHttpsRedirection();
}

// Use CORS middleware
app.UseCors("AllowSpecificOrigin");

// Use custom middleware
app.UseExampleMiddleware();

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controller routes with patterns for each module
app.MapControllers(); // Default route for all controllers

app.Run();