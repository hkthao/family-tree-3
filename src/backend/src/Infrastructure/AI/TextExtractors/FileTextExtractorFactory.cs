using backend.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Infrastructure.AI.TextExtractors
{
    public class FileTextExtractorFactory : IFileTextExtractorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public FileTextExtractorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IFileTextExtractor GetExtractor(string fileExtension)
        {
            return fileExtension.ToLowerInvariant() switch
            {
                ".pdf" => _serviceProvider.GetRequiredService<PdfTextExtractor>(),
                ".txt" => _serviceProvider.GetRequiredService<TxtTextExtractor>(),
                ".md" => _serviceProvider.GetRequiredService<MdTextExtractor>(),
                _ => throw new ArgumentException("Unsupported file type.")
            };
        }
    }
}
