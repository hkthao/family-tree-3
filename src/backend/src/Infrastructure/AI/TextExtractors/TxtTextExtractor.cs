using System.Text;
using backend.Application.Common.Interfaces;

namespace backend.Infrastructure.AI.TextExtractors
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
