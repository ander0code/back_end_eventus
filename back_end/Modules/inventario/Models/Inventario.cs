namespace back_end.Modules.inventario.Models;
using back_end.Modules.servicios.Models;
using back_end.Modules.usuarios.Models;

public partial class Inventario
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int? Stock { get; set; }

    public string? Categoria { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public Guid UsuarioId { get; set; }

    public virtual ICollection<ServicioItem> ServicioItems { get; set; } = new List<ServicioItem>();

    public virtual Usuario Usuario { get; set; } = null!;
}
