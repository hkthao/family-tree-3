namespace backend.Application.Identity.UserProfiles.Queries
{
    public class UserProfileDto
    {
        public string Id { get; set; } = null!;
        public string ExternalId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Avatar { get; set; }
        public List<string> Roles { get; set; } = [];
    }
}
