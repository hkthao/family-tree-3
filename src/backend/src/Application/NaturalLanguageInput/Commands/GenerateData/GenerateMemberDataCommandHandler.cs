using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Queries;
using backend.Domain.Enums;

namespace Application.NaturalLanguageInput.Commands.GenerateData;

public class GenerateMemberDataCommandHandler(IChatProviderFactory chatProviderFactory) : IRequestHandler<GenerateMemberDataCommand, Result<List<MemberDto>>>
{
    private readonly IChatProviderFactory _chatProviderFactory = chatProviderFactory;

    public async Task<Result<List<MemberDto>>> Handle(GenerateMemberDataCommand request, CancellationToken cancellationToken)
    {
        var chatProvider = _chatProviderFactory.GetProvider(ChatAIProvider.Local);

        var systemPrompt = @"You are an AI assistant that generates JSON data for member entities based on natural language descriptions.
The output should always be a single JSON object containing one array: 'members'.
Each object in the 'members' array should have 'fullName', 'gender' (Male, Female, Other), 'dateOfBirth', 'dateOfDeath', 'placeOfBirth', 'placeOfDeath', 'occupation', and 'biography' (extract from prompt if detailed).
Infer the entity type (Member) from the prompt. If the prompt describes multiple entities, include them in the respective arrays.
If details are missing, use placeholders (""Unknown"" or null) instead of leaving fields empty.
Example: 'Thêm thành viên tên Trần Văn A, sinh năm 1990.'
Output: { ""members"": [{ ""fullName"": ""Trần Văn A"", ""gender"": ""Male"", ""dateOfBirth"": ""1990-01-01"", ""occupation"": ""Unknown"" }] }
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
            return Result<List<MemberDto>>.Failure("AI did not return a response.");
        }

        try
        {
            var aiResponse = JsonSerializer.Deserialize<MemberResponseData>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return aiResponse == null || aiResponse.Members == null || !aiResponse.Members.Any()
                ? Result<List<MemberDto>>.Failure("AI generated empty or unparseable JSON response.")
                : Result<List<MemberDto>>.Success(aiResponse.Members);
        }
        catch (JsonException ex)
        {
            return Result<List<MemberDto>>.Failure($"AI generated invalid JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<List<MemberDto>>.Failure($"An unexpected error occurred while processing AI response: {ex.Message}");
        }
    }

    private class MemberResponseData
    {
        public List<MemberDto>? Members { get; set; }
    }
}
