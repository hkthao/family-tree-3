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
        public async IAsyncEnumerable<string> GenerateResponseStreamAsync(string prompt, string? context = null)
        {
            // Combine prompt and context if context is provided
            if (!string.IsNullOrEmpty(context))
            {
                prompt = $"Context: {context}\n\nUser Query: {prompt}";
            }

            var requestBody = new
            {
                model = _localLlmSettings.Model,
                prompt = prompt,
                stream = true // Enable streaming
            };

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, _localLlmSettings.BaseUrl)
                {
                    Content = JsonContent.Create(requestBody)
                };

                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                await using var responseStream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(responseStream);

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(line)) continue;

                    // Ollama typically sends JSON objects for each chunk
                    try
                    {
                        using var doc = JsonDocument.Parse(line);
                        if (doc.RootElement.TryGetProperty("response", out var responseProperty))
                        {
                            yield return responseProperty.GetString() ?? "";
                        }
                        if (doc.RootElement.TryGetProperty("done", out var doneProperty) && doneProperty.GetBoolean())
                        {
                            yield break; // Stream finished
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse JSON from Local LLM stream: {Line}", line);
                        // If it's not JSON, yield the raw line
                        yield return line;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi Local LLM API: {Message}", ex.Message);
                yield return $"Đã xảy ra lỗi khi xử lý yêu cầu của bạn với Local LLM API: {ex.Message}";
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