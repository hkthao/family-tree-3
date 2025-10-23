namespace backend.Application.Common.Models.AppSetting;

public class FaceDetectionSettings
{
    public const string SectionName = "FaceDetectionSettings";
    public string BaseUrl { get; set; } = null!;
}
