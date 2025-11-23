using backend.CompositionRoot;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using backend.Web.Formatters; // Added for custom HTML input formatter

/// <summary>
/// Lớp chính khởi tạo và chạy ứng dụng.
/// </summary>
public partial class Program
{
    /// <summary>
    /// Phương thức Main là điểm khởi đầu của ứng dụng.
    /// </summary>
    /// <param name="args">Các đối số dòng lệnh.</param>
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        // Apply migrations and seed data
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                if (context.Database.IsMySql()) // Only apply migrations if using MySQL
                {
                    await context.Database.MigrateAsync();
                }

                // var initialiser = services.GetRequiredService<ApplicationDbContextInitialiser>();
                // await initialiser.SeedAsync();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while migrating or seeding the database.");
            }
        }

        await host.RunAsync();
    }

    /// <summary>
    /// Tạo và cấu hình HostBuilder cho ứng dụng web.
    /// </summary>
    /// <param name="args">Các đối số dòng lệnh.</param>
    /// <returns>Một IHostBuilder đã được cấu hình.</returns>
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}

/// <summary>
/// Lớp Startup chịu trách nhiệm cấu hình các dịch vụ và pipeline yêu cầu của ứng dụng.
/// </summary>
public class Startup
{
    /// <summary>
    /// Cấu hình ứng dụng.
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// Khởi tạo một phiên bản mới của lớp Startup.
    /// </summary>
    /// <param name="configuration">Đối tượng cấu hình ứng dụng.</param>
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    /// <summary>
    /// Cấu hình các dịch vụ cho ứng dụng. Phương thức này được gọi bởi runtime.
    /// </summary>
    /// <param name="services">Bộ sưu tập dịch vụ để đăng ký.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCompositionRootServices(Configuration);
        services.AddWebServices(Configuration);

        services.AddControllers(options =>
        {
            options.InputFormatters.Insert(0, new HtmlInputFormatter());
        });
        services.AddLocalization();
        services.AddHttpClient();
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

    /// <summary>
    /// Cấu hình pipeline yêu cầu HTTP của ứng dụng. Phương thức này được gọi bởi runtime.
    /// </summary>
    /// <param name="app">Giao diện để cấu hình pipeline yêu cầu của ứng dụng.</param>
    /// <param name="env">Cung cấp thông tin về môi trường web nơi ứng dụng đang chạy.</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            // Initialise and seed database
            // Task.Run(async () =>
            // {
            //     using var scope = app.ApplicationServices.CreateScope();
            //     var initialiser = scope.ServiceProvider.GetRequiredService<backend.Infrastructure.Data.ApplicationDbContextInitialiser>();
            //     await initialiser.InitialiseAsync();
            //     await initialiser.SeedAsync();
            // });
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

        var supportedCultures = new[] { "en-US", "vi-VN" };
        var localizationOptions = new RequestLocalizationOptions()
            .SetDefaultCulture(supportedCultures[0])
            .AddSupportedCultures(supportedCultures)
            .AddSupportedUICultures(supportedCultures);

        app.UseRequestLocalization(localizationOptions);

        app.UseAuthentication();
        app.UseMiddleware<EnsureUserExistsMiddleware>();
        app.UseMiddleware<NovuSubscriberCreationMiddleware>();
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
