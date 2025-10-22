using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;
using FluentValidation.Results;

namespace backend.Application.Families.Commands.GenerateFamilyData;

public class GenerateFamilyDataCommandHandler(IChatProviderFactory chatProviderFactory, IValidator<FamilyDto> familyDtoValidator) : IRequestHandler<GenerateFamilyDataCommand, Result<List<FamilyDto>>>
{
    private readonly IChatProviderFactory _chatProviderFactory = chatProviderFactory;
    private readonly IValidator<FamilyDto> _familyDtoValidator = familyDtoValidator;

    public async Task<Result<List<FamilyDto>>> Handle(GenerateFamilyDataCommand request, CancellationToken cancellationToken)
    {
        var chatProvider = _chatProviderFactory.GetProvider(ChatAIProvider.Local);

        var systemPrompt = @"You are an AI assistant that generates JSON data for family entities based on natural language descriptions.
The output should always be a single JSON object containing one array: 'families'.
Each object in the 'families' array should have 'name', 'description' (extract from prompt if detailed), 'address', 'visibility' (Public, Private, Shared), 'avatarUrl', 'totalMembers', and 'totalGenerations'.
If 'visibility' is not explicitly mentioned in the prompt, default it to 'Public'.
Infer the entity type (Family) from the prompt. If the prompt describes multiple entities, include them in the respective arrays.
If details are missing, use placeholders (""Unknown"" or null) instead of leaving fields empty.
Example: 'Tạo một gia đình tên Nguyễn ở Hà Nội. Gia đình có 15 thành viên và 4 thế hệ. Ảnh đại diện là https://example.com/avatar.png.'
Output: { ""families"": [{ ""name"": ""Gia đình Nguyễn"", ""address"": ""Hà Nội"", ""visibility"": ""Public"", ""totalMembers"": 15, ""totalGenerations"": 4, ""avatarUrl"": ""https://example.com/avatar.png"" }] }
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

            if (aiResponse == null || aiResponse.Families == null || aiResponse.Families.Count == 0)
            {
                return Result<List<FamilyDto>>.Success([]); // Return empty list if no families generated
            }

            foreach (var family in aiResponse.Families)
            {
                if (string.IsNullOrWhiteSpace(family.Visibility))
                    family.Visibility = "Public";

                ValidationResult validationResult = await _familyDtoValidator.ValidateAsync(family, cancellationToken);
                if (!validationResult.IsValid)
                {
                    family.ValidationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                }
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
