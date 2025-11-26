using backend.Application.Common.Interfaces;
using backend.Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.Data
{
    /// <summary>
    /// Factory để tạo ApplicationDbContext tại thời điểm thiết kế (design-time).
    /// Điều này cần thiết cho các công cụ EF Core như 'dotnet ef migrations add' hoặc 'dotnet ef database update'
    /// để có thể tạo một instance của DbContext mà không cần chạy toàn bộ ứng dụng.
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddUserSecrets("backend.Web")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            var services = new ServiceCollection();

            // Register necessary services for DbContext
            _ = services.AddSingleton(new LoggerFactory().CreateLogger<ApplicationDbContextFactory>());
            services.AddScoped<DispatchDomainEventsInterceptor>();
            services.AddScoped<AuditableEntitySaveChangesInterceptor>();
            services.AddScoped<ICurrentUser, DesignTimeUserService>();
            services.AddScoped<IDateTime, DesignTimeDateTimeService>();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationDbContext).Assembly));

            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
                options.AddInterceptors(
                    sp.GetRequiredService<DispatchDomainEventsInterceptor>(),
                    sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>());
            });

            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider.GetRequiredService<ApplicationDbContext>();
        }

        // Simple mock implementation of ICurrentUser for design-time
        private class DesignTimeUserService : ICurrentUser
        {
            public Guid UserId => Guid.Empty;
            public Guid? ProfileId => Guid.Empty;
            public string? Email => "design@time.com";
            public string? Name => "Design Time User";
            public bool IsAuthenticated => true;
            public List<string>? Roles => new List<string> { "Administrator" };
        }

        // Simple mock implementation of IDateTime for design-time
        private class DesignTimeDateTimeService : IDateTime
        {
            public DateTime Now => DateTime.UtcNow;
        }
    }
}
