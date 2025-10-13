using backend.Application.Common.Interfaces;
using System.Text.RegularExpressions;

namespace backend.Infrastructure.AI.TextExtractors
{
    public class MdTextExtractor : IFileTextExtractor
    {
        public async Task<string> ExtractTextAsync(Stream fileStream)
        {
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string markdownContent = await reader.ReadToEndAsync();

                // Simple Markdown to plain text conversion
                // Remove headers (e.g., # Header)
                markdownContent = Regex.Replace(markdownContent, @"^#+\s.*$", "", RegexOptions.Multiline);
                // Remove bold and italics (e.g., **bold**, *italics*)
                markdownContent = Regex.Replace(markdownContent, @"(\*\*|__)(.*?)\1", "$2");
                markdownContent = Regex.Replace(markdownContent, @"(\*|_)(.*?)\1", "$2");
                // Remove links (e.g., [text](url))
                markdownContent = Regex.Replace(markdownContent, @"\\[(.*?)\\](.*?)", " $1");
                // Remove images (e.g., ![alt text](url))
                markdownContent = Regex.Replace(markdownContent, @"!\[(.*?)\](.*?)", " $1");
                // Remove blockquotes (e.g., > Quote)
                markdownContent = Regex.Replace(markdownContent, @"^>\s.*$", "", RegexOptions.Multiline);
                // Remove list items (e.g., - Item)
                markdownContent = Regex.Replace(markdownContent, @"^(\*|\-|\+)\s", "", RegexOptions.Multiline);
                // Remove code blocks (e.g., ```code```)
                markdownContent = Regex.Replace(markdownContent, @"```.*?```", "", RegexOptions.Singleline);
                // Remove inline code (e.g., `code`)
                markdownContent = Regex.Replace(markdownContent, @"`(.*?)`", "$1");
                // Replace multiple newlines with a single one
                markdownContent = Regex.Replace(markdownContent, @"\n\s*\n", "\n");
                // Trim whitespace from each line and the overall string
                markdownContent = string.Join(Environment.NewLine, markdownContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(line => line.Trim()));
                markdownContent = markdownContent.Trim();

                return markdownContent;
            }
        }
    }
}
