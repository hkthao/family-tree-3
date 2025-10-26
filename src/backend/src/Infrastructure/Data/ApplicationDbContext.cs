using System.Reflection;
using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Domain.Common;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Data;

/// <summary>
/// Đại diện cho cơ sở dữ liệu ứng dụng và là điểm truy cập chính để tương tác với dữ liệu.
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IApplicationDbContext
{
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể Family.
    /// </summary>
    public DbSet<Family> Families => Set<Family>();
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể Member.
    /// </summary>
    public DbSet<Member> Members => Set<Member>();
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể Event.
    /// </summary>
    public DbSet<Event> Events => Set<Event>();
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể Relationship.
    /// </summary>
    public DbSet<Relationship> Relationships => Set<Relationship>();
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể UserProfile.
    /// </summary>
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể FamilyUser.
    /// </summary>
    public DbSet<FamilyUser> FamilyUsers => Set<FamilyUser>();
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể UserActivity.
    /// </summary>
    public DbSet<UserActivity> UserActivities => Set<UserActivity>();
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể UserPreference.
    /// </summary>
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể Notification.
    /// </summary>
    public DbSet<Notification> Notifications => Set<Notification>();
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể NotificationPreference.
    /// </summary>
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể NotificationTemplate.
    /// </summary>
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể FileMetadata.
    /// </summary>
    public DbSet<FileMetadata> FileMetadata { get; set; } = null!;

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể EventMember.
    /// </summary>
    public DbSet<EventMember> EventMembers => Set<EventMember>();


    /// <summary>
    /// Cấu hình mô hình được phát hiện bởi DbContext.
    /// </summary>
    /// <param name="builder">Đối tượng ModelBuilder được sử dụng để cấu hình mô hình.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Ignore<JsonDocument>();

        base.OnModelCreating(builder);
    }
}
