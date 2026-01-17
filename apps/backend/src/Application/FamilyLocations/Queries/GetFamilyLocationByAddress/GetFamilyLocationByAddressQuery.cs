using backend.Application.Common.Models;

namespace backend.Application.FamilyLocations.Queries.GetFamilyLocationByAddress;

public record GetFamilyLocationByAddressQuery(string Address) : IRequest<Result<FamilyLocationDto>>;
