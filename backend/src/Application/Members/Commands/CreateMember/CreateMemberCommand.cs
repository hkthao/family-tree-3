using MediatR;
using MongoDB.Bson;

namespace backend.Application.Members.Commands.CreateMember;

public record CreateMemberCommand : IRequest<string>
{
    public string? FullName { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public DateTime? DateOfDeath { get; init; }
    public string? Status { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public int Generation { get; init; }
    public int DisplayOrder { get; init; }
    public string? FamilyId { get; init; }
    public string? Description { get; init; }
}
