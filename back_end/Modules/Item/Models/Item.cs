using back_end.Modules.servicios.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_end.Modules.Item.Models;

public partial class Item
{
    public string? Id { get; set; }

    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }

    public int? Stock { get; set; }

    public int StockDisponible { get; set; }

    public string? Preciobase { get; set; } 

    public virtual ICollection<DetalleServicio> DetalleServicios { get; set; } = new List<DetalleServicio>();
    
    // Propiedad temporal para cálculos (no mapeada a BD)
    [NotMapped]
    public double CantidadEnUso { get; set; }
    
    // Método auxiliar para obtener el precio como decimal cuando sea necesario
    [NotMapped]
    public decimal? PrecioDecimal 
    {
        get 
        {
            if (string.IsNullOrEmpty(Preciobase))
                return null;
                
            if (decimal.TryParse(Preciobase, out decimal result))
                return result;
                
            return null;
        }
    }
}
