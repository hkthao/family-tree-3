using backend.Application.Common.Models;
using backend.Application.ImageRestorationJobs.Common;

namespace backend.Application.ImageRestorationJobs.Commands.CreateImageRestorationJob;

public record CreateImageRestorationJobCommand(
    string OriginalImageUrl,
    string FamilyId
) : IRequest<Result<ImageRestorationJobDto>>;
