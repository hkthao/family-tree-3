using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using McpServer.Config; // For app settings classes
using McpServer.Services; // For AiService and FamilyTreeBackendService
using Microsoft.AspNetCore.Authorization; // For [Authorize] attribute

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure App Settings
builder.Services.Configure<FamilyTreeBackendSettings>(builder.Configuration.GetSection("FamilyTreeBackend"));
builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("Gemini"));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MCP Server API", Version = "v1" });

    // Define the security scheme
    var securityScheme = new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Authorization: Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    };

    // Add the security definition
    c.AddSecurityDefinition("Bearer", securityScheme);

    // Define the security requirement
    var securityRequirement = new OpenApiSecurityRequirement
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
    };

    // Add the security requirement
    c.AddSecurityRequirement(securityRequirement);
});

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var familyTreeBackendSettings = builder.Configuration.GetSection("FamilyTreeBackend").Get<FamilyTreeBackendSettings>();
        if (familyTreeBackendSettings == null)
        {
            throw new InvalidOperationException("FamilyTreeBackend settings are not configured.");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = familyTreeBackendSettings.JwtIssuer,
            ValidAudience = familyTreeBackendSettings.JwtAudience,
            // For a real-world scenario with asymmetric keys, you'd need to fetch the public key
            // or configure options.MetadataAddress or options.ConfigurationManager.
            // For this example, we're assuming a symmetric key for simplicity.
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(familyTreeBackendSettings.JwtSecretKey))
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

// Register AI Service
builder.Services.AddScoped<AiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // Use authentication middleware
app.UseAuthorization(); // Use authorization middleware

app.MapControllers();

app.Run();