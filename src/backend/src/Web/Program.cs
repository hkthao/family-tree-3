using backend.Application.SystemConfigurations.Commands.InitializeSystemConfigurations;
using backend.CompositionRoot;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCompositionRootServices(builder.Configuration);
builder.AddWebServices();

// Add controllers service
builder.Services.AddControllers();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policyBuilder =>
        {
            var corsOriginsString = builder.Configuration["CORS_ORIGINS"];
            var origins = new List<string>();
            if (!string.IsNullOrEmpty(corsOriginsString))
            {
                origins.AddRange(corsOriginsString.Split(',', StringSplitOptions.RemoveEmptyEntries));
            }

            if (origins.Any())
            {
                policyBuilder.WithOrigins(origins.ToArray())
                             .AllowAnyHeader()
                             .AllowAnyMethod();
            }
            else
            {
                Console.WriteLine("CORS_ORIGINS environment variable not found or is empty.");
            }
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Initialise and seed database
    using (var scope = app.Services.CreateScope())
    {
        var initialiser = scope.ServiceProvider.GetRequiredService<backend.Infrastructure.Data.ApplicationDbContextInitialiser>();
        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    await mediator.Send(new InitializeSystemConfigurationsCommand());
}

// Use CORS policy
app.UseCors("AllowFrontend");

app.UseHealthChecks("/health");
// app.UseHttpsRedirection(); // Disabled to allow HTTP access for local development

// Configure static files to exclude the 'uploads' folder
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.WebRootPath)),
    RequestPath = "",
    // Exclude the 'uploads' folder from direct static file serving
    // This requires a custom middleware or a more complex setup if 'uploads' is a subfolder of wwwroot
    // For simplicity, we'll assume 'uploads' is a direct subfolder of wwwroot and won't be served by this.
    // Access to 'uploads' will be via the /api/upload/preview/{fileName} endpoint.
});

app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/api/specification.json";
});

app.UseAuthentication(); // Add Authentication middleware
app.UseAuthorization();  // Add Authorization middleware

app.UseExceptionHandler(options => { });

app.Map("/", () => Results.Redirect("/api"));

// Map controllers
app.MapControllers();

app.Run();

public partial class Program { }

