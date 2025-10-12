using backend.Application.Common.Models;
using backend.Application.Common.Interfaces;
using backend.Application.AI.VectorStore;
using Microsoft.Extensions.Options;
using backend.Domain.Enums;

namespace backend.Application.AI.Chunk.UpsertChunk;

public class UpsertChunkCommandHandler : IRequestHandler<UpsertChunkCommand, Result>
{
    private readonly IVectorStoreFactory _vectorStoreFactory;
    private readonly VectorStoreSettings _vectorStoreSettings;

    public UpsertChunkCommandHandler(IVectorStoreFactory vectorStoreFactory, IOptions<VectorStoreSettings> vectorStoreSettings)
    {
        _vectorStoreFactory = vectorStoreFactory;
        _vectorStoreSettings = vectorStoreSettings.Value;
    }

    public async Task<Result> Handle(UpsertChunkCommand request, CancellationToken cancellationToken)
    {
        if (request.Chunk == null)
        {
            return Result.Failure("Chunk cannot be null.");
        }

        if (request.Chunk.Embedding == null || request.Chunk.Embedding.Length == 0)
        {
            return Result.Failure("Chunk embedding cannot be empty. Please ensure the chunk has been embedded before upserting.");
        }

        try
        {
            var vectorStore = _vectorStoreFactory.CreateVectorStore(Enum.Parse<VectorStoreProviderType>(_vectorStoreSettings.Provider));
            await vectorStore.UpsertAsync(request.Chunk, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to upsert chunk: {ex.Message}");
        }
    }
}
