using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families;
using backend.Application.Members.Queries;
using backend.Application.NaturalLanguageInput.Queries;
using backend.Domain.Enums;
using MediatR;
using System.Text.Json;

namespace Application.NaturalLanguageInput.Commands.GenerateData;

public class GenerateDataCommandHandler : IRequestHandler<GenerateDataCommand, Result<GeneratedEntityDto>>
{
    private readonly IChatProviderFactory _chatProviderFactory;

    public GenerateDataCommandHandler(IChatProviderFactory chatProviderFactory)
    {
        _chatProviderFactory = chatProviderFactory;
    }

    public async Task<Result<GeneratedEntityDto>> Handle(GenerateDataCommand request, CancellationToken cancellationToken)
    {
        var chatProvider = _chatProviderFactory.GetProvider(ChatAIProvider.Local);

        var systemPrompt = @"You are an AI assistant that generates JSON data for family tree entities (Family or Member) based on natural language descriptions.
The output should always be a single JSON object containing two arrays: 'families' and 'members'.
Each object in the 'families' array should have 'name', 'description', 'address', and 'visibility' (Public, Private, Shared).
Each object in the 'members' array should have 'fullName', 'gender' (Male, Female, Other), 'dateOfBirth', 'dateOfDeath', 'placeOfBirth', 'placeOfDeath', 'occupation', and 'biography'.
Infer the entity type (Family or Member) from the prompt. If the prompt describes multiple entities, include them in the respective arrays.
If the prompt is ambiguous or lacks sufficient detail, ask for clarification or provide a best-effort JSON with placeholders.
Example for multiple entities: 'Tạo một gia đình tên Nguyễn ở Hà Nội và thêm thành viên tên Trần Văn A, sinh năm 1990 vào gia đình đó.'
Output: { ""families"": [{ ""name"": ""Gia đình Nguyễn"", ""address"": ""Hà Nội"", ""visibility"": ""Public"" }], ""members"": [{ ""fullName"": ""Trần Văn A"", ""gender"": ""Male"", ""dateOfBirth"": ""1990-01-01"", ""occupation"": ""Unknown"" }] }
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
            return Result<GeneratedEntityDto>.Failure("AI did not return a response.");
        }

        try
        {
            // Temporary class to deserialize AI's response
            var aiResponse = JsonSerializer.Deserialize<AiResponseData>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (aiResponse == null) {
                return Result<GeneratedEntityDto>.Failure("AI generated empty or unparseable JSON response.");
            }

            var generatedEntity = new GeneratedEntityDto
            {
                Families = aiResponse.Families ?? new List<FamilyDto>(),
                Members = aiResponse.Members ?? new List<MemberDto>()
            };

            if (generatedEntity.Families.Any() && generatedEntity.Members.Any())
            {
                generatedEntity.DataType = "Mixed";
            }
            else if (generatedEntity.Families.Any())
            {
                generatedEntity.DataType = "Families";
            }
            else if (generatedEntity.Members.Any())
            {
                generatedEntity.DataType = "Members";
            }
            else
            {
                generatedEntity.DataType = "Unknown";
            }

            return Result<GeneratedEntityDto>.Success(generatedEntity);
        }
        catch (JsonException ex)
        {
            return Result<GeneratedEntityDto>.Failure($"AI generated invalid JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<GeneratedEntityDto>.Failure($"An unexpected error occurred while processing AI response: {ex.Message}");
        }
    }

    // Temporary class to deserialize AI's response
    private class AiResponseData
    {
        public List<FamilyDto>? Families { get; set; }
        public List<MemberDto>? Members { get; set; }
    }
}