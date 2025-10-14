using backend.Application.Common.Models;
using backend.Application.NaturalLanguageInput.Queries;
using MediatR;

namespace Application.NaturalLanguageInput.Commands.GenerateData;

public record GenerateDataCommand(string Prompt) : IRequest<Result<GeneratedEntityDto>>;