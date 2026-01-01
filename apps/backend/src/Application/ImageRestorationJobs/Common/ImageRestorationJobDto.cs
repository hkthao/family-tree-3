using AutoMapper;
using backend.Domain.Entities;
using backend.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backend.Application.ImageRestorationJobs.Common;

public class ImageRestorationJobDto
{
    public string JobId { get; set; } = string.Empty;
    public string OriginalImageUrl { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public RestorationStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RestoredImageUrl { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class ImageRestorationJobProfile : Profile
{
    public ImageRestorationJobProfile()
    {
        CreateMap<ImageRestorationJob, ImageRestorationJobDto>();
    }
}
