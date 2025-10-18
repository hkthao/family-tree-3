using System.Text;
using backend.Application.Common.Interfaces;
using UglyToad.PdfPig;

namespace backend.Infrastructure.AI.TextExtractors
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
