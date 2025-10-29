using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.Services;

public class GlobalSearchService(ILogger<GlobalSearchService> logger, IEmbeddingProviderFactory embeddingProviderFactory, IVectorStoreFactory vectorStoreFactory) : IGlobalSearchService
{
    private readonly ILogger<GlobalSearchService> _logger = logger;
    private readonly IEmbeddingProviderFactory _embeddingProviderFactory = embeddingProviderFactory;
    private readonly IVectorStoreFactory _vectorStoreFactory = vectorStoreFactory;

    public async Task UpsertEntityAsync<T>(T entity, string entityType, Func<T, string> textExtractor, Func<T, Dictionary<string, string>> metadataExtractor, CancellationToken cancellationToken = default)
    {
        try
        {
            var embeddingProvider = _embeddingProviderFactory.GetProvider(EmbeddingAIProvider.Local); // Use configured provider
            var vectorStore = _vectorStoreFactory.CreateVectorStore(VectorStoreProviderType.Pinecone); // Use configured store

            string textToEmbed = textExtractor(entity);
            var embeddingResult = await embeddingProvider.GenerateEmbeddingAsync(textToEmbed, cancellationToken);

            if (embeddingResult.IsSuccess)
            {
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

                string collectionName = entityType.ToLower() + "s"; // Simple convention for collection name
                await vectorStore.UpsertAsync(embeddingResult.Value!.ToList(), metadata, collectionName, embeddingProvider.EmbeddingDimension, cancellationToken);
                _logger.LogInformation("Entity {EntityType} with ID {EntityId} data successfully upserted to vector DB for search.", entityType, metadata["EntityId"]);
            }
            else
            {
                _logger.LogError("Failed to generate embedding for entity {EntityType} with ID {EntityId}: {Error}", entityType, metadataExtractor(entity).GetValueOrDefault("EntityId", "Unknown"), embeddingResult.Error);
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
            var embeddingProvider = _embeddingProviderFactory.GetProvider(EmbeddingAIProvider.OpenAI); // Use configured provider
            var vectorStore = _vectorStoreFactory.CreateVectorStore(VectorStoreProviderType.Pinecone); // Use configured store

            var queryEmbeddingResult = await embeddingProvider.GenerateEmbeddingAsync(query, cancellationToken);

            if (!queryEmbeddingResult.IsSuccess)
            {
                return Result<List<GlobalSearchResult>>.Failure($"Failed to generate embedding for search query: {queryEmbeddingResult.Error}");
            }

            var queryResults = await vectorStore.QueryAsync(queryEmbeddingResult.Value!, 10, new Dictionary<string, string>(), "families", cancellationToken); 

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