using backend.Application.Common.Behaviours;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(System.Reflection.Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
        });


        services.AddScoped<Common.Interfaces.IRelationshipDetectionService, Services.RelationshipDetectionService>();

        return services;
    }
}
