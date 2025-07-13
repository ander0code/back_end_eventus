namespace back_end.Modules.servicios.Models;

using back_end.Modules.reservas.Models;

public partial class Servicio
{
    public string? Id { get; set; }

    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }

    public decimal? PrecioBase { get; set; }

    public virtual ICollection<DetalleServicio> DetalleServicios { get; set; } = new List<DetalleServicio>();

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
