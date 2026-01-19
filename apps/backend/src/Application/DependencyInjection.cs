using backend.Application.Common.Behaviours;
using backend.Application.Common.Models.AppSetting; // Added for KnowledgeSearchServiceSettings
using backend.Application.Knowledge; // Added for IKnowledgeService and KnowledgeService
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options; // Added for IOptions

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
        services.AddTransient<Common.Services.SampleHangfireJob>();
        services.AddTransient<Common.Services.ILunarCalendarService, Common.Services.LunarCalendarService>();
        services.AddTransient<Events.EventOccurrences.Jobs.IGenerateEventOccurrencesJob, Events.EventOccurrences.Jobs.GenerateEventOccurrencesJob>();
        services.AddTransient<Events.EventOccurrences.Jobs.IEventNotificationJob, Events.EventOccurrences.Jobs.EventNotificationJob>();

        services.Configure<KnowledgeSearchServiceSettings>(configuration.GetSection(nameof(KnowledgeSearchServiceSettings)));
        services.AddHttpClient<IKnowledgeService, KnowledgeService>((serviceProvider, httpClient) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<KnowledgeSearchServiceSettings>>().Value;
            if (!string.IsNullOrEmpty(settings.BaseUrl))
            {
                httpClient.BaseAddress = new Uri(settings.BaseUrl);
            }
            // Optional: Add other HttpClient configurations like timeouts, default headers etc.
            // httpClient.Timeout = TimeSpan.FromSeconds(30); 
        });

        return services;
    }
}
