using backend.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder =>
        {
            builder.WithOrigins("http://localhost:5173") // Frontend development server URL
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

builder.AddWebServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Initialise and seed database
    // using (var scope = app.Services.CreateScope())
    // {
    //     var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
    //     await initialiser.InitialiseAsync();
    //     await initialiser.SeedAsync();
    // }
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Use CORS policy
app.UseCors("AllowFrontend");

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/api/specification.json";
});


app.UseExceptionHandler(options => { });

app.Map("/", () => Results.Redirect("/api"));

app.MapEndpoints();

app.Run();

public partial class Program { }
