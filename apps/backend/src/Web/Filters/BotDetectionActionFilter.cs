using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace backend.Web.Filters;

public class BotDetectionActionFilter : ActionFilterAttribute
{
    private readonly ILogger<BotDetectionActionFilter> _logger;
    private readonly BotDetectionSettings _settings;

    public BotDetectionActionFilter(ILogger<BotDetectionActionFilter> logger, IOptions<BotDetectionSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var request = context.HttpContext.Request;
        var userAgent = request.Headers["User-Agent"].ToString();
        var remoteIpAddress = context.HttpContext.Connection.RemoteIpAddress;

        // 1. Kiểm tra User-Agent: Chỉ log, không chặn (theo yêu cầu mới)
        if (string.IsNullOrEmpty(userAgent))
        {
            _logger.LogInformation("Missing User-Agent header from {RemoteIpAddress}. Not blocking as per new requirements.", remoteIpAddress);
        }
        else
        {
            foreach (var blacklisted in _settings.BlacklistedUserAgents)
            {
                if (userAgent.Contains(blacklisted, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Potential bot detected: Blacklisted User-Agent '{UserAgent}' from {RemoteIpAddress}. Not blocking, but logging for analysis.", userAgent, remoteIpAddress);
                    // Không chặn, chỉ log
                    break;
                }
            }
        }

        // 2. Kiểm tra Accept header
        var acceptHeader = request.Headers["Accept"].ToString();
        if (string.IsNullOrEmpty(acceptHeader))
        {
            _logger.LogWarning("Request blocked: Missing Accept header from {RemoteIpAddress}", remoteIpAddress);
            context.Result = new StatusCodeResult(403); // Forbidden
            return;
        }

        bool acceptHeaderValid = false;
        foreach (var allowedAccept in _settings.AllowedAcceptHeaders)
        {
            if (acceptHeader.Contains(allowedAccept, StringComparison.OrdinalIgnoreCase))
            {
                acceptHeaderValid = true;
                break;
            }
        }

        if (!acceptHeaderValid)
        {
            _logger.LogWarning("Request blocked: Invalid Accept header '{AcceptHeader}' from {RemoteIpAddress}", acceptHeader, remoteIpAddress);
            context.Result = new StatusCodeResult(403); // Forbidden
            return;
        }

        // 3. Kiểm tra Content-Type cho các yêu cầu POST và PUT
        if (request.Method == HttpMethod.Post.Method || request.Method == HttpMethod.Put.Method)
        {
            var contentType = request.Headers["Content-Type"].ToString();
            // Yêu cầu phải là application/json cho các API này
            if (string.IsNullOrEmpty(contentType) || !contentType.StartsWith(MediaTypeNames.Application.Json, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Request blocked: Invalid or missing Content-Type '{ContentType}' for {HttpMethod} request from {RemoteIpAddress}. Expected application/json.", contentType, request.Method, remoteIpAddress);
                context.Result = new StatusCodeResult(403); // Forbidden
                return;
            }
        }

        base.OnActionExecuting(context);
    }
}
