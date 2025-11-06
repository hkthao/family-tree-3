using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using McpServer.Config; // For app settings classes
using McpServer.Services; // For AiService, IAiProvider, AiProviderFactory, and concrete providers
using Microsoft.AspNetCore.Authorization; // For [Authorize] attribute
using System.Net.Http.Json; // For PostAsJsonAsync in providers

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure App Settings
builder.Services.Configure<FamilyTreeBackendSettings>(builder.Configuration.GetSection("FamilyTreeBackend"));
builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("Gemini"));
builder.Services.Configure<OpenAiSettings>(builder.Configuration.GetSection("OpenAI")); // New
builder.Services.Configure<LocalLlmSettings>(builder.Configuration.GetSection("LocalLLM")); // New
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings")); // New

builder.Services.AddControllers();

const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      builder =>
                      {
                          builder.WithOrigins("http://localhost", "http://localhost:5173") // Allow requests from your frontend
                                 .AllowAnyHeader()
                                 .AllowAnyMethod();
                      });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MCP Server API", Version = "v1" });

    // Configure JWT authentication for Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Authorization: Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
        if (jwtSettings == null)
        {
            throw new InvalidOperationException("JwtSettings are not configured.");
        }

        options.Authority = $"https://{jwtSettings.Authority}";
        options.Audience = jwtSettings.Audience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://{jwtSettings.Authority}",
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization();

// Register HttpClient for calling Family Tree Backend
builder.Services.AddHttpClient<FamilyTreeBackendService>(client =>
{
    var familyTreeBackendSettings = builder.Configuration.GetSection("FamilyTreeBackend").Get<FamilyTreeBackendSettings>();
    if (familyTreeBackendSettings == null || string.IsNullOrEmpty(familyTreeBackendSettings.BaseUrl))
    {
        throw new InvalidOperationException("FamilyTreeBackend:BaseUrl is not configured.");
    }
    client.BaseAddress = new Uri(familyTreeBackendSettings.BaseUrl);
});

// Register concrete AI Providers
builder.Services.AddHttpClient<GeminiProvider>(); // GeminiProvider uses HttpClient
builder.Services.AddHttpClient<OpenAiProvider>(); // OpenAIProvider uses HttpClient
builder.Services.AddHttpClient<LocalLlmProvider>(client =>
{
    var localLlmSettings = builder.Configuration.GetSection("LocalLLM").Get<LocalLlmSettings>();
    if (localLlmSettings == null || string.IsNullOrEmpty(localLlmSettings.BaseUrl))
    {
        throw new InvalidOperationException("LocalLLM:BaseUrl is not configured.");
    }
    client.BaseAddress = new Uri(localLlmSettings.BaseUrl);
}); // LocalLlmProvider uses HttpClient

// Register AI Provider Factory
builder.Services.AddSingleton<AiProviderFactory>();

// Register ToolExecutor
builder.Services.AddScoped<ToolExecutor>();

// Register IAiProvider using the factory
builder.Services.AddScoped<IAiProvider>(sp =>
{
    var factory = sp.GetRequiredService<AiProviderFactory>();
    var configuration = sp.GetRequiredService<IConfiguration>();
    var defaultProvider = configuration["DefaultAiProvider"] ?? "Gemini";
    return factory.GetProvider(defaultProvider);
});

// Register ToolInteractionHandler
builder.Services.AddScoped<ToolInteractionHandler>();

// Register main AiService
builder.Services.AddScoped<AiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => {});
}

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication(); // Use authentication middleware
app.UseAuthorization(); // Use authorization middleware

app.MapControllers();

app.Run();
