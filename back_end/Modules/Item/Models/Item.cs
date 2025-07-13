namespace back_end.Modules.Item.Models;
using back_end.Modules.servicios.Models;

public partial class Item
{
    public string? Id { get; set; }

    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }

    public int? Stock { get; set; }

    public int StockDisponible { get; set; }

    public string? Preciobase { get; set; }

    public virtual ICollection<DetalleServicio> DetalleServicios { get; set; } = new List<DetalleServicio>();
}
