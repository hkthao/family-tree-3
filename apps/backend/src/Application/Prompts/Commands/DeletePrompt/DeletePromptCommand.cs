using backend.Application.Common.Models;
using MediatR;

namespace backend.Application.Prompts.Commands.DeletePrompt;

public record DeletePromptCommand(Guid Id) : IRequest<Result>;
