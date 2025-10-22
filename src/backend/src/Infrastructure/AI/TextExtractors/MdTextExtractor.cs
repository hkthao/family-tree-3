using System.Text.RegularExpressions;
using backend.Application.Common.Interfaces;

namespace backend.Infrastructure.AI.TextExtractors
{
    public class MdTextExtractor : IFileTextExtractor
    {
        public async Task<string> ExtractTextAsync(Stream fileStream)
        {
            using StreamReader reader = new(fileStream);
            string markdownContent = await reader.ReadToEndAsync();

            // Remove code blocks (both fenced and indented)
            markdownContent = Regex.Replace(markdownContent, @"```.*?```", "", RegexOptions.Singleline);
            markdownContent = Regex.Replace(markdownContent, @"`(.+?)`", "$1", RegexOptions.Singleline);

            // Remove HTML tags
            markdownContent = Regex.Replace(markdownContent, @"<[^>]*>", "");

            // Replace images with their alt text
            markdownContent = Regex.Replace(markdownContent, @"!\[(.*?)\]\(.*?\)", "$1");

            // Replace links with their text
            markdownContent = Regex.Replace(markdownContent, @"\[(.*?)\]\(.*?\)", "$1");

            // Replace headers with a single newline
            markdownContent = Regex.Replace(markdownContent, @"^#+\s.*$", "\n", RegexOptions.Multiline);

            // Replace blockquotes with a single newline
            markdownContent = Regex.Replace(markdownContent, @"^>\s?.*$", "\n", RegexOptions.Multiline);

            // Remove list item markers, but keep the content and its newline
            markdownContent = Regex.Replace(markdownContent, @"^(\s*[\*\-+]|\s*\d+\.)\s+", "", RegexOptions.Multiline);

            // Remove bold and italics
            markdownContent = Regex.Replace(markdownContent, @"(\*\*|__)(.*?)\1", "$2");
            markdownContent = Regex.Replace(markdownContent, @"(\*|_)(.*?)\1", "$2");

            // Normalize newlines
            markdownContent = Regex.Replace(markdownContent, @"\r\n|\r", "\n");

            // Collapse multiple newlines to a single newline
            markdownContent = Regex.Replace(markdownContent, @"\n\s*\n", "\n");

            // Replace multiple spaces/tabs with a single space
            markdownContent = Regex.Replace(markdownContent, @"[ \t]+", " ");

            // Trim leading/trailing whitespace from the overall string
            markdownContent = markdownContent.Trim();

            return markdownContent;
        }
    }
}
