using backend.Application.Common.Models;
using MediatR;
using backend.Domain.Enums;

namespace backend.Application.FamilyLinkRequests.Commands.UpdateFamilyLinkRequest;

public record UpdateFamilyLinkRequestCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public LinkStatus Status { get; init; } // Allow updating status
}

