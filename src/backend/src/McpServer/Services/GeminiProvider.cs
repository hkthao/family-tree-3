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
    /// Tạm thời không hỗ trợ streaming và tool-calling đầy đủ do hạn chế của thư viện hiện tại.
    /// </summary>
    public class GeminiProvider : IAiProvider
    {
        private readonly GeminiSettings _settings;
        private readonly ILogger<GeminiProvider> _logger;

        public GeminiProvider(IOptions<GeminiSettings> settings, ILogger<GeminiProvider> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async IAsyncEnumerable<AiResponsePart> GenerateResponseStreamAsync(
            string prompt,
            List<AiToolDefinition>? tools = null,
            List<AiToolResult>? toolResults = null)
        {
            // Combine prompt and context if context is provided
            // For now, GeminiProvider will not fully implement tool use or streaming.
            // It will return a single text response.

            var fullPrompt = new StringBuilder();
            fullPrompt.AppendLine(prompt);

            if (tools != null && tools.Any())
            {
                fullPrompt.AppendLine("\nAvailable tools: ");
                fullPrompt.AppendLine(JsonSerializer.Serialize(tools, new JsonSerializerOptions { WriteIndented = true }));
            }

            if (toolResults != null && toolResults.Any())
            {
                fullPrompt.AppendLine("\nTool results: ");
                fullPrompt.AppendLine(JsonSerializer.Serialize(toolResults, new JsonSerializerOptions { WriteIndented = true }));
            }

            List<AiResponsePart> parts = new List<AiResponsePart>();
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
                            { "prompt", new Google.Protobuf.WellKnownTypes.Value { StringValue = fullPrompt.ToString() } }
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

                var endpoint = EndpointName.FromProjectLocationPublisherModel(_settings.ProjectId, _settings.Location, "google", _settings.ModelId);

                // The Google.Cloud.AIPlatform.V1 library's PredictAsync does not directly support streaming.
                // For now, I will return the full response as a single chunk.
                var response = await client.PredictAsync(endpoint, new[] { instance }, parameters);

                if (response.Predictions.Any())
                {
                    var prediction = response.Predictions.First();
                    if (prediction.StructValue.Fields.TryGetValue("content", out var contentValue))
                    {
                        parts.Add(new AiTextResponsePart(contentValue.StringValue));
                    }
                }
                else
                {
                    parts.Add(new AiTextResponsePart("Không có phản hồi từ Gemini AI."));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi Gemini AI: {Message}", ex.Message);
                parts.Add(new AiTextResponsePart($"Đã xảy ra lỗi khi xử lý yêu cầu của bạn với Gemini AI: {ex.Message}"));
            }

            foreach (var part in parts)
            {
                yield return part;
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
