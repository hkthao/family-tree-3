using System.Security.Claims;
using backend.Application.Common.Interfaces;

namespace backend.Web.Services;

public class CurrentUser : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IApplicationDbContext _context;
    private Guid? _profileIdCache; // Cached ProfileId

    public CurrentUser(IHttpContextAccessor httpContextAccessor, IApplicationDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public Guid? Id
    {
        get
        {
            if (_profileIdCache.HasValue)
            {
                return _profileIdCache;
            }

            if (string.IsNullOrEmpty(ExternalId))
            {
                return null;
            }

            // Resolve ProfileId asynchronously. This is a bit tricky in a property getter.
            // For simplicity, we'll block here, but in a real app, consider making IUser async or resolving ProfileId earlier.
            var userProfile = _context.UserProfiles.FirstOrDefault(up => up.ExternalId == ExternalId);
            _profileIdCache = userProfile?.Id;
            return _profileIdCache;
        }
    }

    public string? ExternalId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public string? DisplayName => _httpContextAccessor.HttpContext?.User?.FindFirstValue("name"); // Or other claim for display name

    public bool IsAuthenticated => !string.IsNullOrEmpty(ExternalId);

    public List<string>? Roles => _httpContextAccessor.HttpContext?.User?.FindAll("https://familytree.com/roles").Select(x => x.Value).ToList();
}