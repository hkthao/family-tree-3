using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Events.Members;
using MediatR;
using Microsoft.Extensions.Logging;
using backend.Domain.Enums;

namespace backend.Application.Members.EventHandlers;

public class MemberCreatedEventHandler(ILogger<MemberCreatedEventHandler> logger, IMediator mediator, INotificationService notificationService, IGlobalSearchService globalSearchService) : INotificationHandler<MemberCreatedEvent>
{
    private readonly ILogger<MemberCreatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IGlobalSearchService _globalSearchService = globalSearchService;

    public async Task Handle(MemberCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Member {MemberName} ({MemberId}) was successfully created.",
            notification.Member.FullName, notification.Member.Id);

        // Record activity for member creation
        await _mediator.Send(new RecordActivityCommand
        {
            // UserProfileId will be determined by the RecordActivityCommand handler based on the current user
            ActionType = UserActionType.CreateMember,
            TargetType = TargetType.Member,
            TargetId = notification.Member.Id.ToString(),
            ActivitySummary = $"Created member '{notification.Member.FullName}' in family '{notification.Member.FamilyId}'."
        }, cancellationToken);

        // Send notification for member creation
        await _notificationService.SendNotification(new NotificationMessage
        {
            RecipientUserId = notification.Member.CreatedBy!, // Assuming CreatedBy is the recipient
            Title = "Member Created",
            Body = $"A new member '{notification.Member.FullName}' has been added to family '{notification.Member.FamilyId}'.",
            Data = new Dictionary<string, string>
            {
                { "MemberId", notification.Member.Id.ToString() },
                { "MemberName", notification.Member.FullName },
                { "FamilyId", notification.Member.FamilyId.ToString() }
            },
            DeepLink = $"/families/{notification.Member.FamilyId}/members/{notification.Member.Id}" // Example deep link
        }, cancellationToken);

        // Store member data in Vector DB for search via GlobalSearchService
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
