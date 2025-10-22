using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MySqlConnector;
using Xunit;
using Respawn;
using Microsoft.AspNetCore.Identity;

public partial class Testing : IAsyncLifetime
{
    private static WebApplicationFactory<Program> _factory = null!;
    private static IConfiguration _configuration = null!;
    private static IServiceScopeFactory _scopeFactory = null!;
    private static Respawner _respawner = null!;
    private static int _checkpoint = 0;

    public async Task InitializeAsync()
    {
        _factory = new CustomWebApplicationFactory();
        _configuration = _factory.Services.GetRequiredService<IConfiguration>();
        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();

        var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")!);
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            TablesToIgnore = ["__EFMigrationsHistory"],
            DbAdapter = DbAdapter.MySql
        });
        await connection.CloseAsync();

        using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<backend.Infrastructure.Data.ApplicationDbContext>();
                await context.Database.MigrateAsync();
    }

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _scopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        return await mediator.Send(request);
    }

    public static async Task<string> RunAsDefaultUserAsync()
    {
        return await RunAsUserAsync("test@test.com", "Test@123", []);
    }

    public static async Task<string> RunAsAdministratorAsync()
    {
        return await RunAsUserAsync("administrator@test.com", "Administrator@123", ["Administrator"]);
    }

    public static async Task<string> RunAsUserAsync(string userName, string password, string[] roles)
    {
        using var scope = _scopeFactory.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var user = new IdentityUser { UserName = userName, Email = userName };

        var result = await userManager.CreateAsync(user, password);

        if (roles.Any())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }

            await userManager.AddToRolesAsync(user, roles);
        }

        if (result.Succeeded)
        {
            return user.Id;
        }

        var errors = string.Join("\n", result.Errors.Select(e => e.Description));

        throw new Exception($"Unable to create {userName}.{errors}");
    }

    public static async Task ResetState()
    {
        await _respawner.ResetAsync(_configuration.GetConnectionString("DefaultConnection")!);
        _checkpoint++;
    }

    public static async Task<TEntity?> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<backend.Infrastructure.Data.ApplicationDbContext>();

        return await context.FindAsync<TEntity>(keyValues);
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<backend.Infrastructure.Data.ApplicationDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }

    public static async Task<int> CountAsync<TEntity>() where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<backend.Infrastructure.Data.ApplicationDbContext>();

        return await context.Set<TEntity>().CountAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true);
            configurationBuilder.AddEnvironmentVariables();
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"ConnectionStrings:DefaultConnection", "Server=127.0.0.1;Port=3306;Database=familytree_db;Uid=root;Pwd=root_password;"}
            });
        });

        builder.UseEnvironment("Development");

        builder.ConfigureServices((builder, services) =>
        {
            services
                .RemoveAll<DbContextOptions<backend.Infrastructure.Data.ApplicationDbContext>>()
                .AddDbContext<backend.Infrastructure.Data.ApplicationDbContext>((sp, options) =>
                    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection")!,
                        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")!)));

            services.AddTransient<backend.Application.Common.Interfaces.IUser, MockUserService>();
        });
    }
}

public class MockUserService : backend.Application.Common.Interfaces.IUser
{
    public string? Id => "TestUserId";
    public List<string>? Roles => new List<string> { "Administrator" };
}
