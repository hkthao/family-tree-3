using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Events.Members;
using MediatR;
using Microsoft.Extensions.Logging;
using backend.Domain.Enums;

namespace backend.Application.Members.EventHandlers;

public class MemberBiographyUpdatedEventHandler(ILogger<MemberBiographyUpdatedEventHandler> logger, IMediator mediator, INotificationService notificationService, IGlobalSearchService globalSearchService) : INotificationHandler<MemberBiographyUpdatedEvent>
{
    private readonly ILogger<MemberBiographyUpdatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IGlobalSearchService _globalSearchService = globalSearchService;

    public async Task Handle(MemberBiographyUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Member {MemberName} ({MemberId}) biography was successfully updated.",
            notification.Member.FullName, notification.Member.Id);

        // Record activity for member biography update
        await _mediator.Send(new RecordActivityCommand
        {
            // UserProfileId will be determined by the RecordActivityCommand handler based on the current user
            ActionType = UserActionType.UpdateMember,
            TargetType = TargetType.Member,
            TargetId = notification.Member.Id.ToString(),
            ActivitySummary = $"Updated biography for member '{notification.Member.FullName}'."
        }, cancellationToken);

        // Send notification for member biography update
        await _notificationService.SendNotification(new NotificationMessage
        {
            RecipientUserId = notification.Member.LastModifiedBy?.ToString() ?? "UnknownUser", // Assuming LastModifiedBy is the recipient
            Title = "Member Biography Updated",
            Body = $"The biography for member '{notification.Member.FullName}' has been updated.",
            Data = new Dictionary<string, string>
            {
                { "MemberId", notification.Member.Id.ToString() },
                { "MemberName", notification.Member.FullName },
                { "FamilyId", notification.Member.FamilyId.ToString() }
            },
            DeepLink = $"/families/{notification.Member.FamilyId}/members/{notification.Member.Id}" // Example deep link
        }, cancellationToken);

        // Update member data in Vector DB for search via GlobalSearchService
        await _globalSearchService.UpsertEntityAsync(
            notification.Member,
            "Member",
            member => $"Member Name: {member.FullName}. Biography: {member.Biography}. Occupation: {member.Occupation}. Place of birth: {member.PlaceOfBirth}.",
            member => new Dictionary<string, string>
            {
                { "EntityType", "Member" },
                { "EntityId", member.Id.ToString() },
                { "Name", member.FullName },
                { "Description", member.Biography ?? "" },
                { "DeepLink", $"/families/{member.FamilyId}/members/{member.Id}" }
            },
            cancellationToken
        );
    }
}
