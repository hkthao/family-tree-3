namespace backend.Application.Common.Models
{
    public class AuthResult
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? AccessToken { get; set; }
        public List<string> Roles { get; set; } = [];
        public bool Succeeded { get; set; }
        public IEnumerable<string> Errors { get; set; } = new List<string>();
    }
}
