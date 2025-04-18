namespace back_end.Modules.reservas.Models;

using back_end.Modules.servicios.Models;

public partial class ReservaServicio
{
    public Guid Id { get; set; }

    public Guid ReservaId { get; set; }

    public Guid ServicioId { get; set; }

    public int? CantidadItems { get; set; }

    public decimal? Precio { get; set; }

    public virtual Reserva Reserva { get; set; } = null!;

    public virtual Servicio Servicio { get; set; } = null!;
}
