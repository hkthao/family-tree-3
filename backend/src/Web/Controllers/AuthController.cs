using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthProvider _authProvider;

    public AuthController(IAuthProvider authProvider)
    {
        _authProvider = authProvider;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> Login([FromBody] LoginRequest request)
    {
        var result = await _authProvider.LoginAsync(request.Email, request.Password);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return Unauthorized(result.Error);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authProvider.RegisterAsync(request.Email, request.Password, request.Username);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<ActionResult<AuthResult>> GetCurrentUser()
    {
        // In a real application, you would get the user ID from the authenticated context
        // For this mock, we'll use a dummy user ID or assume it's passed in headers/claims
        var userId = "auth0|dummyuser"; // Placeholder
        var result = await _authProvider.GetUserAsync(userId);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return NotFound(result.Error);
    }
}

public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class RegisterRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Username { get; set; } = null!;
}