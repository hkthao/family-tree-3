using backend.Application.Common.Mappings;
using backend.Domain.Entities;
using backend.Application.Common.Dtos;

namespace backend.Application.Members.Queries.GetMembers;

public class MemberListDto : BaseAuditableDto, IMapFrom<Member>
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public Guid FamilyId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public ICollection<RelationshipDto> Relationships { get; set; } = new List<RelationshipDto>();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Member, MemberListDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => $"{s.LastName} {s.FirstName}"))
            .ForMember(d => d.AvatarUrl, opt => opt.MapFrom(s => s.AvatarUrl))
            .ForMember(d => d.FamilyId, opt => opt.MapFrom(s => s.FamilyId))
            .ForMember(d => d.DateOfBirth, opt => opt.MapFrom(s => s.DateOfBirth))
            .ForMember(d => d.Gender, opt => opt.MapFrom(s => s.Gender));
    }
}