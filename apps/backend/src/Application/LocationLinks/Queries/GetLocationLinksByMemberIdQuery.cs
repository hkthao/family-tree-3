using backend.Application.Common.Models;
using MediatR;

namespace backend.Application.LocationLinks.Queries;

public record GetLocationLinksByMemberIdQuery : IRequest<Result<List<LocationLinkDto>>>
{
    public Guid MemberId { get; init; }
}
