using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;
using MediatR;
using System.Text.Json;

namespace Application.NaturalLanguageInput.Commands.GenerateData;

public class GenerateDataCommandHandler : IRequestHandler<GenerateDataCommand, Result<string>>
{
    private readonly IChatProviderFactory _chatProviderFactory;

    public GenerateDataCommandHandler(IChatProviderFactory chatProviderFactory)
    {
        _chatProviderFactory = chatProviderFactory;
    }

    public async Task<Result<string>> Handle(GenerateDataCommand request, CancellationToken cancellationToken)
    {
        // For now, we'll use a default AI provider. In a real scenario, this might be configurable.
        var chatProvider = _chatProviderFactory.GetProvider(backend.Domain.Enums.ChatAIProvider.Local);

        // Construct a prompt for the AI to generate JSON for family/member data
        // This prompt needs to be carefully engineered to guide the AI
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

        var chatMessages = new List<backend.Application.Common.Models.ChatMessage>
        {
            new backend.Application.Common.Models.ChatMessage { Role = "system", Content = systemPrompt },
            new backend.Application.Common.Models.ChatMessage { Role = "user", Content = userPrompt }
        };

        string jsonString = await chatProvider.GenerateResponseAsync(chatMessages);

        if (string.IsNullOrWhiteSpace(jsonString))
        {
            return Result<string>.Failure("AI did not return a response.");
        }

        // Basic validation to ensure it's valid JSON
        try
        {
            JsonDocument.Parse(jsonString);
            return Result<string>.Success(jsonString);
        }
        catch (JsonException)
        {
            return Result<string>.Failure("AI generated invalid JSON. Please try again or refine your prompt.");
        }
    }
}