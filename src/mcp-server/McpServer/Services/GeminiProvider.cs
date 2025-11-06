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
        private readonly GeminiSettings _settings;
        private readonly ILogger<GeminiProvider> _logger;
        private readonly IAiPromptBuilder _promptBuilder; // Inject IAiPromptBuilder

        public GeminiProvider(IOptions<GeminiSettings> settings, ILogger<GeminiProvider> logger, IAiPromptBuilder promptBuilder)
        {
            _settings = settings.Value;
            _logger = logger;
            _promptBuilder = promptBuilder;
        }

        public async IAsyncEnumerable<AiResponsePart> GenerateToolUseResponseStreamAsync(
            string prompt,
            List<AiToolDefinition>? tools = null,
            List<AiToolResult>? toolResults = null)
        {
            var fullPrompt = _promptBuilder.BuildPromptForToolUse(prompt, tools, toolResults);
            List<AiResponsePart> parts = new List<AiResponsePart>();
            try
            {
                var credential = GoogleCredential.GetApplicationDefault();
                var client = await new PredictionServiceClientBuilder
                {
                    Endpoint = "aiplatform.googleapis.com",
                    Credential = credential
                }.BuildAsync();

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

                var endpointNameString = $"projects/{_settings.ProjectId}/locations/{_settings.Location}/publishers/google/models/{_settings.ModelId}";
                var endpoint = EndpointName.Parse(endpointNameString);

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

        public async IAsyncEnumerable<AiResponsePart> GenerateChatResponseStreamAsync(string prompt)
        {
            var fullPrompt = _promptBuilder.BuildPromptForChat(prompt);
            List<AiResponsePart> parts = new List<AiResponsePart>();
            try
            {
                var credential = GoogleCredential.GetApplicationDefault();
                var client = await new PredictionServiceClientBuilder
                {
                    Endpoint = "aiplatform.googleapis.com",
                    Credential = credential
                }.BuildAsync();

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

                var endpointNameString = $"projects/{_settings.ProjectId}/locations/{_settings.Location}/publishers/google/models/{_settings.ModelId}";
                var endpoint = EndpointName.Parse(endpointNameString);

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

        public Task<string> GetStatusAsync()
        {
            // For now, just return "OK" as a placeholder.
            // In a real-world scenario, this would involve checking connectivity to the Gemini API.
            return Task.FromResult("OK");
        }
    }
}