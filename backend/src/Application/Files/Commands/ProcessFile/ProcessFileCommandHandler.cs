using backend.Application.Common.Models;
using MediatR;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Services;
using System.Text.RegularExpressions;

namespace backend.Application.Files.Commands.ProcessFile
{
    public class ProcessFileCommandHandler : IRequestHandler<ProcessFileCommand, Result<List<TextChunk>>>
    {
        private readonly IFileTextExtractorFactory _extractorFactory;
        private readonly ChunkingPolicy _chunkingPolicy;

        public ProcessFileCommandHandler(IFileTextExtractorFactory extractorFactory, ChunkingPolicy chunkingPolicy)
        {
            _extractorFactory = extractorFactory;
            _chunkingPolicy = chunkingPolicy;
        }

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
}
