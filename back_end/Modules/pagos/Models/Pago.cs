namespace back_end.Modules.pagos.Models;
using back_end.Modules.reservas.Models;

public partial class Pago
{
    public string Id { get; set; } = null!;

    public string? IdReserva { get; set; }

    public string? IdTipoPago { get; set; }

    public string? Monto { get; set; }

    public virtual Reserva? IdReservaNavigation { get; set; }

    public virtual TipoPago? IdTipoPagoNavigation { get; set; }
}
