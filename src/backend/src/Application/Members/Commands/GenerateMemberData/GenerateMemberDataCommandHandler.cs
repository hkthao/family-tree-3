using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Queries;
using backend.Domain.Enums;
using FluentValidation.Results;

namespace backend.Application.Members.Commands.GenerateMemberData;

public class GenerateMemberDataCommandHandler(IChatProviderFactory chatProviderFactory, IValidator<AIMemberDto> aiMemberDtoValidator, IApplicationDbContext context, IAuthorizationService authorizationService) : IRequestHandler<GenerateMemberDataCommand, Result<List<AIMemberDto>>>
{
    private readonly IChatProviderFactory _chatProviderFactory = chatProviderFactory;
    private readonly IValidator<AIMemberDto> _aiMemberDtoValidator = aiMemberDtoValidator;
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<List<AIMemberDto>>> Handle(GenerateMemberDataCommand request, CancellationToken cancellationToken)
    {
        var chatProvider = _chatProviderFactory.GetProvider(ChatAIProvider.Local);

        string systemPrompt = "You are an AI assistant that generates JSON data for member entities based on natural language descriptions. The output must always be a single JSON object containing one array: \"members\". Each object in the \"members\" array should have the following fields: - \"firstName\" - \"lastName\" - \"nickname\" - \"gender\" (Male, Female, Other) - \"dateOfBirth\" (YYYY-MM-DD) - \"dateOfDeath\" (YYYY-MM-DD) - \"placeOfBirth\" - \"placeOfDeath\" - \"occupation\" - \"biography\" - \"avatarUrl\" - \"familyName\". If the full name is given (e.g., \"Trần Văn A\"), automatically split it into: \"lastName\": the first word (\"Trần\"), \"firstName\": the remaining part (\"Văn A\"). If \"gender\" is not explicitly mentioned, default it to \"Other\". If only a year is provided for \"dateOfBirth\" or \"dateOfDeath\", default the date to \"YYYY-01-01\". If details are missing, use placeholders (\"Unknown\" or null) instead of leaving fields empty. Infer the entity type (Member) from the prompt. If the prompt describes multiple entities, include them in the respective arrays. Example input: Thêm thành viên tên Trần Văn A, sinh năm 1990, giới tính Nam, thuộc gia đình Nguyễn. Nơi sinh: Hà Nội. Nghề nghiệp: Kỹ sư. Tiểu sử: Một kỹ sư tài năng và đam mê công nghệ. Ảnh đại diện: https://example.com/avatar.png. Biệt danh: A Kỹ sư. Example output: { \"members\": [ { \"firstName\": \"Văn A\", \"lastName\": \"Trần\", \"nickname\": \"A Kỹ sư\", \"gender\": \"Male\", \"dateOfBirth\": \"1990-01-01\", \"placeOfBirth\": \"Hà Nội\", \"occupation\": \"Kỹ sư\", \"biography\": \"Một kỹ sư tài năng và đam mê công nghệ.\", \"avatarUrl\": \"https://example.com/avatar.png\", \"familyName\": \"Nguyễn\" } ] }. Always respond with ONLY the JSON object. Do not include any conversational text.";

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
            return Result<List<AIMemberDto>>.Failure("AI did not return a response.");
        }

        try
        {
            var aiResponse = JsonSerializer.Deserialize<AIResponseData>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (aiResponse == null || aiResponse.Members == null || aiResponse.Members.Count == 0)
            {
                return Result<List<AIMemberDto>>.Success([]); // Return empty list if no members generated
            }

            foreach (var memberDto in aiResponse.Members)
            {
                memberDto.ValidationErrors ??= [];

                // Default Gender if missing
                if (string.IsNullOrWhiteSpace(memberDto.Gender))
                    memberDto.Gender = Gender.Other.ToString();

                // Resolve FamilyId from FamilyName
                if (!string.IsNullOrWhiteSpace(memberDto.FamilyName))
                {
                    var families = await _context.Families
                        .Include(x => x.FamilyUsers)
                        .Where(f => f.Name == memberDto.FamilyName)
                        .ToListAsync(cancellationToken);

                    if (families.Count == 1)
                    {
                        var family = families.First();
                        var authResult = _authorizationService.CanAccessFamily(family.Id);
                        if (authResult)
                            memberDto.FamilyId = family.Id;
                        else
                            memberDto.ValidationErrors.Add("you do not have permission to access family");
                    }
                    else if (families.Count == 0)
                    {
                        memberDto.ValidationErrors.Add($"Family '{memberDto.FamilyName}' not found or you do not have permission to manage it.");
                    }
                    else
                    {
                        memberDto.ValidationErrors.Add($"Multiple families found with name '{memberDto.FamilyName}'. Please specify.");
                    }
                }

                // Validate AIMemberDto
                ValidationResult validationResult = await _aiMemberDtoValidator.ValidateAsync(memberDto, cancellationToken);
                if (!validationResult.IsValid)
                {
                    memberDto.ValidationErrors.AddRange(validationResult.Errors.Where(e => e.ErrorMessage != null).Select(e => e.ErrorMessage!));
                }
            }

            return Result<List<AIMemberDto>>.Success(aiResponse.Members);
        }
        catch (JsonException ex)
        {
            return Result<List<AIMemberDto>>.Failure($"AI generated invalid JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<List<AIMemberDto>>.Failure($"An unexpected error occurred while processing AI response: {ex.Message}");
        }
    }
    private class AIResponseData
    {
        public List<AIMemberDto>? Members { get; set; }
    }
}
