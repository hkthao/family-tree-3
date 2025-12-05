using backend.Application.Common.Models;

namespace backend.Application.Prompts.Commands.DeletePrompt;

public record DeletePromptCommand(Guid Id) : IRequest<Result>;
