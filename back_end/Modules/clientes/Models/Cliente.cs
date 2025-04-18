namespace back_end.Modules.clientes.Models;
using back_end.Modules.reservas.Models;
using back_end.Modules.usuarios.Models;
public partial class Cliente
{
    public Guid Id { get; set; }

    public string? TipoCliente { get; set; }

    public string Nombre { get; set; } = null!;

    public string? CorreoElectronico { get; set; }

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public Guid UsuarioId { get; set; }

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    public virtual Usuario Usuario { get; set; } = null!;
}
