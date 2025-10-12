using backend.Application.Common.Interfaces;
using System.Text;

namespace backend.Infrastructure.Services
{
    public class TxtTextExtractor : IFileTextExtractor
    {
        public async Task<string> ExtractTextAsync(Stream fileStream)
        {
            using (StreamReader reader = new StreamReader(fileStream, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
