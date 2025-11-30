using backend.Application.Common.Models; // For Result type
using System.Threading;
using System.Threading.Tasks;

namespace backend.Application.Common.Interfaces;

public interface IThumbnailUploadService
{
    Task<Result<string>> UploadThumbnailAsync(string base64Thumbnail, Guid memberFamilyId, string faceId, CancellationToken cancellationToken);
}
