using backend.Application.Common.Models;
using backend.Application.Families;

namespace Application.NaturalLanguageInput.Commands.GenerateData;

public record GenerateFamilyDataCommand(string Prompt) : IRequest<Result<List<FamilyDto>>>;