using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Services;

namespace backend.Application.AI.Chunk.ProcessFile;

public class ProcessFileCommandHandler(IFileTextExtractorFactory extractorFactory, IChunkingPolicy chunkingPolicy) : IRequestHandler<ProcessFileCommand, Result<List<TextChunk>>>
{
    private readonly IFileTextExtractorFactory _extractorFactory = extractorFactory;
    private readonly IChunkingPolicy _chunkingPolicy = chunkingPolicy;

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
