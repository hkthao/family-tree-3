using backend.Application.Common.Models;

namespace backend.Application.FamilyLocations.Commands.DeleteFamilyLocation;

public record DeleteFamilyLocationCommand(Guid Id) : IRequest<Result>;
