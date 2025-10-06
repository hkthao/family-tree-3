using backend.Application;
using backend.Infrastructure;
using backend.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Read Auth0 configuration from environment variables
var auth0Domain = builder.Configuration["Auth0:Domain"];

// Configure Auth0 Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = auth0Domain;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false, // Set to false if no specific audience is configured
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Example policies, adjust as needed
    options.AddPolicy("read:messages", policy => policy.RequireClaim("permissions", "read:messages"));
    options.AddPolicy("write:messages", policy => policy.RequireClaim("permissions", "write:messages"));
});

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder =>
        {
            builder.WithOrigins("http://localhost:5173", "https://localhost:5173", "https://localhost:5001") // Frontend development server URL
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

builder.AddWebServices();

// Add controllers service
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Initialise and seed database
    using (var scope = app.Services.CreateScope())
    {
        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
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

app.UseAuthentication(); // Add Authentication middleware
app.UseAuthorization();  // Add Authorization middleware

app.UseExceptionHandler(options => { });

app.Map("/", () => Results.Redirect("/api"));

// app.MapEndpoints(); // Removed this line

// Map controllers
app.MapControllers();

app.Run();

public partial class Program { }
