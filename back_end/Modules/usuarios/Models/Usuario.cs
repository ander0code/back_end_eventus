namespace back_end.Modules.usuarios.Models;

using back_end.Modules.inventario.Models;
using back_end.Modules.reservas.Models;
using back_end.Modules.servicios.Models;
using back_end.Modules.clientes.Models;

public partial class Usuario
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string CorreoElectronico { get; set; } = null!;

    public string? Celular { get; set; }

    public string Contrasena { get; set; } = null!;

    public bool? Verificado { get; set; }

    public string? TokenVerificacion { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    public virtual ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();
}
