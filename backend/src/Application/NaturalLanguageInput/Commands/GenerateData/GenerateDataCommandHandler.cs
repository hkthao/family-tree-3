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
The output should always be a JSON object.
For a 'Family' entity, the JSON should have 'name', 'description', 'address', and 'visibility' (Public, Private, Shared).
For a 'Member' entity, the JSON should have 'fullName', 'gender' (Male, Female, Other), 'dateOfBirth', 'dateOfDeath', 'placeOfBirth', 'placeOfDeath', 'occupation', and 'biography'.
Infer the entity type (Family or Member) from the prompt.
If the prompt is ambiguous or lacks sufficient detail, ask for clarification or provide a best-effort JSON with placeholders.
Example for Family: 'Tạo một gia đình tên Nguyễn với mô tả là gia đình truyền thống ở Hà Nội, công khai.'
Output: { ""name"": ""Gia đình Nguyễn"", ""description"": ""gia đình truyền thống ở Hà Nội"", ""address"": ""Hà Nội"", ""visibility"": ""Public"" }
Example for Member: 'Thêm thành viên tên Trần Văn A, sinh ngày 1/1/1990, nghề nghiệp kỹ sư.'
Output: { ""fullName"": ""Trần Văn A"", ""gender"": ""Male"", ""dateOfBirth"": ""1990-01-01"", ""occupation"": ""Kỹ sư"" }
Always respond with ONLY the JSON object. Do not include any conversational text.";

        var userPrompt = request.Prompt;

        var chatMessages = new List<ChatMessage>
        {
            new() { Role = "system", Content = systemPrompt },
            new() { Role = "user", Content = userPrompt }
        };

        string jsonString = await chatProvider.GenerateResponseAsync(chatMessages);

        if (string.IsNullOrWhiteSpace(jsonString))
        {
            return Result<GeneratedEntityDto>.Failure("AI did not return a response.");
        }

        try
        {
            using JsonDocument doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;

            if (root.TryGetProperty("name", out _) && root.TryGetProperty("visibility", out _))
            {
                var familyDto = JsonSerializer.Deserialize<FamilyDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (familyDto != null) {
                    return Result<GeneratedEntityDto>.Success(new GeneratedEntityDto { DataType = "Family", Family = familyDto });
                }
            }
            else if (root.TryGetProperty("fullName", out _) && root.TryGetProperty("gender", out _))
            {
                var memberDto = JsonSerializer.Deserialize<MemberDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (memberDto != null) {
                    return Result<GeneratedEntityDto>.Success(new GeneratedEntityDto { DataType = "Member", Member = memberDto });
                }
            }
            return Result<GeneratedEntityDto>.Failure("AI generated JSON that does not match Family or Member schema.");
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
}