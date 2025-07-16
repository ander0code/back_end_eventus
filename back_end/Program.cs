using back_end.Core.Data;
using back_end.Core.Configurations;
using back_end.Middleware;
using back_end.Modules.Auth.Repositories;
using back_end.Modules.Auth.Services;
using back_end.Modules.Item.Repositories;
using back_end.Modules.Item.Services;
using back_end.Modules.reservas.Services;
using back_end.Modules.reservas.Repositories;
using back_end.Modules.servicios.Services;
using back_end.Modules.servicios.Repositories;
using back_end.Modules.clientes.Repositories;
using back_end.Modules.clientes.Services;
using back_end.Modules.organizador.services;
using back_end.Modules.organizador.Repositories;
using back_end.Modules.pagos.Repositories;
using back_end.Core.Utils;
using back_end.Modules.pagos.services;
using back_end.Modules.dashboard.services;
using back_end.Modules.reportes.Repositories;
using back_end.Modules.reportes.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>(optional: true);


builder.Services.AddAppSettings();

builder.Services.AddControllers(options =>
{
    // Esto ayuda a resolver ambigüedades en los métodos HTTP
    options.SuppressAsyncSuffixInActionNames = false;
});
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

// Configuración para usarlo con ngrok
/*
builder.Services.AddDbContext<DbEventusContext>((serviceProvider, options) =>
{
    var appSettings = serviceProvider.GetRequiredService<AppSettings>();
    options.UseSqlServer(appSettings.DatabaseConnection);
});
*/

// Configuración para usarlo de manera tradicional
builder.Services.AddDbContext<DbEventusContext>();


// Agregar configuración JWT usando AppSettings
builder.Services.AddJwtConfiguration(builder.Configuration);


// Auth Module
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Organizador Module
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IOrganizadorService, OrganizadorService>();
builder.Services.AddScoped<IOrganizadorRepository, OrganizadorRepository>();

// Reservas Module
builder.Services.AddScoped<IReservaRepository, ReservaRepository>();
builder.Services.AddScoped<IReservaService, ReservaService>();
builder.Services.AddScoped<ITipoEventoService, TipoEventoService>();

// Servicios Module
builder.Services.AddScoped<IServicioRepository, ServicioRepository>();
builder.Services.AddScoped<IServicioService, ServicioService>();

// Item Module
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IItemService, ItemService>();

// Cliente Module
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();

// Pagos Module
builder.Services.AddScoped<IPagosRepository, PagosRepository>();
builder.Services.AddScoped<IPagosService, PagosService>();
builder.Services.AddScoped<back_end.Modules.pagos.Services.ITipoPagoService, back_end.Modules.pagos.Services.TipoPagoService>();

// Dashboard Module
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Reportes Module
builder.Services.AddScoped<IClientesReporteRepository, ClientesReporteRepository>();
builder.Services.AddScoped<IInventarioReporteRepository, InventarioReporteRepository>();
builder.Services.AddScoped<IPagosReporteRepository, PagosReporteRepository>();
builder.Services.AddScoped<IReservasReporteRepository, ReservasReporteRepository>();
builder.Services.AddScoped<IServiciosReporteRepository, ServiciosReporteRepository>();
builder.Services.AddScoped<IResumenEjecutivoRepository, ResumenEjecutivoRepository>();
builder.Services.AddScoped<IReporteService, ReporteService>();

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

// Inicializar los contadores para el generador de IDs
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DbEventusContext>();
        var counterPersistence = new CounterPersistence(context);
        // Inicializar los contadores de forma sincrónica para asegurar que estén listos antes de continuar
        counterPersistence.InitializeCounters().GetAwaiter().GetResult();
        
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Contadores de ID inicializados correctamente");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al inicializar los contadores de ID");
    }
}

app.Run();