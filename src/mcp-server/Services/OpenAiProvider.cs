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
        public async IAsyncEnumerable<string> GenerateResponseStreamAsync(string prompt, string? context = null)
        {
            // Combine prompt and context if context is provided
            if (!string.IsNullOrEmpty(context))
            {
                prompt = $"Context: {context}\n\nUser Query: {prompt}";
            }

            // Local function to handle the actual streaming logic
            async IAsyncEnumerable<string> StreamResponse(string currentPrompt)
            {
                List<string> chunks = new List<string>();
                try
                {
                    var requestBody = new
                    {
                        model = _openAiSettings.Model,
                        messages = new[]
                        {
                            new { role = "user", content = currentPrompt }
                        },
                        temperature = 0.7,
                        max_tokens = 1024,
                        top_p = 0.95,
                        frequency_penalty = 0,
                        presence_penalty = 0,
                        stream = true // Enable streaming
                    };

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiSettings.ApiKey);

                    var request = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions")
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

                        if (line.StartsWith("data: "))
                        {
                            var data = line.Substring("data: ".Length);
                            if (data == "[DONE]")
                            {
                                break; // Exit while loop, not yield break
                            }

                            string? contentToYield = null;
                            try
                            {
                                using var doc = JsonDocument.Parse(data);
                                var choices = doc.RootElement.GetProperty("choices");
                                if (choices.GetArrayLength() > 0)
                                {
                                    var delta = choices[0].GetProperty("delta");
                                    if (delta.TryGetProperty("content", out var content))
                                    {
                                        contentToYield = content.GetString() ?? "";
                                    }
                                }
                            }
                            catch (JsonException ex)
                            {
                                _logger.LogWarning(ex, "Failed to parse JSON from OpenAI stream: {Data}", data);
                            }

                            if (contentToYield != null)
                            {
                                chunks.Add(contentToYield);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi gọi OpenAI API: {Message}", ex.Message);
                    chunks.Add($"Đã xảy ra lỗi khi xử lý yêu cầu của bạn với OpenAI API: {ex.Message}");
                }

                foreach (var chunk in chunks)
                {
                    yield return chunk;
                }
            }

            await foreach (var chunk in StreamResponse(prompt))
            {
                yield return chunk;
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