namespace backend.Application.Common.Models.AppSetting;

public class CohereSettings
{
    public const string SectionName = "CohereSettings";
    public string ApiKey { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int MaxTextLength { get; set; }
}
