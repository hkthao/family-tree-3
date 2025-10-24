using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using backend.Application.AI.Embeddings;
using backend.Application.AI.VectorStore;

namespace backend.Infrastructure.Services;

public class GlobalSearchService(ILogger<GlobalSearchService> logger, IEmbeddingProviderFactory embeddingProviderFactory, IVectorStoreFactory vectorStoreFactory) : IGlobalSearchService
{
    private readonly ILogger<GlobalSearchService> _logger = logger;
    private readonly IEmbeddingProviderFactory _embeddingProviderFactory = embeddingProviderFactory;
    private readonly IVectorStoreFactory _vectorStoreFactory = vectorStoreFactory;

    private const string FamilyCollectionName = "families";

    public async Task UpsertFamilyForSearchAsync(Family family, CancellationToken cancellationToken = default)
    {
        try
        {
            var embeddingProvider = _embeddingProviderFactory.GetProvider(EmbeddingAIProvider.OpenAI); // Use configured provider
            var vectorStore = _vectorStoreFactory.CreateVectorStore(VectorStoreProviderType.Pinecone); // Use configured store

            string textToEmbed = $"Family Name: {family.Name}. Description: {family.Description}. Address: {family.Address}";
            var embeddingResult = await embeddingProvider.GenerateEmbeddingAsync(textToEmbed, cancellationToken);

            if (embeddingResult.IsSuccess)
            {
                var metadata = new Dictionary<string, string>
                {
                    { "EntityType", "Family" },
                    { "EntityId", family.Id.ToString() },
                    { "Name", family.Name },
                    { "Description", family.Description ?? "" },
                    { "DeepLink", $"/families/{family.Id}" }
                };
                await vectorStore.UpsertAsync(embeddingResult.Value!.ToList(), metadata, FamilyCollectionName, embeddingProvider.EmbeddingDimension, cancellationToken);
                _logger.LogInformation("Family {FamilyId} data successfully upserted to vector DB for search.", family.Id);
            }
            else
            {
                _logger.LogError("Failed to generate embedding for family {FamilyId}: {Error}", family.Id, embeddingResult.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting family {FamilyId} data to vector DB for search.", family.Id);
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

            var queryResults = await vectorStore.QueryAsync(queryEmbeddingResult.Value!, 10, new Dictionary<string, string>(), FamilyCollectionName, cancellationToken);

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
}
