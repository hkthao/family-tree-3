using backend.Application.Common.Models;

namespace backend.Application.FamilyLocations.Queries.ExportFamilyLocations;

public record ExportFamilyLocationsQuery(Guid FamilyId) : IRequest<Result<List<FamilyLocationDto>>>;
