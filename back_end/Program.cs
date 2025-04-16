using back_end.Core.Data;
using back_end.Middleware;
using back_end.Modules.Auth.Repositories;
using back_end.Modules.Auth.Services;
using back_end.Modules.inventario.services;
using back_end.Modules.reservas.services;
using back_end.Modules.servicios.services;
using back_end.Modules.usuarios.services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Register services for each module
// Auth Module
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Usuarios Module
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// Reservas Module
builder.Services.AddScoped<IReservaService, ReservaService>();

// Servicios Module
builder.Services.AddScoped<IServicioService, ServicioService>();

// Inventario Module
builder.Services.AddScoped<IInventarioService, InventarioService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS middleware
app.UseCors("AllowSpecificOrigin");

// Use custom middleware
app.UseExampleMiddleware();

app.UseHttpsRedirection();
app.UseAuthorization();

// Map controller routes with patterns for each module
app.MapControllers(); // Default route for all controllers

// Map specific routes for modules
app.MapControllerRoute(
    name: "auth",
    pattern: "api/auth/{controller=Auth}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "usuarios",
    pattern: "api/usuarios/{controller=Usuarios}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "reservas",
    pattern: "api/reservas/{controller=Reservas}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "servicios",
    pattern: "api/servicios/{controller=Servicios}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "inventario",
    pattern: "api/inventario/{controller=Inventario}/{action=Index}/{id?}");

app.Run();
