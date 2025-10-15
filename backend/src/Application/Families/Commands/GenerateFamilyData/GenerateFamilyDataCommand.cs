using backend.Application.Common.Models;

namespace backend.Application.Families.Commands.GenerateFamilyData;

public record GenerateFamilyDataCommand(string Prompt) : IRequest<Result<List<FamilyDto>>>;