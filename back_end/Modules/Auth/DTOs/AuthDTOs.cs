namespace back_end.Modules.Auth.DTOs
{
    public class AuthRequestDTO
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public class AuthResponseDTO
    {
        public required string Username { get; set; }
        public required string Token { get; set; }
    }
}