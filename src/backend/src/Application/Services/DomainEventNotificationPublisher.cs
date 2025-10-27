using backend.Application.Common.Interfaces;
using backend.Domain.Common;
using backend.Domain.Events;
using backend.Domain.Events.Families;
using backend.Domain.Events.Members;
using backend.Domain.Events.Relationships;
using backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace backend.Application.Services;

/// <summary>
/// Dịch vụ xuất bản thông báo dựa trên các Domain Event, ánh xạ Domain Event thành các thông báo cụ thể.
/// </summary>
public class DomainEventNotificationPublisher : IDomainEventNotificationPublisher
{
    private readonly ILogger<DomainEventNotificationPublisher> _logger;
    private readonly INotificationService _notificationService;
    private readonly IApplicationDbContext _context;

    public DomainEventNotificationPublisher(
        ILogger<DomainEventNotificationPublisher> logger,
        INotificationService notificationService,
        IApplicationDbContext context)
    {
        _logger = logger;
        _notificationService = notificationService;
        _context = context;
    }

    /// <summary>
    /// Xuất bản một thông báo dựa trên Domain Event đã cho.
    /// </summary>
    /// <param name="domainEvent">Domain Event cần xử lý.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    public async Task PublishNotificationForEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Publishing notification for domain event: {DomainEventType}", domainEvent.GetType().Name);

