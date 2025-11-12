using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTree.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VersionController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "N/A";
        return Ok(new { version });
    }
}
