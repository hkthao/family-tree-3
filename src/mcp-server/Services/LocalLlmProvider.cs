using McpServer.Config;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Net.Http.Headers;

namespace McpServer.Services
{
    /// <summary>
    /// Nhà cung cấp AI Assistant sử dụng Local LLM (ví dụ: Ollama).
    /// </summary>
    public class LocalLlmProvider : IAiProvider
    {
        private readonly LocalLlmSettings _localLlmSettings;
        private readonly ILogger<LocalLlmProvider> _logger;
        private readonly HttpClient _httpClient; // For Local LLM API calls

        public LocalLlmProvider(IOptions<LocalLlmSettings> localLlmSettings, ILogger<LocalLlmProvider> logger, HttpClient httpClient)
        {
            _localLlmSettings = localLlmSettings.Value;
            _logger = logger;
            _httpClient = httpClient;

            if (string.IsNullOrEmpty(_localLlmSettings.BaseUrl))
            {
                _logger.LogWarning("LocalLlmSettings:BaseUrl is not configured. LocalLlmProvider might not function correctly.");
            }
            else
            {
                _httpClient.BaseAddress = new Uri(_localLlmSettings.BaseUrl);
            }
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Gửi prompt đến Local LLM và nhận kết quả.
        /// </summary>
        /// <param name="prompt">Prompt từ người dùng.</param>
        /// <param name="context">Ngữ cảnh bổ sung (ví dụ: dữ liệu backend đã được truy xuất).</param>
        /// <returns>Phản hồi từ Local LLM.</returns>
        public async Task<string> GenerateResponseAsync(string prompt, string? context = null)
        {
            if (string.IsNullOrEmpty(_localLlmSettings.BaseUrl))
            {
                return "Error: Local LLM BaseUrl is not configured.";
            }

            if (!string.IsNullOrEmpty(context))
            {
                prompt = $"Context: {context}\n\nUser Query: {prompt}";
            }

            _logger.LogInformation("Calling Local LLM API with prompt: {Prompt}", prompt);

            // Placeholder for actual Local LLM API call (e.g., Ollama)
            // This example assumes an Ollama-like API structure
            var requestBody = new
            {
                model = _localLlmSettings.Model,
                prompt = prompt,
                stream = false
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/generate", requestBody); // Ollama generate endpoint
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                using (JsonDocument doc = JsonDocument.Parse(responseContent))
                {
                    if (doc.RootElement.TryGetProperty("response", out JsonElement content))
                    {
                        return content.GetString() ?? "No content from Local LLM.";
                    }
                }
                return "Failed to parse Local LLM response.";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling Local LLM API.");
                return $"Error: Failed to connect to Local LLM API. {ex.Message}";
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing Local LLM response.");
                return $"Error: Failed to parse Local LLM response. {ex.Message}";
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái của Local LLM.
        /// </summary>
        /// <returns>Trạng thái của Local LLM.</returns>
        public async Task<string> GetStatusAsync()
        {
            if (string.IsNullOrEmpty(_localLlmSettings.BaseUrl))
            {
                return "Local LLM BaseUrl is not configured.";
            }
            try
            {
                // Assuming a simple GET request to the base URL can indicate status
                var response = await _httpClient.GetAsync("/");
                response.EnsureSuccessStatusCode();
                return "Local LLM is operational.";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error checking Local LLM status.");
                return $"Local LLM is not reachable. {ex.Message}";
            }
        }
    }
}