        switch (domainEvent)
        {
            case FamilyCreatedEvent familyCreatedEvent:
                await HandleFamilyCreatedEvent(familyCreatedEvent, cancellationToken);
                break;
            case FamilyUpdatedEvent familyUpdatedEvent:
                await HandleFamilyUpdatedEvent(familyUpdatedEvent, cancellationToken);
                break;
            case FamilyDeletedEvent familyDeletedEvent:
                await HandleFamilyDeletedEvent(familyDeletedEvent, cancellationToken);
                break;
            case MemberCreatedEvent memberCreatedEvent:
                await HandleMemberCreatedEvent(memberCreatedEvent, cancellationToken);
                break;
            case MemberUpdatedEvent memberUpdatedEvent:
                await HandleMemberUpdatedEvent(memberUpdatedEvent, cancellationToken);
                break;
            case MemberDeletedEvent memberDeletedEvent:
                await HandleMemberDeletedEvent(memberDeletedEvent, cancellationToken);
                break;
            case MemberBiographyUpdatedEvent memberBiographyUpdatedEvent:
                await HandleMemberBiographyUpdatedEvent(memberBiographyUpdatedEvent, cancellationToken);
                break;
            case RelationshipCreatedEvent relationshipCreatedEvent:
                await HandleRelationshipCreatedEvent(relationshipCreatedEvent, cancellationToken);
                break;
            case RelationshipUpdatedEvent relationshipUpdatedEvent:
                await HandleRelationshipUpdatedEvent(relationshipUpdatedEvent, cancellationToken);
                break;
            case RelationshipDeletedEvent relationshipDeletedEvent:
                await HandleRelationshipDeletedEvent(relationshipDeletedEvent, cancellationToken);
                break;
            // Thêm các trường hợp khác cho các Domain Event khác
            default:
                _logger.LogWarning("No notification mapping found for domain event type: {DomainEventType}", domainEvent.GetType().Name);
                break;
        }
    }

    private async Task HandleFamilyCreatedEvent(FamilyCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        // Resolve UserProfileId from CreatedBy (ExternalId)
        Guid? recipientUserProfileId = null;
        if (!string.IsNullOrEmpty(domainEvent.Family.CreatedBy))
        {
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.ExternalId == domainEvent.Family.CreatedBy, cancellationToken);
            recipientUserProfileId = userProfile?.Id;
        }

        // await _notificationService.SendNotificationAsync(
        //     NotificationType.FamilyCreated,
        //     NotificationChannel.InApp,
        //     new Dictionary<string, string>
        //     {
        //         { "FamilyName", domainEvent.Family.Name },
        //         { "DeepLink", $"/family/{domainEvent.Family.Id}" }
        //     },
        //     recipientUserProfileId,
        //     domainEvent.Family.Id,
        //     cancellationToken: cancellationToken
        // );
    }

    private async Task HandleFamilyUpdatedEvent(FamilyUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        // Resolve UserProfileId from LastModifiedBy (ExternalId)
        Guid? recipientUserProfileId = null;
        if (!string.IsNullOrEmpty(domainEvent.Family.LastModifiedBy))
        {
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.ExternalId == domainEvent.Family.LastModifiedBy, cancellationToken);
            recipientUserProfileId = userProfile?.Id;
        }

        // await _notificationService.SendNotificationAsync(
        //     NotificationType.FamilyUpdated,
        //     NotificationChannel.InApp,
        //     new Dictionary<string, string>
        //     {
        //         { "FamilyName", domainEvent.Family.Name },
        //         { "DeepLink", $"/family/{domainEvent.Family.Id}" }
        //     },
        //     recipientUserProfileId,
        //     domainEvent.Family.Id,
        //     cancellationToken: cancellationToken
        // );
    }

    private async Task HandleFamilyDeletedEvent(FamilyDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        // Resolve UserProfileId from LastModifiedBy (ExternalId)
        Guid? recipientUserProfileId = null;
        if (!string.IsNullOrEmpty(domainEvent.Family.LastModifiedBy))
        {
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.ExternalId == domainEvent.Family.LastModifiedBy, cancellationToken);
            recipientUserProfileId = userProfile?.Id;
        }

        // await _notificationService.SendNotificationAsync(
        //     NotificationType.FamilyDeleted,
        //     NotificationChannel.InApp,
        //     new Dictionary<string, string>
        //     {
        //         { "FamilyName", domainEvent.Family.Name }
        //     },
        //     recipientUserProfileId, // Assuming the deleter is the recipient
        //     cancellationToken: cancellationToken
        // );
    }

    private async Task HandleMemberCreatedEvent(MemberCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        // Resolve UserProfileId from CreatedBy (ExternalId)
        Guid? recipientUserProfileId = null;
        if (!string.IsNullOrEmpty(domainEvent.Member.CreatedBy))
        {
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.ExternalId == domainEvent.Member.CreatedBy, cancellationToken);
            recipientUserProfileId = userProfile?.Id;
        }

        // await _notificationService.SendNotificationAsync(
        //     NotificationType.MemberCreated,
        //     NotificationChannel.InApp,
        //     new Dictionary<string, string>
        //     {
        //         { "MemberName", $"{domainEvent.Member.FirstName} {domainEvent.Member.LastName}" },
        //         { "FamilyName", domainEvent.Member.Family?.Name ?? "" },
        //         { "DeepLink", $"/member/{domainEvent.Member.Id}" }
        //     },
        //     recipientUserProfileId,
        //     domainEvent.Member.FamilyId,
        //     cancellationToken: cancellationToken
        // );
    }

    private async Task HandleMemberUpdatedEvent(MemberUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        // Resolve UserProfileId from LastModifiedBy (ExternalId)
        Guid? recipientUserProfileId = null;
        if (!string.IsNullOrEmpty(domainEvent.Member.LastModifiedBy))
        {
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.ExternalId == domainEvent.Member.LastModifiedBy, cancellationToken);
            recipientUserProfileId = userProfile?.Id;
        }

        // await _notificationService.SendNotificationAsync(
        //     NotificationType.MemberUpdated,
        //     NotificationChannel.InApp,
        //     new Dictionary<string, string>
        //     {
        //         { "MemberName", $"{domainEvent.Member.FirstName} {domainEvent.Member.LastName}" },
        //         { "FamilyName", domainEvent.Member.Family?.Name ?? "" },
        //         { "DeepLink", $"/member/{domainEvent.Member.Id}" }
        //     },
        //     recipientUserProfileId,
        //     domainEvent.Member.FamilyId,
        //     cancellationToken: cancellationToken
        // );
    }

    private async Task HandleMemberDeletedEvent(MemberDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        // Resolve UserProfileId from LastModifiedBy (ExternalId)
        Guid? recipientUserProfileId = null;
        if (!string.IsNullOrEmpty(domainEvent.Member.LastModifiedBy))
        {
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.ExternalId == domainEvent.Member.LastModifiedBy, cancellationToken);
            recipientUserProfileId = userProfile?.Id;
        }

        // await _notificationService.SendNotificationAsync(
        //     NotificationType.MemberDeleted,
        //     NotificationChannel.InApp,
        //     new Dictionary<string, string>
        //     {
        //         { "MemberName", $"{domainEvent.Member.FirstName} {domainEvent.Member.LastName}" },
        //         { "FamilyName", domainEvent.Member.Family?.Name ?? "" }
        //     },
        //     recipientUserProfileId, // Assuming the deleter is the recipient
        //     cancellationToken: cancellationToken
        // );
    }

    private async Task HandleMemberBiographyUpdatedEvent(MemberBiographyUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        // Resolve UserProfileId from LastModifiedBy (ExternalId)
        Guid? recipientUserProfileId = null;
        if (!string.IsNullOrEmpty(domainEvent.Member.LastModifiedBy))
        {
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.ExternalId == domainEvent.Member.LastModifiedBy, cancellationToken);
            recipientUserProfileId = userProfile?.Id;
        }

        // await _notificationService.SendNotificationAsync(
        //     NotificationType.MemberBiographyUpdated,
        //     NotificationChannel.InApp,
        //     new Dictionary<string, string>
        //     {
        //         { "MemberName", $"{domainEvent.Member.FirstName} {domainEvent.Member.LastName}" },
        //         { "FamilyName", domainEvent.Member.Family?.Name ?? "" },
        //         { "DeepLink", $"/member/{domainEvent.Member.Id}" }
        //     },
        //     recipientUserProfileId,
        //     domainEvent.Member.FamilyId,
        //     cancellationToken: cancellationToken
        // );
    }
    private async Task HandleRelationshipCreatedEvent(RelationshipCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        // Resolve UserProfileId from CreatedBy (ExternalId)
        Guid? recipientUserProfileId = null;
        if (!string.IsNullOrEmpty(domainEvent.Relationship.CreatedBy))
        {
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.ExternalId == domainEvent.Relationship.CreatedBy, cancellationToken);
            recipientUserProfileId = userProfile?.Id;
        }

        // await _notificationService.SendNotificationAsync(
        //     NotificationType.RelationshipCreated,
        //     NotificationChannel.InApp,
        //     new Dictionary<string, string>
        //     {
        //         { "SourceMemberName", $"{domainEvent.Relationship.SourceMember?.FirstName} {domainEvent.Relationship.SourceMember?.LastName}" },
        //         { "TargetMemberName", $"{domainEvent.Relationship.TargetMember?.FirstName} {domainEvent.Relationship.TargetMember?.LastName}" },
        //         { "RelationshipType", domainEvent.Relationship.Type.ToString() },
        //         { "FamilyName", domainEvent.Relationship.Family?.Name ?? "" }
        //     },
        //     recipientUserProfileId,
        //     domainEvent.Relationship.FamilyId,
        //     cancellationToken: cancellationToken
        // );
    }

    private async Task HandleRelationshipUpdatedEvent(RelationshipUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        // Resolve UserProfileId from LastModifiedBy (ExternalId)
        Guid? recipientUserProfileId = null;
        if (!string.IsNullOrEmpty(domainEvent.Relationship.LastModifiedBy))
        {
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.ExternalId == domainEvent.Relationship.LastModifiedBy, cancellationToken);
            recipientUserProfileId = userProfile?.Id;
        }

        // await _notificationService.SendNotificationAsync(
        //     NotificationType.RelationshipUpdated,
        //     NotificationChannel.InApp,
        //     new Dictionary<string, string>
        //     {
        //         { "SourceMemberName", $"{domainEvent.Relationship.SourceMember?.FirstName} {domainEvent.Relationship.SourceMember?.LastName}" },
        //         { "TargetMemberName", $"{domainEvent.Relationship.TargetMember?.FirstName} {domainEvent.Relationship.TargetMember?.LastName}" },
        //         { "RelationshipType", domainEvent.Relationship.Type.ToString() },
        //         { "FamilyName", domainEvent.Relationship.Family?.Name ?? "" }
        //     },
        //     recipientUserProfileId,
        //     domainEvent.Relationship.FamilyId,
        //     cancellationToken: cancellationToken
        // );
    }

    private async Task HandleRelationshipDeletedEvent(RelationshipDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        // Resolve UserProfileId from LastModifiedBy (ExternalId)
        Guid? recipientUserProfileId = null;
        if (!string.IsNullOrEmpty(domainEvent.Relationship.LastModifiedBy))
        {
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.ExternalId == domainEvent.Relationship.LastModifiedBy, cancellationToken);
            recipientUserProfileId = userProfile?.Id;
        }

        // await _notificationService.SendNotificationAsync(
        //     NotificationType.RelationshipDeleted,
        //     NotificationChannel.InApp,
        //     new Dictionary<string, string>
        //     {
        //         { "SourceMemberName", $"{domainEvent.Relationship.SourceMember?.FirstName} {domainEvent.Relationship.SourceMember?.LastName}" },
        //         { "TargetMemberName", $"{domainEvent.Relationship.TargetMember?.FirstName} {domainEvent.Relationship.TargetMember?.LastName}" },
        //         { "RelationshipType", domainEvent.Relationship.Type.ToString() },
        //         { "FamilyName", domainEvent.Relationship.Family?.Name ?? "" }
        //     },

        //     recipientUserProfileId,
        //     domainEvent.Relationship.FamilyId,
        //     cancellationToken: cancellationToken
        // );
    }
}
