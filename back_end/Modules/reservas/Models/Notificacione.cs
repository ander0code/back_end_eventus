using System;
using System.Collections.Generic;

namespace back_end.Modules.reservas.Models;

public partial class Notificacione
{
    public int Id { get; set; }

    public int? ReservaId { get; set; }

    public string? TipoNotificacion { get; set; }

    public DateTime? FechaEnvio { get; set; }

    public bool? EstadoEnvio { get; set; }

    public virtual Reserva? Reserva { get; set; }
}
