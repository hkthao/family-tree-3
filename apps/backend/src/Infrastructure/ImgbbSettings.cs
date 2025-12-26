namespace FamilyTree.Infrastructure;

public class ImgbbSettings
{
    public const string SectionName = "Imgbb";
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.imgbb.com/1/";
}
