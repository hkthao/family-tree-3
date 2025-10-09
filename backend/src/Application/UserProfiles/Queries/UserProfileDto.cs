namespace backend.Application.UserProfiles.Queries;

public class UserProfileDto
{
    public string Id { get; set; } = null!;
    public string Auth0UserId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Avatar { get; set; }
}
