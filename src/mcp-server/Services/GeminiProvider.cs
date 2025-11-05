using Google.Cloud.AIPlatform.V1; // For Vertex AI Gemini API
using Google.Apis.Auth.OAuth2; // For authentication
using Grpc.Auth; // For gRPC authentication
using Grpc.Core; // For Channel
using Google.Protobuf.WellKnownTypes; // For Value and Struct
using McpServer.Config;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace McpServer.Services
{
    /// <summary>
    /// Nhà cung cấp AI Assistant sử dụng Gemini API.
    /// </summary>
    public class GeminiProvider : IAiProvider
    {
        private readonly GeminiSettings _geminiSettings;
        private readonly ILogger<GeminiProvider> _logger;

        public GeminiProvider(IOptions<GeminiSettings> geminiSettings, ILogger<GeminiProvider> logger)
        {
            _geminiSettings = geminiSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Gửi prompt đến Gemini AI và nhận kết quả.
        /// </summary>
        /// <param name="prompt">Prompt từ người dùng.</param>
        /// <param name="context">Ngữ cảnh bổ sung (ví dụ: dữ liệu backend đã được truy xuất).</param>
        /// <returns>Phản hồi từ Gemini AI.</returns>
        public async IAsyncEnumerable<string> GenerateResponseStreamAsync(string prompt, string? context = null)
        {
            // Combine prompt and context if context is provided
            if (!string.IsNullOrEmpty(context))
            {
                prompt = string.Format("Context: {0}\n\nUser Query: {1}", context, prompt);
            }

            // Local function to handle the actual streaming logic
            async IAsyncEnumerable<string> StreamResponse(string currentPrompt)
            {
                List<string> chunks = new List<string>();
                try
                {
                    // Initialize Gemini client
                    var credential = GoogleCredential.GetApplicationDefault();
                    var client = new PredictionServiceClientBuilder
                    {
                        Endpoint = "aiplatform.googleapis.com",
                        Credential = credential
                    }.Build();

                    // Construct the request for the Gemini model
                    var instance = new Google.Protobuf.WellKnownTypes.Value
                    {
                        StructValue = new Struct
                        {
                            Fields =
                            {
                                { "prompt", new Google.Protobuf.WellKnownTypes.Value { StringValue = currentPrompt } }
                            }
                        }
                    };

                    var parameters = new Google.Protobuf.WellKnownTypes.Value
                    {
                        StructValue = new Struct
                        {
                            Fields =
                            {
                                { "temperature", new Google.Protobuf.WellKnownTypes.Value { NumberValue = 0.7 } },
                                { "maxOutputTokens", new Google.Protobuf.WellKnownTypes.Value { NumberValue = 1024 } },
                                { "topP", new Google.Protobuf.WellKnownTypes.Value { NumberValue = 0.95 } },
                                { "topK", new Google.Protobuf.WellKnownTypes.Value { NumberValue = 40 } }
                            }
                        }
                    };

                    var endpoint = EndpointName.FromProjectLocationPublisherModel(_geminiSettings.ProjectId, _geminiSettings.Location, "google", _geminiSettings.ModelId);

                    // The Google.Cloud.AIPlatform.V1 library's PredictAsync does not directly support streaming.
                    // For now, I will return the full response as a single chunk.
                    var response = await client.PredictAsync(endpoint, new[] { instance }, parameters);

                    if (response.Predictions.Any())
                    {
                        var prediction = response.Predictions.First();
                        if (prediction.StructValue.Fields.TryGetValue("content", out var contentValue))
                        {
                            chunks.Add(contentValue.StringValue);
                        }
                    }
                    else
                    {
                        chunks.Add("Không có phản hồi từ Gemini AI.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi gọi Gemini AI: {Message}", ex.Message);
                    chunks.Add($"Đã xảy ra lỗi khi xử lý yêu cầu của bạn với Gemini AI: {ex.Message}");
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
        /// Kiểm tra trạng thái hoạt động của nhà cung cấp AI.
        /// </summary>
        /// <returns>Trạng thái hoạt động.</returns>
        public Task<string> GetStatusAsync()
        {
            // For now, just return "OK" as a placeholder.
            // In a real-world scenario, this would involve checking connectivity to the Gemini API.
            return Task.FromResult("OK");
        }
    }
}