using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Domain.Enums;

namespace backend.Application.AI.Chunk.EmbedChunks;

/// <summary>
/// Xử lý lệnh EmbedChunksCommand để tạo và lưu trữ các embedding cho các đoạn văn bản.
/// </summary>
public class EmbedChunksCommandHandler(IEmbeddingProviderFactory embeddingProviderFactory, IVectorStoreFactory vectorStoreFactory, IConfigProvider configProvider) : IRequestHandler<EmbedChunksCommand, Result>
{
    /// <summary>
    /// Nhà máy tạo nhà cung cấp nhúng (embedding) AI.
    /// </summary>
    private readonly IEmbeddingProviderFactory _embeddingProviderFactory = embeddingProviderFactory;
    /// <summary>
    /// Nhà máy tạo kho lưu trữ vector.
    /// </summary>
    private readonly IVectorStoreFactory _vectorStoreFactory = vectorStoreFactory;
    /// <summary>
    /// Nhà cung cấp cấu hình ứng dụng.
    /// </summary>
    private readonly IConfigProvider _configProvider = configProvider;

    /// <summary>
    /// Xử lý lệnh để nhúng các đoạn văn bản và lưu trữ chúng vào kho vector.
    /// </summary>
    /// <param name="request">Lệnh chứa danh sách các đoạn văn bản cần nhúng.</param>
    /// <param name="cancellationToken">Token để hủy bỏ thao tác.</param>
    /// <returns>Một đối tượng Result cho biết thành công hay thất bại.</returns>
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
            await vectorStore.UpsertAsync([.. chunk.Embedding], chunk.Metadata, cancellationToken);
        }

        return Result.Success();
    }
}
