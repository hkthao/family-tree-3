using backend.Domain.Enums;
using MediatR;

namespace backend.Application.Relationships.Commands.CreateRelationship;

public record CreateRelationshipCommand : IRequest<string>
{
    public string? MemberId { get; init; }
    public RelationshipType Type { get; init; }
    public string? TargetId { get; init; }
    public string? FamilyId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
