using backend.Application.Common.Models;

namespace backend.Application.Identity.UserProfiles.Commands.UpdateUserProfile;

public class UpdateUserProfileCommand : IRequest<Result>
{
    public Guid Id { get; private set; }
    public string? Name { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? AvatarBase64 { get; set; }
    public string? Email { get; set; }
    public Dictionary<string, object>? UserMetadata { get; set; }

    public void SetId(Guid id)
    {
        Id = id;
    }
}
