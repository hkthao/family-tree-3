using backend.Application.Families;
using MediatR;
using MongoDB.Bson;

namespace backend.Application.Families.Queries.GetFamilyById;

public record GetFamilyByIdQuery(string Id) : IRequest<FamilyDto>;
