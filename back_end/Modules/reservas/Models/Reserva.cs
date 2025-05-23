namespace back_end.Modules.reservas.Models;
using back_end.Modules.clientes.Models;
using back_end.Modules.pagos.Models;
using back_end.Modules.servicios.Models;

public partial class Reserva
{
    public string Id { get; set; } = null!;

    public string? NombreEvento { get; set; }

    public DateOnly? FechaEjecucion { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public string? Descripcion { get; set; }

    public string? Estado { get; set; }

    public decimal? PrecioTotal { get; set; }

    public Guid? TiposEvento { get; set; }

    public string? ClienteId { get; set; }

    public Guid? ServicioId { get; set; }

    public double? PrecioAdelanto { get; set; }

    public virtual Cliente? Cliente { get; set; }

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    public virtual Servicio? Servicio { get; set; }

    public virtual TiposEvento? TiposEventoNavigation { get; set; }
}
