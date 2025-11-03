using System.Security.Claims;
using backend.Application.Common.Interfaces;
using backend.Domain.Constants;
using Microsoft.AspNetCore.Http;

namespace backend.Infrastructure.Services;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            if (_httpContextAccessor.HttpContext?.Items.TryGetValue(HttpContextItemKeys.UserId, out var userIdObj) == true && userIdObj is Guid userId)
            {
                return userId;
            }
            return Guid.Empty; // Hoặc ném ngoại lệ nếu UserId luôn bắt buộc
        }
    }

    public Guid? ProfileId
    {
        get
        {
            if (_httpContextAccessor.HttpContext?.Items.TryGetValue(HttpContextItemKeys.ProfileId, out var profileIdObj) == true && profileIdObj is Guid profileId)
            {
                return profileId;
            }
            return null;
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public string? Name => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name); // Sử dụng ClaimTypes.Name cho tên hiển thị

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public List<string>? Roles => _httpContextAccessor.HttpContext?.User?.FindAll("https://familytree.com/roles").Select(x => x.Value).ToList();
}