using System.Text;
using System.Text.RegularExpressions;
using backend.Domain.Entities;

namespace backend.Domain.Services
{
    public class ChunkingPolicy
    {
        private const int MinChunkWords = 300;
        private const int MaxChunkWords = 400;

        public List<TextChunk> ChunkText(string text, string fileName, string fileId, string familyId, string category, string createdBy)
        {
            string cleanedText = CleanText(text);
            List<TextChunk> chunks = new List<TextChunk>();
            string[] words = cleanedText.Split(new[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            int currentWordIndex = 0;
            while (currentWordIndex < words.Length)
            {
                StringBuilder currentChunkContent = new StringBuilder();
                int chunkWordCount = 0;

                // Try to build a chunk within MaxChunkWords
                for (int i = currentWordIndex; i < words.Length; i++)
                {
                    if (chunkWordCount + words[i].Length + 1 > MaxChunkWords) // +1 for space
                    {
                        if (chunkWordCount >= MinChunkWords) break; // Chunk is big enough, break
                    }
                    currentChunkContent.Append(words[i]).Append(' ');
                    chunkWordCount++;
                }

                // If we couldn't even reach MinChunkWords, just take MaxChunkWords or remaining words
                if (chunkWordCount < MinChunkWords && (words.Length - currentWordIndex) > MinChunkWords)
                {
                    currentChunkContent.Clear();
                    chunkWordCount = 0;
                    for (int i = currentWordIndex; i < Math.Min(currentWordIndex + MaxChunkWords, words.Length); i++)
                    {
                        currentChunkContent.Append(words[i]).Append(' ');
                        chunkWordCount++;
                    }
                }

                if (currentChunkContent.Length > 0)
                {
                    chunks.Add(new TextChunk
                    {
                        Id = Guid.NewGuid().ToString(),
                        Content = currentChunkContent.ToString().Trim(),
                        Metadata = new Dictionary<string, string>
                        {
                            { "fileName", fileName ?? string.Empty },
                            { "fileId", fileId ?? string.Empty },
                            { "familyId", familyId ?? string.Empty },
                            { "category", category ?? string.Empty },
                            { "createdBy", createdBy ?? string.Empty },
                            { "createdAt", DateTime.UtcNow.ToString("o") }, // ISO 8601 format
                            { "page", "1" } // Placeholder for page number, to be improved with PDF extraction
                        }
                    });
                    currentWordIndex += chunkWordCount;
                }
                else
                {
                    // Should not happen if words.Length > 0, but as a safeguard
                    currentWordIndex++;
                }
            }

            return chunks;
        }

        private string CleanText(string text)
        {
            // Normalize line endings
            text = text.Replace("\r\n", "\n").Replace("\r", "\n");
            // Replace multiple newlines with at most two (to preserve paragraph breaks)
            text = Regex.Replace(text, @"\n{3,}", "\n\n");
            // Remove extra spaces
            text = Regex.Replace(text, @"[ \t]+", " ");
            // Trim leading/trailing whitespace from each line
            text = string.Join("\n", text.Split("\n").Select(line => line.Trim()));
            // Remove leading/trailing whitespace from the whole text
            return text.Trim();
        }
    }
}
