using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events;
using backend.Domain.Enums;

namespace Application.NaturalLanguageInput.Commands.GenerateData;

public class GenerateEventDataCommandHandler(IChatProviderFactory chatProviderFactory) : IRequestHandler<GenerateEventDataCommand, Result<List<EventDto>>>
{
    private readonly IChatProviderFactory _chatProviderFactory = chatProviderFactory;

    public async Task<Result<List<EventDto>>> Handle(GenerateEventDataCommand request, CancellationToken cancellationToken)
    {
        var chatProvider = _chatProviderFactory.GetProvider(ChatAIProvider.Local);

        var systemPrompt = @"You are an AI assistant that generates JSON data for event entities based on natural language descriptions.
The output should always be a single JSON object containing one array: 'events'.
Each object in the 'events' array should have 'title', 'description', 'date', 'location', 'associatedMembers', and 'type'.
Infer the entity type (Event) from the prompt. If the prompt describes multiple entities, include them in the respective arrays.
If details are missing, use placeholders (""Unknown"" or null) instead of leaving fields empty.
Example: 'Lên lịch một buổi họp mặt gia đình vào ngày 2025-01-01 tại nhà.'
Output: { ""events"": [{ ""title"": ""Buổi họp mặt gia đình"", ""date"": ""2025-01-01"", ""location"": ""Nhà"", ""type"": ""Gathering"" }] }
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
            return Result<List<EventDto>>.Failure("AI did not return a response.");
        }

        try
        {
            var aiResponse = JsonSerializer.Deserialize<EventResponseData>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return aiResponse == null || aiResponse.Events == null || !aiResponse.Events.Any()
                ? Result<List<EventDto>>.Failure("AI generated empty or unparseable JSON response.")
                : Result<List<EventDto>>.Success(aiResponse.Events);
        }
        catch (JsonException ex)
        {
            return Result<List<EventDto>>.Failure($"AI generated invalid JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<List<EventDto>>.Failure($"An unexpected error occurred while processing AI response: {ex.Message}");
        }
    }

    private class EventResponseData
    {
        public List<EventDto>? Events { get; set; }
    }
}
