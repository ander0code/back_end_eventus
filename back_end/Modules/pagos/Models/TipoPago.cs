namespace back_end.Modules.pagos.Models;
public partial class TipoPago
{
    public string Id { get; set; } = null!;

    public string? Nombre { get; set; }

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
