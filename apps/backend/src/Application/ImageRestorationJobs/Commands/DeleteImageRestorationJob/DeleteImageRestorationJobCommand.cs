using backend.Application.Common.Models;

namespace backend.Application.ImageRestorationJobs.Commands.DeleteImageRestorationJob;

public record DeleteImageRestorationJobCommand(string JobId) : IRequest<Result<Unit>>;
