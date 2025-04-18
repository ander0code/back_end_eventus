namespace back_end.Modules.servicios.Models;
using back_end.Modules.inventario.Models;

public partial class ServicioItem
{
    public Guid Id { get; set; }

    public Guid ServicioId { get; set; }

    public Guid InventarioId { get; set; }

    public int? Cantidad { get; set; }

    public virtual Inventario Inventario { get; set; } = null!;

    public virtual Servicio Servicio { get; set; } = null!;
}
