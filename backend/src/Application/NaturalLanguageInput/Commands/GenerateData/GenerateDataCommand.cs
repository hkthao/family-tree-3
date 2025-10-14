using backend.Application.Common.Models;
using MediatR;

namespace Application.NaturalLanguageInput.Commands.GenerateData;

public record GenerateDataCommand(string Prompt) : IRequest<Result<string>>;