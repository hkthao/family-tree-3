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
            cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
        });

        services.AddScoped<Common.Interfaces.IFamilyTreeService, Services.FamilyTreeService>();
        services.AddScoped<Common.Services.FamilyAuthorizationService>(); 
        services.Configure<Common.Models.EmbeddingSettings>(configuration.GetSection(nameof(Common.Models.EmbeddingSettings)));

        return services;
    }
}
