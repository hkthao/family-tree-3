using System;
using backend.Application.Common.Models;
using backend.Application.ImageRestorationJobs.Common;

namespace backend.Application.ImageRestorationJobs.Commands.CreateImageRestorationJob;

public record CreateImageRestorationJobCommand(
    string OriginalImageUrl,
    Guid FamilyId
) : IRequest<Result<ImageRestorationJobDto>>;
