namespace back_end.Modules.reservas.Models;
using back_end.Modules.clientes.Models; 
using back_end.Modules.usuarios.Models;

public partial class Reserva
{
    public Guid Id { get; set; }

    public string? NombreEvento { get; set; }

    public DateOnly? FechaEvento { get; set; }

    public string? HoraEvento { get; set; }

    public string? TipoEvento { get; set; }

    public string? Descripcion { get; set; }

    public string? Estado { get; set; }

    public decimal? PrecioTotal { get; set; }

    public Guid UsuarioId { get; set; }

    public Guid ClienteId { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual ICollection<ReservaServicio> ReservaServicios { get; set; } = new List<ReservaServicio>();

    public virtual Usuario Usuario { get; set; } = null!;
}
