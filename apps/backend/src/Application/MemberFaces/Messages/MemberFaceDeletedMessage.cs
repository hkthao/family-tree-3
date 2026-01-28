namespace backend.Application.MemberFaces.Messages;

public class MemberFaceDeletedMessage
{
    public Guid MemberFaceId { get; set; }
    public string? VectorDbId { get; set; }
    public Guid MemberId { get; set; }
    public Guid FamilyId { get; set; }
}
