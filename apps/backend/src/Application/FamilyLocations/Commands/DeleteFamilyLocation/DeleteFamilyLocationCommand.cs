using backend.Application.Common.Models;
using MediatR; // Explicitly add MediatR

namespace backend.Application.FamilyLocations.Commands.DeleteFamilyLocation;

public record DeleteFamilyLocationCommand(Guid Id) : IRequest<Result>;
