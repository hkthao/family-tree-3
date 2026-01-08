namespace backend.Application.FamilyFollows;

public class FamilyFollowDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid FamilyId { get; set; }
    public bool IsFollowing { get; set; }
    public bool NotifyDeathAnniversary { get; set; }
    public bool NotifyBirthday { get; set; }
    public bool NotifyEvent { get; set; }
}
