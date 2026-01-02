using backend.Application.Common.Models;
using backend.Application.ImageRestorationJobs.Common;

namespace backend.Application.ImageRestorationJobs.Commands.CreateImageRestorationJob;

public record CreateImageRestorationJobCommand(
    byte[] ImageData,                 // New parameter for image data
    string FileName,
    string ContentType,
    Guid FamilyId,
    bool UseCodeformer = false        // New parameter for optional CodeFormer step
) : IRequest<Result<ImageRestorationJobDto>>;
