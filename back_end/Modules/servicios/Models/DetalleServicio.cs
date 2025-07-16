namespace back_end.Modules.servicios.Models;
using back_end.Modules.Item.Models;


public partial class DetalleServicio
{
    public string? Id { get; set; }

    public string? ServicioId { get; set; }

    public string? InventarioId { get; set; }

    public double? Cantidad { get; set; }

    public string? Estado { get; set; }

    public DateTime? Fecha { get; set; }

    public string? PrecioActual { get; set; }

    public virtual back_end.Modules.Item.Models.Item? Inventario { get; set; }

    public virtual Servicio? Servicio { get; set; }
}
