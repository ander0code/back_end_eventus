namespace back_end.Modules.organizador.Models;

public partial class Organizador
{
    public string Id { get; set; } = null!;

    public string? NombreNegocio { get; set; }

    public string? Contrasena { get; set; }

    public string? UsuarioId { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
