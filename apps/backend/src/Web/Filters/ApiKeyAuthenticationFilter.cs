using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace backend.Web.Filters;

public class ApiKeyAuthenticationFilter : IAsyncActionFilter
{
    private readonly ILogger<ApiKeyAuthenticationFilter> _logger;
    private readonly ApiKeySettings _apiKeySettings;

    public ApiKeyAuthenticationFilter(ILogger<ApiKeyAuthenticationFilter> logger, IOptions<ApiKeySettings> apiKeySettings)
    {
        _logger = logger;
        _apiKeySettings = apiKeySettings.Value;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 1. Kiểm tra xem có cần bỏ qua xác thực API Key không (ví dụ: cho các endpoint đặc biệt)
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.Any(em => em.GetType() == typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute));
        if (allowAnonymous)
        {
            await next();
            return;
        }

        // 2. Kiểm tra sự tồn tại của Header và giá trị của API Key
        if (!context.HttpContext.Request.Headers.TryGetValue(_apiKeySettings.HeaderName, out var extractedApiKey))
        {
            _logger.LogWarning("API Key missing from request from {RemoteIpAddress}. Expected header: {HeaderName}",
                context.HttpContext.Connection.RemoteIpAddress, _apiKeySettings.HeaderName);
            context.Result = new UnauthorizedResult(); // 401 Unauthorized
            return;
        }

        if (!_apiKeySettings.ApiKeyValue.Equals(extractedApiKey))
        {
            _logger.LogWarning("Invalid API Key '{ExtractedApiKey}' from {RemoteIpAddress}. Expected header: {HeaderName}",
                extractedApiKey, context.HttpContext.Connection.RemoteIpAddress, _apiKeySettings.HeaderName);
            context.Result = new UnauthorizedResult(); // 401 Unauthorized
            return;
        }

        // Nếu API Key hợp lệ, tiếp tục xử lý request
        await next();
    }
}
