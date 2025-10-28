namespace backend.Application.Identity.UserProfiles.Queries;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Avatar { get; set; }
    public List<string> Roles { get; set; } = [];
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
}
