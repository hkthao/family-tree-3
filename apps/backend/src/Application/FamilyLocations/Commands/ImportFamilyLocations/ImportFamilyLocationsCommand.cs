using backend.Application.Common.Models;
using MediatR;

namespace backend.Application.FamilyLocations.Commands.ImportFamilyLocations;

public record ImportFamilyLocationsCommand(Guid FamilyId, List<ImportFamilyLocationItemDto> Locations) : IRequest<Result<List<FamilyLocationDto>>>;