namespace back_end.Modules.pagos.DTOs
{
    public class PagoResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string? IdReserva { get; set; }
        public string? IdTipoPago { get; set; }
        public string? Monto { get; set; }
        public string? TipoPagoNombre { get; set; }
        public string? NombreReserva { get; set; }
    }

    public class PagoCreateDTO
    {
        public string? IdReserva { get; set; }
        public string? NombreTipoPago { get; set; }
        public string? Monto { get; set; }
    }

    public class PagoUpdateDTO
    {
        public string? NombreTipoPago { get; set; }
        public string? Monto { get; set; }
    }

    public class TipoPagoDTO
    {
        public string Id { get; set; } = string.Empty;
        public string? Nombre { get; set; }
    }
}