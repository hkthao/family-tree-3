using backend.Application.Common.Models;
using backend.Application.Families;

namespace backend.Application.Families.Commands.GenerateFamilyData;

public record GenerateFamilyDataCommand(string Prompt) : IRequest<Result<List<FamilyDto>>>;