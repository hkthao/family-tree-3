using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Domain.Enums;

namespace backend.Application.AI.Chunk.EmbedChunks;

public class EmbedChunksCommandHandler : IRequestHandler<EmbedChunksCommand, Result>
{
    private readonly IEmbeddingProviderFactory _embeddingProviderFactory;
    private readonly IVectorStoreFactory _vectorStoreFactory;
    private readonly IConfigProvider _configProvider;

    public EmbedChunksCommandHandler(IEmbeddingProviderFactory embeddingProviderFactory, IVectorStoreFactory vectorStoreFactory, IConfigProvider configProvider)
    {
        _embeddingProviderFactory = embeddingProviderFactory;
        _vectorStoreFactory = vectorStoreFactory;
        _configProvider = configProvider;
    }

    public async Task<Result> Handle(EmbedChunksCommand request, CancellationToken cancellationToken)
    {
        if (request.Chunks == null || request.Chunks.Count == 0)
        {
            return Result.Failure("No chunks provided for embedding.");
        }

        var embeddingSettings = _configProvider.GetSection<EmbeddingSettings>();
        IEmbeddingProvider embeddingProvider;
        try
        {
            embeddingProvider = _embeddingProviderFactory.GetProvider(Enum.Parse<EmbeddingAIProvider>(embeddingSettings.Provider));
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        var vectorStoreSettings = _configProvider.GetSection<VectorStoreSettings>();
        IVectorStore vectorStore = _vectorStoreFactory.CreateVectorStore(Enum.Parse<VectorStoreProviderType>(vectorStoreSettings.Provider));

        foreach (var chunk in request.Chunks)
        {
            var embeddingResult = await embeddingProvider.GenerateEmbeddingAsync(chunk.Content, cancellationToken);
            if (!embeddingResult.IsSuccess)
            {
                return Result.Failure($"Failed to generate embedding for chunk {chunk.Id}: {embeddingResult.Error}");
            }
            chunk.Embedding = embeddingResult.Value;

            if (chunk.Embedding == null || !chunk.Embedding.Any())
            {
                return Result.Failure($"Generated embedding for chunk {chunk.Id} is null or empty.");
            }

            chunk.Metadata["Content"] = chunk.Content;
            await vectorStore.UpsertAsync(chunk.Embedding.ToList(), chunk.Metadata, cancellationToken);
        }

        return Result.Success();
    }
}
