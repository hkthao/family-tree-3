using backend.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Infrastructure.AI.TextExtractors
{
    public class FileTextExtractorFactory : IFileTextExtractorFactory
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public FileTextExtractorFactory(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        public IFileTextExtractor GetExtractor(string fileExtension)
        {
            var scope = _serviceScopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider; // Get the service provider for this scope

            return fileExtension.ToLowerInvariant() switch
            {
                ".pdf" => serviceProvider.GetRequiredService<PdfTextExtractor>(),
                ".txt" => serviceProvider.GetRequiredService<TxtTextExtractor>(),
                ".md" => serviceProvider.GetRequiredService<MdTextExtractor>(),
                _ => throw new ArgumentException("Unsupported file type.")
            };
        }
    }
}
