using backend.Domain.Enums;

namespace backend.Application.FamilyMedias.DTOs;

public class MediaLinkDto
{
    public Guid Id { get; set; }
    public Guid FamilyMediaId { get; set; }
    public RefType RefType { get; set; }
    public Guid RefId { get; set; }
    public string? RefName { get; set; } // Name of the linked entity (e.g., Member's FullName, MemberStory's Title)
}
