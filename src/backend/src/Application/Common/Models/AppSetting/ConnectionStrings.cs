namespace backend.Application.Common.Models.AppSetting;

public class ConnectionStrings
{
    public const string SectionName = "ConnectionStrings";
    public string DefaultConnection { get; set; } = null!;
}
