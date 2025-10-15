using backend.Application.Common.Models;
using backend.Application.Families.Queries;
using MediatR;

namespace backend.Application.Families.Commands.CreateFamilies;

public record CreateFamiliesCommand(List<FamilyDto> Families) : IRequest<Result<List<Guid>>>;