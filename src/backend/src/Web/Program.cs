
using backend.CompositionRoot;
using Microsoft.Extensions.FileProviders;

public partial class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCompositionRootServices(Configuration);
        services.AddWebServices();

        services.AddControllers();

        services.AddCors(options => options.AddPolicy("AllowFrontend",
            policyBuilder =>
            {
                var corsOriginsString = Configuration["CORS_ORIGINS"];
                var origins = new List<string>();
                if (!string.IsNullOrEmpty(corsOriginsString))
                {
                    origins.AddRange(corsOriginsString.Split(',', StringSplitOptions.RemoveEmptyEntries));
                }

                if (origins.Any())
                {
                    policyBuilder.WithOrigins([.. origins])
                                 .AllowAnyHeader()
                                 .AllowAnyMethod();
                }
                else
                {
                    Console.WriteLine("CORS_ORIGINS environment variable not found or is empty.");
                }
            }));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            // Initialise and seed database
            // using var scope = app.ApplicationServices.CreateScope();
            // var initialiser = scope.ServiceProvider.GetRequiredService<backend.Infrastructure.Data.ApplicationDbContextInitialiser>();
            // await initialiser.InitialiseAsync();
            // await initialiser.SeedAsync();
        }
        else
        {
            app.UseHsts();
        }



        app.UseCors("AllowFrontend");
        app.UseHealthChecks("/health");

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(env.WebRootPath)),
            RequestPath = "",
        });

        app.UseSwaggerUi(settings =>
        {
            settings.Path = "/api";
            settings.DocumentPath = "/api/specification.json";
        });

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseExceptionHandler(options => { });

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapFallbackToFile("index.html");
            endpoints.MapGet("/", context =>
            {
                context.Response.Redirect("/api", permanent: false);
                return Task.CompletedTask;
            });
        });
    }
}

