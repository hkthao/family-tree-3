using backend.Application.Common.Models;

namespace backend.Application.ImageRestorationJobs.Commands.DeleteImageRestorationJob;

public record DeleteImageRestorationJobCommand(Guid Id) : IRequest<Result<Unit>>;
