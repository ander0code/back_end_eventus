using System;
using System.Collections.Generic;

using back_end.Modules.inventario.Models;

namespace back_end.Modules.servicios.Models;

public partial class ServicioInventario
{
    public int ServicioId { get; set; }

    public int ItemId { get; set; }

    public int? CantidadUtilizada { get; set; }

    public virtual Inventario Item { get; set; } = null!;

    public virtual Servicio Servicio { get; set; } = null!;
}
