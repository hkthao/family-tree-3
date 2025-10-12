using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;
using Microsoft.Extensions.Options;
using backend.Application.AI.VectorStore;

namespace backend.Application.AI.Chunk.EmbedChunks;

public class EmbedChunksCommandHandler : IRequestHandler<EmbedChunksCommand, Result>
{
    private readonly IEmbeddingProviderFactory _embeddingProviderFactory;
    private readonly IVectorStoreFactory _vectorStoreFactory;
    private readonly VectorStoreSettings _vectorStoreSettings;

    public EmbedChunksCommandHandler(IEmbeddingProviderFactory embeddingProviderFactory, IVectorStoreFactory vectorStoreFactory, IOptions<VectorStoreSettings> vectorStoreSettingsOptions)
    {
        _embeddingProviderFactory = embeddingProviderFactory;
        _vectorStoreFactory = vectorStoreFactory;
        _vectorStoreSettings = vectorStoreSettingsOptions.Value;
    }

    public async Task<Result> Handle(EmbedChunksCommand request, CancellationToken cancellationToken)
    {
        if (request.Chunks == null || !request.Chunks.Any())
        {
            return Result.Failure("No chunks provided for embedding.");
        }

        IEmbeddingProvider embeddingProvider;
        try
        {
            embeddingProvider = _embeddingProviderFactory.GetProvider(request.ProviderName);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        IVectorStore vectorStore = _vectorStoreFactory.CreateVectorStore(Enum.Parse<VectorStoreProviderType>(_vectorStoreSettings.Provider));

        foreach (var chunk in request.Chunks)
        {
            var embeddingResult = await embeddingProvider.GenerateEmbeddingAsync(chunk.Content, cancellationToken);
            if (!embeddingResult.IsSuccess)
            {
                return Result.Failure($"Failed to generate embedding for chunk {chunk.Id}: {embeddingResult.Error}");
            }
            chunk.Embedding = embeddingResult.Value;

            await vectorStore.UpsertAsync(chunk, cancellationToken);
        }

        return Result.Success();
    }
}
