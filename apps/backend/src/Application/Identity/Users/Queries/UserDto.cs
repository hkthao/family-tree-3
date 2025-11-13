namespace backend.Application.Users.Queries;

public class UserDto
{
    public string Id { get; set; } = null!;
    public string AuthProviderId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
}
