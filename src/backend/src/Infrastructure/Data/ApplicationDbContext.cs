using System.Reflection;
using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Common;
using Microsoft.EntityFrameworkCore;
using backend.Infrastructure.Persistence.Extensions;

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
    /// Lấy hoặc thiết lập DbSet cho các thực thể User.
    /// </summary>
    public DbSet<User> Users => Set<User>();
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
    /// Lấy hoặc thiết lập DbSet cho các thực thể FileMetadata.
    /// </summary>
    public DbSet<FileMetadata> FileMetadata { get; set; } = null!;

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể FileUsage.
    /// </summary>
    public DbSet<FileUsage> FileUsages => Set<FileUsage>();

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể EventMember.
    /// </summary>
    public DbSet<EventMember> EventMembers => Set<EventMember>();

    //public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    //{
        // foreach (EntityEntry<BaseEntity> entry in ChangeTracker.Entries<BaseEntity>())
        // {
        //     switch (entry.State)
        //     {
        //         case EntityState.Added:
        //             if (entry.Entity is BaseAuditableEntity auditableEntity)
        //             {
        //                 auditableEntity.CreatedBy = _currentUser.Id?.ToString();
        //                 auditableEntity.Created = _dateTime.Now;
        //             }
        //             break;

        //         case EntityState.Modified:
        //             if (entry.Entity is BaseAuditableEntity auditableEntityModified)
        //             {
        //                 auditableEntityModified.LastModifiedBy = _currentUser.Id?.ToString();
        //                 auditableEntityModified.LastModified = _dateTime.Now;
        //             }
        //             break;

        //         case EntityState.Deleted:
        //             if (entry.Entity is ISoftDelete softDeleteEntity)
        //             {
        //                 softDeleteEntity.IsDeleted = true;
        //                 softDeleteEntity.DeletedBy = _currentUser.Id?.ToString();
        //                 softDeleteEntity.DeletedDate = _dateTime.Now;
        //                 entry.State = EntityState.Modified;
        //             }
        //             break;
        //     }
        // }

        // return await base.SaveChangesAsync(cancellationToken);
   // }

    /// <summary>
    /// Cấu hình mô hình được phát hiện bởi DbContext.
    /// </summary>
    /// <param name="builder">Đối tượng ModelBuilder được sử dụng để cấu hình mô hình.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Ignore<JsonDocument>();

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                entityType.AddSoftDeleteQueryFilter();
            }
        }

        base.OnModelCreating(builder);

        builder.ApplySnakeCaseNamingConvention();
    }
}

