using back_end.Core.Data;
using back_end.Core.Configurations;
using back_end.Middleware;
using back_end.Modules.Auth.Repositories;
using back_end.Modules.Auth.Services;
using back_end.Modules.inventario.Repositories;
using back_end.Modules.inventario.services;
using back_end.Modules.reservas.Services;
using back_end.Modules.reservas.Repositories;
using back_end.Modules.servicios.Services;
using back_end.Modules.servicios.Repositories;
using back_end.Modules.clientes.Repositories;
using back_end.Modules.clientes.Services;
using back_end.Modules.usuarios.services;
using back_end.Modules.usuarios.Repositories;
using back_end.Modules.dashboard.services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true) 
    .AddEnvironmentVariables() 
    .AddUserSecrets<Program>(optional: true); 


builder.Services.AddAppSettings();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerConfiguration();

builder.Services.AddLoggingConfiguration();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});


builder.Services.AddDbContext<DbEventusContext>((serviceProvider, options) => {
    var appSettings = serviceProvider.GetRequiredService<AppSettings>();
    options.UseSqlServer(appSettings.DatabaseConnection);
});

// Agregar configuración JWT usando AppSettings
builder.Services.AddJwtConfiguration(builder.Configuration);


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
builder.Services.AddScoped<IInventarioRepository, InventarioRepository>();
builder.Services.AddScoped<IInventarioService, InventarioService>();

// Cliente Module
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();

// Dashboard Module
builder.Services.AddScoped<IDashboardService, DashboardService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerConfiguration();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowSpecificOrigin");

app.UseExampleMiddleware();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();