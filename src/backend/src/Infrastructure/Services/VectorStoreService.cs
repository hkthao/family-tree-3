using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Infrastructure.Services;

public class VectorStoreService : IVectorStoreService
{
    private readonly ILogger<VectorStoreService> _logger;

    public VectorStoreService(ILogger<VectorStoreService> logger)
    {
        _logger = logger;
    }

    public async Task<Result<Unit>> SaveVectorAsync(List<float> embedding, Dictionary<string, string> metadata, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Simulating SaveVectorAsync call. Embedding size: {EmbeddingSize}, Metadata count: {MetadataCount}",
            embedding.Count, metadata.Count);

        // Simulate API call delay
        await Task.Delay(50, cancellationToken);

        // Log metadata for verification
        foreach (var item in metadata)
        {
            _logger.LogDebug("Metadata: {Key} = {Value}", item.Key, item.Value);
        }

        return Result<Unit>.Success(Unit.Value);
    }
}
