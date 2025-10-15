using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families;
using backend.Domain.Enums;
using System.Text.Json;

namespace Application.NaturalLanguageInput.Commands.GenerateData;

public class GenerateFamilyDataCommandHandler : IRequestHandler<GenerateFamilyDataCommand, Result<List<FamilyDto>>>
{
    private readonly IChatProviderFactory _chatProviderFactory;

    public GenerateFamilyDataCommandHandler(IChatProviderFactory chatProviderFactory)
    {
        _chatProviderFactory = chatProviderFactory;
    }

    public async Task<Result<List<FamilyDto>>> Handle(GenerateFamilyDataCommand request, CancellationToken cancellationToken)
    {
        var chatProvider = _chatProviderFactory.GetProvider(ChatAIProvider.Local);

        var systemPrompt = @"You are an AI assistant that generates JSON data for family entities based on natural language descriptions.
The output should always be a single JSON object containing one array: 'families'.
Each object in the 'families' array should have 'name', 'description' (extract from prompt if detailed), 'address', and 'visibility' (Public, Private, Shared).
Infer the entity type (Family) from the prompt. If the prompt describes multiple entities, include them in the respective arrays.
If details are missing, use placeholders (""Unknown"" or null) instead of leaving fields empty.
Example: 'Tạo một gia đình tên Nguyễn ở Hà Nội.'
Output: { ""families"": [{ ""name"": ""Gia đình Nguyễn"", ""address"": ""Hà Nội"", ""visibility"": ""Public"" }] }
Always respond with ONLY the JSON object. Do not include any conversational text.";

        var userPrompt = request.Prompt;

        var chatMessages = new List<ChatMessage>
        {
            new() { Role = "system", Content = systemPrompt },
            new() { Role = "user", Content = userPrompt }
        };

        string jsonString = await chatProvider.GenerateResponseAsync(chatMessages);
        jsonString = jsonString.Trim(); // Trim whitespace

        if (string.IsNullOrWhiteSpace(jsonString))
        {
            return Result<List<FamilyDto>>.Failure("AI did not return a response.");
        }

        try
        {
            var aiResponse = JsonSerializer.Deserialize<FamilyResponseData>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (aiResponse == null || aiResponse.Families == null) {
                return Result<List<FamilyDto>>.Failure("AI generated empty or unparseable JSON response.");
            }

            return Result<List<FamilyDto>>.Success(aiResponse.Families);
        }
        catch (JsonException ex)
        {
            return Result<List<FamilyDto>>.Failure($"AI generated invalid JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<List<FamilyDto>>.Failure($"An unexpected error occurred while processing AI response: {ex.Message}");
        }
    }

    private class FamilyResponseData
    {
        public List<FamilyDto>? Families { get; set; }
    }
}
