using backend.Domain.Enums;
using System.Text.Json.Serialization;

namespace backend.Application.Memories.DTOs;

public class AiPhotoAnalysisInputDto
{
    public string? ImageUrl { get; set; }
    public string? ImageBase64 { get; set; }
    public string? ImageSize { get; set; } // e.g., "512x512"
    public List<AiDetectedFaceDto> Faces { get; set; } = new List<AiDetectedFaceDto>();
    public string? TargetFaceId { get; set; }
    public string? TargetFaceCropUrl { get; set; }
    public AiMemberInfoDto? MemberInfo { get; set; }
    public List<AiOtherFaceSummaryDto>? OtherFacesSummary { get; set; }
    public AiExifInfoDto? Exif { get; set; }
}

public class AiDetectedFaceDto
{
    public string FaceId { get; set; } = Guid.NewGuid().ToString(); // Use Guid for unique ID
    public List<int> Bbox { get; set; } = new List<int>(); // [x, y, w, h]
    public AiEmotionLocalDto EmotionLocal { get; set; } = new AiEmotionLocalDto();
    public string? Quality { get; set; }
}

public class AiEmotionLocalDto
{
    public string Dominant { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

public class AiMemberInfoDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Gender? Gender { get; set; } // Assuming Gender is an enum
    public int? Age { get; set; }
}

public class AiOtherFaceSummaryDto
{
    public string? EmotionLocal { get; set; }
}

public class AiExifInfoDto
{
    public string? Datetime { get; set; }
    public string? Gps { get; set; }
    public string? CameraInfo { get; set; }
}

public class PhotoAnalysisResultDto
{
    public string? Summary { get; set; }
    public string? Scene { get; set; }
    public string? Event { get; set; }
    public string? Emotion { get; set; }
    public string? YearEstimate { get; set; }
    public List<string>? Objects { get; set; }
    public List<PhotoAnalysisPersonDto>? Persons { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PhotoAnalysisPersonDto
{
    public string? Id { get; set; }
    public string? MemberId { get; set; }
    public string? Name { get; set; }
    public string? Emotion { get; set; }
    public double? Confidence { get; set; } // NEW PROPERTY
}