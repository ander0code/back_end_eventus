namespace back_end.Modules.servicios.Models;
using back_end.Modules.Item.Models;
public partial class DetalleServicio
{
    public Guid Id { get; set; }

    public Guid? ServicioId { get; set; }

    public Guid? InventarioId { get; set; }

    public double? Cantidad { get; set; }

    public string? Estado { get; set; }

    public DateTime? Fecha { get; set; }

    public string? PrecioActual { get; set; }

    public virtual Item? Inventario { get; set; }

    public virtual Servicio? Servicio { get; set; }
}
