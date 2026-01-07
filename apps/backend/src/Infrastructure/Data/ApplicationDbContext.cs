using System.Reflection;
using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Domain.Common;
using backend.Domain.Entities;
using backend.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
namespace backend.Infrastructure.Data;
/// <summary>
/// Đại diện cho cơ sở dữ liệu ứng dụng và là điểm truy cập chính để tương tác với dữ liệu.
/// </summary>
public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDomainEventDispatcher domainEventDispatcher,
    ICurrentUser currentUser,
    IDateTime dateTime) : DbContext(options), IApplicationDbContext
{
    private readonly IDomainEventDispatcher _domainEventDispatcher = domainEventDispatcher;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IDateTime _dateTime = dateTime;
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể Family.
    /// </summary>
    public DbSet<Family> Families => Set<Family>();
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể User.
    /// </summary>
    public DbSet<User> Users => Set<User>();
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể UserPushToken.
    /// </summary>
    public DbSet<UserPushToken> UserPushTokens => Set<UserPushToken>();
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
    /// Lấy hoặc thiết lập DbSet cho các thực thể FamilyFollow.
    /// </summary>
    public DbSet<FamilyFollow> FamilyFollows => Set<FamilyFollow>();
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể UserActivity.
    /// </summary>
    public DbSet<UserActivity> UserActivities => Set<UserActivity>();
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể UserPreference.
    /// </summary>
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể FamilyLocation.
    /// </summary>
    public DbSet<FamilyLocation> FamilyLocations => Set<FamilyLocation>();

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể Location.
    /// </summary>
    public DbSet<Location> Locations => Set<Location>();

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể EventMember.
    /// </summary>
    public DbSet<EventMember> EventMembers => Set<EventMember>();
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể PrivacyConfiguration.
    /// </summary>
    public DbSet<PrivacyConfiguration> PrivacyConfigurations => Set<PrivacyConfiguration>();

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể FamilyConfiguration.
    /// </summary>
    public DbSet<FamilyLimitConfiguration> FamilyLimitConfigurations => Set<FamilyLimitConfiguration>();
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể FamilyDict.
    /// </summary>
    public DbSet<FamilyDict> FamilyDicts => Set<FamilyDict>();
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể MemberFace.
    /// </summary>
    public DbSet<MemberFace> MemberFaces => Set<MemberFace>();

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể Prompt.
    /// </summary>
    public DbSet<Prompt> Prompts { get; set; } = null!;

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể VoiceProfile.
    /// </summary>
    public DbSet<VoiceProfile> VoiceProfiles => Set<VoiceProfile>();

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể VoiceGeneration.
    /// </summary>
    public DbSet<VoiceGeneration> VoiceGenerations => Set<VoiceGeneration>();



    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể FamilyLink.
    /// </summary>
    public DbSet<FamilyLink> FamilyLinks => Set<FamilyLink>();

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể FamilyMedia.
    /// </summary>
    public DbSet<FamilyMedia> FamilyMedia { get; set; } = null!;

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể MediaLink.
    /// </summary>
    public DbSet<MediaLink> MediaLinks { get; set; } = null!;

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể MemoryItem.
    /// </summary>
    public DbSet<MemoryItem> MemoryItems => Set<MemoryItem>();

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể MemoryMedia.
    /// </summary>
    public DbSet<MemoryMedia> MemoryMedia => Set<MemoryMedia>();

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể MemoryPerson.
    /// </summary>
    public DbSet<MemoryPerson> MemoryPersons => Set<MemoryPerson>();

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể LocationLink.
    /// </summary>
    public DbSet<LocationLink> LocationLinks => Set<LocationLink>();


    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Lấy tất cả các thực thể có sự kiện miền trước khi lưu thay đổi
        var entitiesWithDomainEvents = ChangeTracker
            .Entries<BaseEntity>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.Entity is BaseAuditableEntity auditableEntity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        auditableEntity.CreatedBy = _currentUser.UserId.ToString();
                        auditableEntity.Created = _dateTime.Now;
                        break;
                    case EntityState.Modified:
                        auditableEntity.LastModifiedBy = _currentUser.UserId.ToString();
                        auditableEntity.LastModified = _dateTime.Now;
                        break;
                }
            }
            // if (entry.State == EntityState.Deleted && entry.Entity is ISoftDelete softDeleteEntity)
            // {
            //     softDeleteEntity.IsDeleted = true;
            //     softDeleteEntity.DeletedBy = _currentUser.UserId.ToString();
            //     softDeleteEntity.DeletedDate = _dateTime.Now;
            //     entry.State = EntityState.Modified; // Chuyển trạng thái về Modified để EF Core không xóa vật lý
            // }
        }
        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            entry.Entity.ClearDomainEvents(); // Clear events AFTER dispatch
        }

        // Điều phối các sự kiện miền sau khi SaveChanges thành công
        await _domainEventDispatcher.DispatchEvents(entitiesWithDomainEvents);

        return result;
    }
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
        // Convert all DateTime properties to UTC
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(
                        new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                            (DateTime v) => v.ToUniversalTime(),
                            (DateTime v) => DateTime.SpecifyKind(v, DateTimeKind.Utc)));
                }
            }
        }
        builder.ApplySnakeCaseNamingConvention();
    }
}
