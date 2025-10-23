using backend.Application.Common.Models;
using backend.Application.Events;

namespace Application.NaturalLanguageInput.Commands.GenerateData;

public record GenerateEventDataCommand(string Prompt) : IRequest<Result<List<EventDto>>>;
