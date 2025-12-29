using backend.Application.Common.Models;

namespace backend.Application.FamilyLocations.Commands.ImportFamilyLocations;

public record ImportFamilyLocationsCommand(Guid FamilyId, List<ImportFamilyLocationItemDto> Locations) : IRequest<Result<List<FamilyLocationDto>>>;
