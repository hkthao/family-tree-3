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

    public DomainEventNotificationPublisher(
        ILogger<DomainEventNotificationPublisher> logger,
        INotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
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
            case NewFamilyMemberAddedEvent newFamilyMemberAddedEvent:
                await HandleNewFamilyMemberAddedEvent(newFamilyMemberAddedEvent, cancellationToken);
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
        await _notificationService.SendNotificationAsync(
            NotificationType.FamilyCreated,
            NotificationChannel.InApp,
            new Dictionary<string, string>
            {
                { "FamilyName", domainEvent.Family.Name },
                { "DeepLink", $"/families/{domainEvent.Family.Id}" }
            },
            domainEvent.Family.CreatedBy!,
            domainEvent.Family.Id,
            cancellationToken: cancellationToken
        );
    }

    private async Task HandleFamilyUpdatedEvent(FamilyUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        await _notificationService.SendNotificationAsync(
            NotificationType.FamilyUpdated,
            NotificationChannel.InApp,
            new Dictionary<string, string>
            {
                { "FamilyName", domainEvent.Family.Name },
                { "DeepLink", $"/families/{domainEvent.Family.Id}" }
            },
            domainEvent.Family.LastModifiedBy!,
            domainEvent.Family.Id,
            cancellationToken: cancellationToken
        );
    }

    private async Task HandleFamilyDeletedEvent(FamilyDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        await _notificationService.SendNotificationAsync(
            NotificationType.FamilyDeleted,
            NotificationChannel.InApp,
            new Dictionary<string, string>
            {
                { "FamilyName", domainEvent.Family.Name }
            },
            domainEvent.Family.LastModifiedBy!, // Assuming the deleter is the recipient
            cancellationToken: cancellationToken
        );
    }

    private async Task HandleMemberCreatedEvent(MemberCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        await _notificationService.SendNotificationAsync(
            NotificationType.MemberCreated,
            NotificationChannel.InApp,
            new Dictionary<string, string>
            {
                { "MemberName", $"{domainEvent.Member.FirstName} {domainEvent.Member.LastName}" },
                { "FamilyName", domainEvent.Member.Family?.Name ?? "" },
                { "DeepLink", $"/member/{domainEvent.Member.Id}" }
            },
            domainEvent.Member.CreatedBy!,
            domainEvent.Member.FamilyId,
            cancellationToken: cancellationToken
        );
    }

    private async Task HandleMemberUpdatedEvent(MemberUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        await _notificationService.SendNotificationAsync(
            NotificationType.MemberUpdated,
            NotificationChannel.InApp,
            new Dictionary<string, string>
            {
                { "MemberName", $"{domainEvent.Member.FirstName} {domainEvent.Member.LastName}" },
                { "FamilyName", domainEvent.Member.Family?.Name ?? "" },
                { "DeepLink", $"/member/{domainEvent.Member.Id}" }
            },
            domainEvent.Member.LastModifiedBy!,
            domainEvent.Member.FamilyId,
            cancellationToken: cancellationToken
        );
    }

    private async Task HandleMemberDeletedEvent(MemberDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        await _notificationService.SendNotificationAsync(
            NotificationType.MemberDeleted,
            NotificationChannel.InApp,
            new Dictionary<string, string>
            {
                { "MemberName", $"{domainEvent.Member.FirstName} {domainEvent.Member.LastName}" },
                { "FamilyName", domainEvent.Member.Family?.Name ?? "" }
            },
            domainEvent.Member.LastModifiedBy!, // Assuming the deleter is the recipient
            cancellationToken: cancellationToken
        );
    }

    private async Task HandleMemberBiographyUpdatedEvent(MemberBiographyUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        await _notificationService.SendNotificationAsync(
            NotificationType.MemberBiographyUpdated,
            NotificationChannel.InApp,
            new Dictionary<string, string>
            {
                { "MemberName", $"{domainEvent.Member.FirstName} {domainEvent.Member.LastName}" },
                { "FamilyName", domainEvent.Member.Family?.Name ?? "" },
                { "DeepLink", $"/member/{domainEvent.Member.Id}" }
            },
            domainEvent.Member.LastModifiedBy!,
            domainEvent.Member.FamilyId,
            cancellationToken: cancellationToken
        );
    }

    private async Task HandleNewFamilyMemberAddedEvent(NewFamilyMemberAddedEvent domainEvent, CancellationToken cancellationToken)
    {
        await _notificationService.SendNotificationAsync(
            NotificationType.NewFamilyMember,
            NotificationChannel.InApp,
            new Dictionary<string, string>
            {
                { "NewMemberName", $"{domainEvent.NewMember.FirstName} {domainEvent.NewMember.LastName}" },
                { "FamilyName", domainEvent.NewMember.Family?.Name ?? "" },
                { "DeepLink", $"/member/{domainEvent.NewMember.Id}" }
            },
            domainEvent.NewMember.CreatedBy!,
            domainEvent.NewMember.FamilyId,
            cancellationToken: cancellationToken
        );
    }

    private async Task HandleRelationshipCreatedEvent(RelationshipCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        await _notificationService.SendNotificationAsync(
            NotificationType.RelationshipCreated,
            NotificationChannel.InApp,
            new Dictionary<string, string>
            {
                { "SourceMemberName", $"{domainEvent.Relationship.SourceMember?.FirstName} {domainEvent.Relationship.SourceMember?.LastName}" },
                { "TargetMemberName", $"{domainEvent.Relationship.TargetMember?.FirstName} {domainEvent.Relationship.TargetMember?.LastName}" },
                { "RelationshipType", domainEvent.Relationship.Type.ToString() },
                { "FamilyName", domainEvent.Relationship.Family?.Name ?? "" }
            },
            domainEvent.Relationship.CreatedBy!,
            domainEvent.Relationship.FamilyId,
            cancellationToken: cancellationToken
        );
    }

    private async Task HandleRelationshipUpdatedEvent(RelationshipUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        await _notificationService.SendNotificationAsync(
            NotificationType.RelationshipUpdated,
            NotificationChannel.InApp,
            new Dictionary<string, string>
            {
                { "SourceMemberName", $"{domainEvent.Relationship.SourceMember?.FirstName} {domainEvent.Relationship.SourceMember?.LastName}" },
                { "TargetMemberName", $"{domainEvent.Relationship.TargetMember?.FirstName} {domainEvent.Relationship.TargetMember?.LastName}" },
                { "RelationshipType", domainEvent.Relationship.Type.ToString() },
                { "FamilyName", domainEvent.Relationship.Family?.Name ?? "" }
            },
            domainEvent.Relationship.LastModifiedBy!,
            domainEvent.Relationship.FamilyId,
            cancellationToken: cancellationToken
        );
    }

    private async Task HandleRelationshipDeletedEvent(RelationshipDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        await _notificationService.SendNotificationAsync(
            NotificationType.RelationshipDeleted,
            NotificationChannel.InApp,
            new Dictionary<string, string>
            {
                { "SourceMemberName", $"{domainEvent.Relationship.SourceMember?.FirstName} {domainEvent.Relationship.SourceMember?.LastName}" },
                { "TargetMemberName", $"{domainEvent.Relationship.TargetMember?.FirstName} {domainEvent.Relationship.TargetMember?.LastName}" },
                { "RelationshipType", domainEvent.Relationship.Type.ToString() },
                { "FamilyName", domainEvent.Relationship.Family?.Name ?? "" }
            },
            domainEvent.Relationship.LastModifiedBy!,
            domainEvent.Relationship.FamilyId,
            cancellationToken: cancellationToken
        );
    }
}
