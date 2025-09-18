using MediatR;
using backend.Domain.Enums;
using System;

namespace backend.Application.Relationships.Commands.UpdateRelationship;

public record UpdateRelationshipCommand : IRequest
{
    public string Id { get; init; } = null!;
    public string? SourceMemberId { get; init; }
    public string? TargetMemberId { get; init; }
    public RelationshipType Type { get; init; }
    public string? FamilyId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
