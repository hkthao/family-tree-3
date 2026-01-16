using backend.Application.Common.Models;
using backend.Application.FamilyLocations.Queries.SearchFamilyLocations;
using MediatR;

namespace backend.Application.FamilyLocations.Queries.GetFamilyLocationByAddress;

public record GetFamilyLocationByAddressQuery(string Address) : IRequest<Result<FamilyLocationDto>>;
