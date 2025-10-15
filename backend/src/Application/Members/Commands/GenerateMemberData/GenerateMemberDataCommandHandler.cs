using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Queries;
using backend.Domain.Enums;
using System.Text.Json;
using FluentValidation.Results;

namespace backend.Application.Members.Commands.GenerateMemberData;

public class GenerateMemberDataCommandHandler : IRequestHandler<GenerateMemberDataCommand, Result<List<MemberDto>>>
{
    private readonly IChatProviderFactory _chatProviderFactory;
    private readonly IValidator<MemberDto> _memberDtoValidator;

    public GenerateMemberDataCommandHandler(IChatProviderFactory chatProviderFactory, IValidator<MemberDto> memberDtoValidator)
    {
        _chatProviderFactory = chatProviderFactory;
        _memberDtoValidator = memberDtoValidator;
    }

    public async Task<Result<List<MemberDto>>> Handle(GenerateMemberDataCommand request, CancellationToken cancellationToken)
    {
        var chatProvider = _chatProviderFactory.GetProvider(ChatAIProvider.Local);

        var systemPrompt = @"You are an AI assistant that generates JSON data for member entities based on natural language descriptions.
The output should always be a single JSON object containing one array: 'members'.
Each object in the 'members' array should have 'fullName', 'gender' (Male, Female, Other), 'dateOfBirth' (YYYY-MM-DD), 'dateOfDeath' (YYYY-MM-DD), 'placeOfBirth', 'placeOfDeath', 'occupation', 'biography', and 'familyId'.
If 'gender' is not explicitly mentioned in the prompt, default it to 'Other'.
Infer the entity type (Member) from the prompt. If the prompt describes multiple entities, include them in the respective arrays.
If details are missing, use placeholders (""Unknown"" or null) instead of leaving fields empty.
Example: 'Thêm thành viên tên Trần Văn A, sinh năm 1990, giới tính Nam, thuộc gia đình Nguyễn.'
Output: { ""members"": [{ ""fullName"": ""Trần Văn A"", ""dateOfBirth"": ""1990-01-01"", ""gender"": ""Male"", ""familyId"": ""<guid-of-nguyen-family>"", ""occupation"": ""Unknown"" }] }
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

            if (aiResponse == null || aiResponse.Members == null || !aiResponse.Members.Any())
            {
                return Result<List<MemberDto>>.Success(new List<MemberDto>()); // Return empty list if no members generated
            }

            foreach (var member in aiResponse.Members)
            {
                if (string.IsNullOrWhiteSpace(member.Gender))
                    member.Gender = Gender.Other.ToString();

                ValidationResult validationResult = await _memberDtoValidator.ValidateAsync(member, cancellationToken);
                if (!validationResult.IsValid)
                {
                    member.ValidationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                }
            }

            return Result<List<MemberDto>>.Success(aiResponse.Members);
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