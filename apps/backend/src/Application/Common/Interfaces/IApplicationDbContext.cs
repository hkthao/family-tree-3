using backend.Domain.Entities;

namespace backend.Application.Common.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho Application Database Context, cung cấp quyền truy cập vào các DbSet của ứng dụng.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể User.
    /// </summary>
    DbSet<User> Users { get; }
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể UserPushToken.
    /// </summary>
    DbSet<UserPushToken> UserPushTokens { get; }
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể Family.
    /// </summary>
    DbSet<Family> Families { get; }
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể Member.
    /// </summary>
    DbSet<Member> Members { get; }
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể Event.
    /// </summary>
    DbSet<Event> Events { get; }
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể Relationship.
    /// </summary>
    DbSet<Relationship> Relationships { get; }
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể UserProfile.
    /// </summary>
    DbSet<UserProfile> UserProfiles { get; }
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể FamilyUser.
    /// </summary>
    DbSet<FamilyUser> FamilyUsers { get; }
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể UserActivity.
    /// </summary>
    DbSet<UserActivity> UserActivities { get; }
    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể UserPreference.
    /// </summary>
    DbSet<UserPreference> UserPreferences { get; }

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể FamilyLocation.
    /// </summary>
    DbSet<FamilyLocation> FamilyLocations { get; }

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể Location.
    /// </summary>
    DbSet<Location> Locations { get; }


    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể EventMember.
    /// </summary>
    DbSet<EventMember> EventMembers { get; }
    DbSet<LocationLink> LocationLinks { get; }

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể PrivacyConfiguration.
    /// </summary>
    DbSet<PrivacyConfiguration> PrivacyConfigurations { get; }

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể FamilyLimitConfiguration.
    /// </summary>
    DbSet<FamilyLimitConfiguration> FamilyLimitConfigurations { get; }

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể FamilyDict.
    /// </summary>
    DbSet<FamilyDict> FamilyDicts { get; }

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể MemberFace.
    /// </summary>
    DbSet<MemberFace> MemberFaces { get; }

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể Prompt.
    /// </summary>
    DbSet<Prompt> Prompts { get; }

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể VoiceProfile.
    /// </summary>
    DbSet<VoiceProfile> VoiceProfiles { get; }

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể VoiceGeneration.
    /// </summary>
    DbSet<VoiceGeneration> VoiceGenerations { get; }



    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể FamilyLink.
    /// </summary>
    DbSet<FamilyLink> FamilyLinks { get; }

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể FamilyMedia.
    /// </summary>
    DbSet<FamilyMedia> FamilyMedia { get; }



    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể MediaLink.
    /// </summary>
    DbSet<MediaLink> MediaLinks { get; }

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể MemoryItem.
    /// </summary>
    DbSet<MemoryItem> MemoryItems { get; }

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể MemoryMedia.
    /// </summary>
    DbSet<MemoryMedia> MemoryMedia { get; }

    /// <summary>
    /// Lấy hoặc thiết lập DbSet cho các thực thể MemoryPerson.
    /// </summary>
    DbSet<MemoryPerson> MemoryPersons { get; }

    /// <summary>
    /// Lưu tất cả các thay đổi được thực hiện trong context vào cơ sở dữ liệu một cách không đồng bộ.
    /// </summary>
    /// <param name="cancellationToken">Token để hủy bỏ thao tác.</param>
    /// <returns>Số lượng thực thể đã được ghi vào cơ sở dữ liệu.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
