using backend.Infrastructure.Identity;
using Microsoft.AspNetCore.Routing;

namespace backend.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapIdentityApi<ApplicationUser>();
    }
}
