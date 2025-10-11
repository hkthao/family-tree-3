using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Files.CleanupUnusedFiles
{
    public class CleanupUnusedFilesCommandHandler : IRequestHandler<CleanupUnusedFilesCommand, Result<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IFileStorage _fileStorage;
        private readonly IDateTime _dateTime;

        public CleanupUnusedFilesCommandHandler(IApplicationDbContext context, IFileStorage fileStorage, IDateTime dateTime)
        {
            _context = context;
            _fileStorage = fileStorage;
            _dateTime = dateTime;
        }

        public async Task<Result<int>> Handle(CleanupUnusedFilesCommand request, CancellationToken cancellationToken)
        {
            var cutoffDate = _dateTime.Now.Subtract(request.OlderThan);

            // Find unused files: not active, not linked to any entity, and older than cutoffDate
            var unusedFiles = await _context.FileMetadata
                .Where(fm => !fm.IsActive && fm.UsedById == null && fm.Created < cutoffDate)
                .ToListAsync(cancellationToken);

            if (!unusedFiles.Any())
            {
                return Result<int>.Success(0);
            }

            int deletedCount = 0;
            foreach (var file in unusedFiles)
            {
                // Attempt to delete from actual storage
                var deleteResult = await _fileStorage.DeleteFileAsync(file.Url, cancellationToken);
                if (deleteResult.IsSuccess)
                {
                    // Remove metadata from DB only if successfully deleted from storage
                    _context.FileMetadata.Remove(file);
                    deletedCount++;
                }
                else
                {
                    // Log error but continue with other files
                    // In a real app, you might want more robust error handling/retries
                    Console.WriteLine($"Failed to delete file {file.FileName} from storage: {deleteResult.Error}");
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(deletedCount);
        }
    }
}
