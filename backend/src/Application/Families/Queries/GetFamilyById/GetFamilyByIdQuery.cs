using backend.Application.Families;
using MediatR;

namespace backend.Application.Families.Queries.GetFamilyById;

public record GetFamilyByIdQuery(string Id) : IRequest<FamilyDto>;
