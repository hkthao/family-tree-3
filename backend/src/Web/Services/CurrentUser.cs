using System.Security.Claims;
using backend.Application.Common.Interfaces;
using MongoDB.Bson;

namespace backend.Web.Services;

public class CurrentUser : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ObjectId? Id
    {
        get
        {
            var idString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return idString != null ? ObjectId.Parse(idString) : (ObjectId?)null;
        }
    }
    public List<string>? Roles => _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList();

}
