using backend.Application.Common.Models;
namespace backend.Application.MemberFaces.Queries.SearchVectorFace;
public record SearchMemberFaceQuery : IRequest<Result<List<FoundFaceDto>>>
{
    public SearchMemberFaceQuery(Guid familyId)
    {
        FamilyId = familyId;
    }
    public Guid FamilyId { get; init; } // Bắt buộc: lọc tìm kiếm theo một gia đình cụ thể
    public Guid? MemberId { get; init; } // Optional: filter search to a specific member
    public List<float> Vector { get; init; } = new List<float>(); // Face embedding to search with
    public int Limit { get; init; } = 5; // Max number of results
}
