using MediatR;

namespace backend.Application.Members.Commands.UpdateMember;

public record UpdateMemberCommand : IRequest
{
    public string? Id { get; init; }
    public string? FullName { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public DateTime? DateOfDeath { get; init; }
    public string? Gender { get; init; }
    public string? AvatarUrl { get; init; }
    public string? PlaceOfBirth { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public int Generation { get; init; }
    public string? Biography { get; init; }
    public object? Metadata { get; init; }
}
