namespace backend.Infrastructure;

public class ImgbbSettings
{
    public const string SectionName = "ImgbbSettings";
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.imgbb.com/1/";
}
