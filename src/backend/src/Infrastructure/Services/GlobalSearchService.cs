using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.Services;

public class GlobalSearchService(ILogger<GlobalSearchService> logger, IVectorStoreFactory vectorStoreFactory, IN8nService n8nService) : IGlobalSearchService
{
    private readonly ILogger<GlobalSearchService> _logger = logger;
    private readonly IVectorStoreFactory _vectorStoreFactory = vectorStoreFactory;
    private readonly IN8nService _n8nService = n8nService;

    public async Task UpsertEntityAsync<T>(T entity, string entityType, Func<T, string> textExtractor, Func<T, Dictionary<string, string>> metadataExtractor, CancellationToken cancellationToken = default)
    {
        try
        {
            var vectorStore = _vectorStoreFactory.CreateVectorStore(VectorStoreProviderType.Pinecone); // Use configured store

            string textToEmbed = textExtractor(entity);
            var metadata = metadataExtractor(entity);

            // Ensure EntityType and EntityId are in metadata
            if (!metadata.ContainsKey("EntityType")) metadata["EntityType"] = entityType;
            if (!metadata.ContainsKey("EntityId"))
            {
                // Assuming entity has an Id property of type Guid
                var idProperty = typeof(T).GetProperty("Id");
                if (idProperty != null && idProperty.PropertyType == typeof(Guid))
                {
                    metadata["EntityId"] = ((Guid)idProperty.GetValue(entity)!).ToString();
                }
                else
                {
                    _logger.LogWarning("Entity {EntityType} does not have a Guid Id property for search indexing. Skipping upsert.", entityType);
                    return; // Skip upsert if no identifiable ID
                }
            }

            var embeddingDto = new EmbeddingWebhookDto
            {
                EntityType = entityType,
                EntityId = metadata["EntityId"],
                ActionType = "Upsert",
                EntityData = textToEmbed,
                Description = metadata["Description"]
            };

            var embeddingResult = await _n8nService.CallEmbeddingWebhookAsync(embeddingDto, cancellationToken);

            if (embeddingResult.IsSuccess && embeddingResult.Value != null)
            {
                // Assuming the embeddingResult.Value now contains the double[] embedding
                string collectionName = entityType.ToLower() + "s"; // Simple convention for collection name
                await vectorStore.UpsertAsync(embeddingResult.Value.ToList(), metadata, collectionName, embeddingResult.Value.Length, cancellationToken);
                _logger.LogInformation("Entity {EntityType} with ID {EntityId} data successfully upserted to vector DB for search.", entityType, metadata["EntityId"]);
            }
            else
            {
                _logger.LogError("Failed to generate embedding for entity {EntityType} with ID {EntityId}: {Error}", entityType, metadata["EntityId"], embeddingResult.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting entity {EntityType} data to vector DB for search.", entityType);
        }
    }

    public async Task<Result<List<GlobalSearchResult>>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        try
        {
            var embeddingDto = new EmbeddingWebhookDto
            {
                EntityType = "Query",
                EntityId = Guid.NewGuid().ToString(), // Unique ID for the query
                ActionType = "Search",
                EntityData = query,
                Description = "Search query embedding"
            };

            var queryEmbeddingResult = await _n8nService.CallEmbeddingWebhookAsync(embeddingDto, cancellationToken);

            if (!queryEmbeddingResult.IsSuccess || queryEmbeddingResult.Value == null)
            {
                return Result<List<GlobalSearchResult>>.Failure($"Failed to generate embedding for search query: {queryEmbeddingResult.Error}");
            }

            // Assuming the queryEmbeddingResult.Value now contains the double[] embedding
            var vectorStore = _vectorStoreFactory.CreateVectorStore(VectorStoreProviderType.Pinecone); // Use configured store

            var queryResults = await vectorStore.QueryAsync(queryEmbeddingResult.Value, 10, new Dictionary<string, string>(), "families", cancellationToken);

            var results = queryResults.Select(qr => new GlobalSearchResult
            {
                EntityType = qr.Metadata["EntityType"],
                EntityId = qr.Metadata["EntityId"],
                Title = qr.Metadata["Name"],
                Description = qr.Metadata.TryGetValue("Description", out var desc) ? desc : "",
                DeepLink = qr.Metadata["DeepLink"],
                Score = qr.Score
            }).ToList();

            return Result<List<GlobalSearchResult>>.Success(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing global search for query: {Query}", query);
            return Result<List<GlobalSearchResult>>.Failure($"Error performing global search: {ex.Message}");
        }
    }

    public async Task DeleteEntityFromSearchAsync(string entityId, string entityType, CancellationToken cancellationToken = default)
    {
        try
        {
            var vectorStore = _vectorStoreFactory.CreateVectorStore(VectorStoreProviderType.Pinecone); // Use configured store
            string collectionName = entityType.ToLower() + "s"; // Simple convention for collection name

            await vectorStore.DeleteAsync(entityId, collectionName, cancellationToken);
            _logger.LogInformation("Entity {EntityType} with ID {EntityId} successfully deleted from vector DB search index.", entityType, entityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity {EntityType} with ID {EntityId} from vector DB search index.", entityType, entityId);
        }
    }
}
