using System.IO;
using System.Threading.Tasks;

namespace backend.Application.Common.Interfaces
{
    public interface IFileTextExtractor
    {
        Task<string> ExtractTextAsync(Stream fileStream);
    }
}
