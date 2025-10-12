using backend.Application.Common.Models;
using MediatR;
using backend.Domain.Entities;

namespace backend.Application.Files.Commands.ProcessFile
{
    public class ProcessFileCommand : IRequest<Result<List<TextChunk>>>
    {
        public Stream FileStream { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string FileId { get; set; } = null!;
        public string FamilyId { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string CreatedBy { get; set; } = null!;
    }
}
