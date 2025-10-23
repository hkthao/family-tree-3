using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Services;

namespace backend.Application.AI.Chunk.ProcessFile;

/// <summary>
/// Xử lý lệnh ProcessFileCommand để trích xuất văn bản từ tệp và chia thành các đoạn.
/// </summary>
public class ProcessFileCommandHandler(IFileTextExtractorFactory extractorFactory, IChunkingPolicy chunkingPolicy) : IRequestHandler<ProcessFileCommand, Result<List<TextChunk>>>
{
    /// <summary>
    /// Nhà máy tạo trình trích xuất văn bản từ tệp.
    /// </summary>
    private readonly IFileTextExtractorFactory _extractorFactory = extractorFactory;
    /// <summary>
    /// Chính sách chia đoạn văn bản.
    /// </summary>
    private readonly IChunkingPolicy _chunkingPolicy = chunkingPolicy;

    /// <summary>
    /// Xử lý lệnh để trích xuất văn bản từ tệp và chia thành các đoạn.
    /// </summary>
    /// <param name="request">Lệnh chứa luồng tệp, tên tệp và các thông tin liên quan.</param>
    /// <param name="cancellationToken">Token để hủy bỏ thao tác.</param>
    /// <returns>Một đối tượng Result chứa danh sách các đoạn văn bản đã được trích xuất hoặc thông báo lỗi.</returns>
    public async Task<Result<List<TextChunk>>> Handle(ProcessFileCommand request, CancellationToken cancellationToken)
    {
        string fileExtension = Path.GetExtension(request.FileName).ToLowerInvariant();
        IFileTextExtractor extractor;
        try
        {
            extractor = _extractorFactory.GetExtractor(fileExtension);
        }
        catch (ArgumentException ex)
        {
            return Result<List<TextChunk>>.Failure(ex.Message, "Validation");
        }

        string textContent = await extractor.ExtractTextAsync(request.FileStream);

        var chunks = _chunkingPolicy.ChunkText(textContent, request.FileName, request.FileId, request.FamilyId, request.Category, request.CreatedBy);
        return Result<List<TextChunk>>.Success(chunks);
    }
}
