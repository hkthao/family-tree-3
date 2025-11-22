namespace backend.Web.Filters;

public class ApiKeySettings
{
    public string HeaderName { get; set; } = "X-App-Key";
    public string ApiKeyValue { get; set; } = string.Empty; // Sẽ được cấu hình từ appsettings.json
}
