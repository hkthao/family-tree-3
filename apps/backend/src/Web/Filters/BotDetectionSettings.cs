namespace backend.Web.Filters;

public class BotDetectionSettings
{
    public string[] BlacklistedUserAgents { get; set; } = [];
    public bool BlockEmptyUserAgent { get; set; } = false; // Mặc định không chặn
    public string[] AllowedAcceptHeaders { get; set; } = ["application/json", "*/*"];
}
