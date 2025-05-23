namespace back_end.Modules.organizador.Models;
using back_end.Modules.clientes.Models;

public partial class Usuario
{
    public string Id { get; set; } = null!;

    public string? Nombre { get; set; }

    public string? Apellido { get; set; }

    public string? Correo { get; set; }

    public string? Celular { get; set; }

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ICollection<Organizador> Organizadors { get; set; } = new List<Organizador>();
}
