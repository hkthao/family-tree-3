using Google.Cloud.AIPlatform.V1; // For Vertex AI Gemini API
using Google.Apis.Auth.OAuth2; // For authentication
using Grpc.Auth; // For gRPC authentication
using Grpc.Core; // For Channel
using Google.Protobuf.WellKnownTypes; // For Value and Struct
using McpServer.Config;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using McpServer.Services.Ai; // For IAiProvider
using McpServer.Services.Ai.Prompt; // For IAiPromptBuilder
using McpServer.Services.Ai.Tools; // For AiTool related types

namespace McpServer.Services.Ai.Providers
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
            string userPrompt,
            List<AiToolDefinition>? tools = null,
            List<AiToolResult>? toolResults = null)
        {
            List<AiResponsePart> partsToYield = new List<AiResponsePart>();
            try
            {
                var messages = _promptBuilder.BuildPromptForToolUse(userPrompt, tools, toolResults);
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
                            { "prompt", new Google.Protobuf.WellKnownTypes.Value { StringValue = messages.First().Content } } // Assuming the first message is the main prompt
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
                        partsToYield.Add(new AiTextResponsePart(contentValue.StringValue));
                    }
                    else
                    {
                        partsToYield.Add(new AiTextResponsePart("Không có phản hồi từ Gemini AI."));
                    }
                }
                else
                {
                    partsToYield.Add(new AiTextResponsePart("Không có phản hồi từ Gemini AI."));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi Gemini AI: {Message}", ex.Message);
                partsToYield.Add(new AiTextResponsePart($"Đã xảy ra lỗi khi xử lý yêu cầu của bạn với Gemini AI: {ex.Message}"));
            }

            foreach (var part in partsToYield)
            {
                yield return part;
            }
        }

        public async IAsyncEnumerable<AiResponsePart> GenerateChatResponseStreamAsync(string userPrompt)
        {
            List<AiResponsePart> partsToYield = new List<AiResponsePart>();
            try
            {
                var messages = _promptBuilder.BuildPromptForChat(userPrompt);
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
                            { "prompt", new Google.Protobuf.WellKnownTypes.Value { StringValue = messages.First().Content } } // Assuming the first message is the main prompt
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
                        partsToYield.Add(new AiTextResponsePart(contentValue.StringValue));
                    }
                    else
                    {
                        partsToYield.Add(new AiTextResponsePart("Không có phản hồi từ Gemini AI."));
                    }
                }
                else
                {
                    partsToYield.Add(new AiTextResponsePart("Không có phản hồi từ Gemini AI."));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi Gemini AI: {Message}", ex.Message);
                partsToYield.Add(new AiTextResponsePart($"Đã xảy ra lỗi khi xử lý yêu cầu của bạn với Gemini AI: {ex.Message}"));
            }

            foreach (var part in partsToYield)
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