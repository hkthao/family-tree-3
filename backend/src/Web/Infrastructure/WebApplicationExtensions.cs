using System.Reflection;
using Microsoft.AspNetCore.Routing;

namespace backend.Web.Infrastructure;

public static class WebApplicationExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var endpointGroupType = typeof(EndpointGroupBase);

        var assembly = Assembly.GetExecutingAssembly();

        var endpointGroupTypes = assembly.GetExportedTypes()
            .Where(t => t.IsSubclassOf(endpointGroupType));

        foreach (var type in endpointGroupTypes)
        {
            if (Activator.CreateInstance(type) is EndpointGroupBase instance)
            {
                var groupName = instance.GroupName ?? instance.GetType().Name;
                var group = app.MapGroup($"/api/{groupName}")
                               .WithGroupName(groupName)
                               .WithTags(groupName);

                instance.Map(group);
            }
        }

        return app;
    }
}
