using backend.Application.Common.Interfaces;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using System.Text;

namespace backend.Infrastructure.Services
{
    public class PdfTextExtractor : IFileTextExtractor
    {
        public async Task<string> ExtractTextAsync(Stream fileStream)
        {
            return await Task.Run(() =>
            {
                using (var document = PdfDocument.Open(fileStream))
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var page in document.GetPages())
                    {
                        sb.AppendLine(page.Text);
                    }
                    return sb.ToString();
                }
            });
        }
    }
}
