using McpServer.Config;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Net.Http.Headers;

namespace McpServer.Services
{
    /// <summary>
    /// Nhà cung cấp AI Assistant sử dụng OpenAI API.
    /// </summary>
    public class OpenAiProvider : IAiProvider
    {
        private readonly OpenAiSettings _openAiSettings;
        private readonly ILogger<OpenAiProvider> _logger;
        private readonly HttpClient _httpClient; // For OpenAI API calls

        public OpenAiProvider(IOptions<OpenAiSettings> openAiSettings, ILogger<OpenAiProvider> logger, HttpClient httpClient)
        {
            _openAiSettings = openAiSettings.Value;
            _logger = logger;
            _httpClient = httpClient;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiSettings.ApiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/"); // OpenAI Base URL
        }

        /// <summary>
        /// Gửi prompt đến OpenAI và nhận kết quả.
        /// </summary>
        /// <param name="prompt">Prompt từ người dùng.</param>
        /// <param name="context">Ngữ cảnh bổ sung (ví dụ: dữ liệu backend đã được truy xuất).</param>
        /// <returns>Phản hồi từ OpenAI.</returns>
        public async Task<string> GenerateResponseAsync(string prompt, string? context = null)
        {
            if (!string.IsNullOrEmpty(context))
            {
                prompt = $"Context: {context}\n\nUser Query: {prompt}";
            }

            _logger.LogInformation("Calling OpenAI API with prompt: {Prompt}", prompt);

            // Placeholder for actual OpenAI API call
            // Example using System.Net.Http.Json
            var requestBody = new
            {
                model = _openAiSettings.Model,
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful AI assistant for family tree data." },
                    new { role = "user", content = prompt }
                }
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("chat/completions", requestBody);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                using (JsonDocument doc = JsonDocument.Parse(responseContent))
                {
                    if (doc.RootElement.TryGetProperty("choices", out JsonElement choices) &&
                        choices.EnumerateArray().FirstOrDefault().TryGetProperty("message", out JsonElement message) &&
                        message.TryGetProperty("content", out JsonElement content))
                    {
                        return content.GetString() ?? "No content from OpenAI.";
                    }
                }
                return "Failed to parse OpenAI response.";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling OpenAI API.");
                return $"Error: Failed to connect to OpenAI API. {ex.Message}";
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing OpenAI response.");
                return $"Error: Failed to parse OpenAI response. {ex.Message}";
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái của OpenAI.
        /// </summary>
        /// <returns>Trạng thái của OpenAI.</returns>
        public Task<string> GetStatusAsync()
        {
            // In a real scenario, you might ping the OpenAI API or check credentials.
            return Task.FromResult("OpenAI is operational.");
        }
    }
}