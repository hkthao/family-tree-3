using System.Reflection;
using Microsoft.AspNetCore.Mvc;

using backend.Application.Common.Constants;
using Microsoft.AspNetCore.RateLimiting;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting(RateLimitConstants.PerUserPolicy)]
public class VersionController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "N/A";
        return Ok(new { version });
    }
}
