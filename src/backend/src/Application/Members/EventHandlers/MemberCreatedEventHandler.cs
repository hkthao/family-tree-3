using backend.Application.Common.Interfaces;
using backend.Application.Common.Helpers;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Enums;
using backend.Domain.Events.Members;
using Microsoft.Extensions.Logging;

namespace backend.Application.Members.EventHandlers;

public class MemberCreatedEventHandler(ILogger<MemberCreatedEventHandler> logger, IMediator mediator, IFamilyTreeService familyTreeService, ICurrentUser _user, IN8nService n8nService) : INotificationHandler<MemberCreatedEvent>
{
    private readonly ILogger<MemberCreatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly IFamilyTreeService _familyTreeService = familyTreeService;
    private readonly ICurrentUser _user = _user;
    private readonly IN8nService _n8nService = n8nService;

    public async Task Handle(MemberCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Member {MemberName} ({MemberId}) was successfully created.",
            notification.Member.FullName, notification.Member.Id);

        // Record activity for member creation
        await _mediator.Send(new RecordActivityCommand
        {
            UserId = _user.UserId,
            ActionType = UserActionType.CreateMember,
            TargetType = TargetType.Member,
            TargetId = notification.Member.Id.ToString(),
            ActivitySummary = $"Created member '{notification.Member.FullName}' in family '{notification.Member.FamilyId}'."
        }, cancellationToken);

        // Publish notification for member creation

        // Store member data in Vector DB for search via GlobalSearchService
        // Call n8n webhook for embedding update
        var (entityData, description) = EmbeddingDescriptionFactory.CreateMemberData(notification.Member);
        var embeddingDto = new EmbeddingWebhookDto
        {
            EntityType = "Member",
            EntityId = notification.Member.Id.ToString(),
            ActionType = "Created",
            EntityData = entityData,
            Description = description
        };
        await _n8nService.CallEmbeddingWebhookAsync(embeddingDto, cancellationToken);
    }
}
