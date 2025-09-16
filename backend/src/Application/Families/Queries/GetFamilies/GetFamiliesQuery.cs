using backend.Application.Common.Models;
using MediatR;
using MongoDB.Bson;

namespace backend.Application.Families.Queries.GetFamilies;

public record GetFamiliesQuery : IRequest<List<FamilyDto>>;
