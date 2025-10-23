using backend.Application.Common.Models;
using backend.Application.Events.Queries;

namespace backend.Application.Events.Commands.GenerateEventData;

public record GenerateEventDataCommand(string Prompt) : IRequest<Result<List<AIEventDto>>>;
