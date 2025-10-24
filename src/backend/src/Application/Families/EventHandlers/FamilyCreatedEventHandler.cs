using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Events.Families;
using MediatR;
using Microsoft.Extensions.Logging;
using backend.Domain.Enums;
using backend.Application.AI.VectorStore;
using backend.Application.AI.Embeddings;

namespace backend.Application.Families.EventHandlers;

public class FamilyCreatedEventHandler(ILogger<FamilyCreatedEventHandler> logger, IMediator mediator, INotificationService notificationService, IVectorStoreFactory vectorStoreFactory, IEmbeddingProviderFactory embeddingProviderFactory) : INotificationHandler<FamilyCreatedEvent>
{
    private readonly ILogger<FamilyCreatedEventHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IVectorStoreFactory _vectorStoreFactory = vectorStoreFactory;
    private readonly IEmbeddingProviderFactory _embeddingProviderFactory = embeddingProviderFactory;

    public async Task Handle(FamilyCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Family Tree Domain Event: {DomainEvent}", notification.GetType().Name);

        _logger.LogInformation("Family {FamilyName} ({FamilyId}) was successfully created.",
            notification.Family.Name, notification.Family.Id);

        // Record activity for family creation
        await _mediator.Send(new RecordActivityCommand
        {
            // UserProfileId will be determined by the RecordActivityCommand handler based on the current user
            ActionType = UserActionType.CreateFamily,
            TargetType = TargetType.Family,
            TargetId = notification.Family.Id.ToString(),
            ActivitySummary = $"Created family '{notification.Family.Name}'."
        }, cancellationToken);

        // Send notification for family creation
        await _notificationService.SendNotification(new NotificationMessage
        {
            RecipientUserId = notification.Family.CreatedBy!, // Assuming CreatedBy is the recipient
            Title = "Family Created",
            Body = $"Your family '{notification.Family.Name}' has been successfully created.",
            Data = new Dictionary<string, string>
            {
                { "FamilyId", notification.Family.Id.ToString() },
                { "FamilyName", notification.Family.Name }
            },
            DeepLink = $"/families/{notification.Family.Id}" // Example deep link
        }, cancellationToken);

        // Store family data in Vector DB for search
        try
        {
            var embeddingProvider = _embeddingProviderFactory.GetProvider(EmbeddingAIProvider.OpenAI); // Use correct enum
            var vectorStore = _vectorStoreFactory.CreateVectorStore(VectorStoreProviderType.Pinecone); // Use correct method and enum

            string textToEmbed = $"Family Name: {notification.Family.Name}. Description: {notification.Family.Description}";
            var embeddingResult = await embeddingProvider.GenerateEmbeddingAsync(textToEmbed, cancellationToken);

            if (embeddingResult.IsSuccess) // Use IsSuccess
            {
                var metadata = new Dictionary<string, string>
                {
                    { "FamilyId", notification.Family.Id.ToString() },
                    { "FamilyName", notification.Family.Name },
                    { "Description", notification.Family.Description ?? "" }
                };
                await vectorStore.UpsertAsync(embeddingResult.Value!.ToList(), metadata, "families", embeddingProvider.EmbeddingDimension, cancellationToken); // Use EmbeddingDimension and null-forgiving operator
                _logger.LogInformation("Family {FamilyId} data successfully upserted to vector DB.", notification.Family.Id);
            }
            else
            {
                _logger.LogError("Failed to generate embedding for family {FamilyId}: {Error}", notification.Family.Id, embeddingResult.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing family {FamilyId} data in vector DB.", notification.Family.Id);
        }
    }
}
