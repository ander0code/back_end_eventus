using System;
using System.Collections.Generic;

using back_end.Modules.usuarios.Models;
using back_end.Modules.servicios.Models;

namespace back_end.Modules.inventario.Models;
public partial class Inventario
{
    public int Id { get; set; }

    public int? UsuarioId { get; set; }

    public string? NombreItem { get; set; }

    public string? Descripcion { get; set; }

    public int? StockActual { get; set; }

    public int? StockMinimo { get; set; }

    public decimal? PrecioDia { get; set; }

    public decimal? PrecioSemana { get; set; }

    public decimal? PrecioMes { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public virtual ICollection<ServicioInventario> ServicioInventarios { get; set; } = new List<ServicioInventario>();

    public virtual Usuario? Usuario { get; set; }
}
