namespace back_end.Modules.Auth.DTOs
{
    public class AuthRequestDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AuthResponseDTO
    {
        public string Username { get; set; }
        public string Token { get; set; }
    }
}