using backend.Application.Common.Models;
using MediatR;

namespace backend.Application.FamilyLinkRequests.Commands.DeleteFamilyLinkRequest;

public record DeleteFamilyLinkRequestCommand(Guid Id) : IRequest<Result<Unit>>;
