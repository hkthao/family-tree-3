using MediatR;
using backend.Application.Common.Models;

namespace backend.Application.FamilyFollows.Queries.GetFollowStatus;

public record GetFollowStatusQuery : IRequest<Result<FamilyFollowDto>>
{
    public Guid FamilyId { get; init; }
}
