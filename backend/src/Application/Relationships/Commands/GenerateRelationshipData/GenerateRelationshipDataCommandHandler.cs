using backend.Application.Common.Models;
using backend.Application.Common.Services;
using System.Text.Json;
using FluentValidation.Results;
using backend.Domain.Enums;
using backend.Application.Common.Interfaces;

namespace backend.Application.Relationships.Commands.GenerateRelationshipData;

public class GenerateRelationshipDataCommandHandler : IRequestHandler<GenerateRelationshipDataCommand, Result<List<AIRelationshipDto>>>
{
    private readonly IChatProviderFactory _chatProviderFactory;
    private readonly IValidator<AIRelationshipDto> _aiRelationshipDtoValidator;
    private readonly IApplicationDbContext _context;
    private readonly FamilyAuthorizationService _familyAuthorizationService;

    public GenerateRelationshipDataCommandHandler(IChatProviderFactory chatProviderFactory, IValidator<AIRelationshipDto> aiRelationshipDtoValidator, IApplicationDbContext context, FamilyAuthorizationService familyAuthorizationService)
    {
        _chatProviderFactory = chatProviderFactory;
        _aiRelationshipDtoValidator = aiRelationshipDtoValidator;
        _context = context;
        _familyAuthorizationService = familyAuthorizationService;
    }

    public async Task<Result<List<AIRelationshipDto>>> Handle(GenerateRelationshipDataCommand request, CancellationToken cancellationToken)
    {
        var chatProvider = _chatProviderFactory.GetProvider(ChatAIProvider.Local); // Assuming Local for now

        string systemPrompt = "You are an AI assistant that generates JSON data for relationship entities based on natural language descriptions. The output must always be a single JSON object containing one array: \"relationships\". Each object in the \"relationships\" array should have the following fields: - \"sourceMemberName\" (full name) - \"targetMemberName\" (full name) - \"type\" (e.g., Spouse, Parent, Child, Sibling) - \"startDate\" (YYYY-MM-DD) - \"endDate\" (YYYY-MM-DD) - \"description\". If details are missing, use placeholders (\"Unknown\" or null) instead of leaving fields empty. Infer the entity type (Relationship) from the prompt. If the prompt describes multiple entities, include them in the respective arrays. Example input: Tạo mối quan hệ giữa Nguyễn Văn A và Trần Thị B. Nguyễn Văn A là chồng của Trần Thị B. Example output: { \"relationships\": [ { \"sourceMemberName\": \"Nguyễn Văn A\", \"targetMemberName\": \"Trần Thị B\", \"type\" : \"Spouse\", \"description\": \"Married couple\" } ] }. Always respond with ONLY the JSON object. Do not include any conversational text.";

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
            return Result<List<AIRelationshipDto>>.Failure("AI did not return a response.");
        }

        try
        {
            var aiResponse = JsonSerializer.Deserialize<AIResponseData>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (aiResponse == null || aiResponse.Relationships == null || aiResponse.Relationships.Count == 0)
            {
                return Result<List<AIRelationshipDto>>.Success([]); // Return empty list if no relationships generated
            }

            foreach (var relationshipDto in aiResponse.Relationships)
            {
                relationshipDto.ValidationErrors ??= [];

                // Resolve SourceMemberId from SourceMemberName
                if (!string.IsNullOrWhiteSpace(relationshipDto.SourceMemberName))
                {
                    var sourceMember = await _context.Members
                        .Where(m => m.FullName == relationshipDto.SourceMemberName)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (sourceMember != null)
                    {
                        var authResult = await _familyAuthorizationService.AuthorizeFamilyAccess(sourceMember.FamilyId, cancellationToken);
                        if (authResult.IsSuccess)
                        {
                            relationshipDto.SourceMemberId = sourceMember.Id;
                        }
                        else
                        {
                            relationshipDto.ValidationErrors.Add(authResult.Error ?? $"No permission to access family of {relationshipDto.SourceMemberName}.");
                        }
                    }
                    else
                    {
                        relationshipDto.ValidationErrors.Add($"Source member '{relationshipDto.SourceMemberName}' not found.");
                    }
                }

                // Resolve TargetMemberId from TargetMemberName
                if (!string.IsNullOrWhiteSpace(relationshipDto.TargetMemberName))
                {
                    var targetMember = await _context.Members
                        .Where(m => m.FullName == relationshipDto.TargetMemberName)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (targetMember != null)
                    {
                        var authResult = await _familyAuthorizationService.AuthorizeFamilyAccess(targetMember.FamilyId, cancellationToken);
                        if (authResult.IsSuccess)
                        {
                            relationshipDto.TargetMemberId = targetMember.Id;
                        }
                        else
                        {
                            relationshipDto.ValidationErrors.Add(authResult.Error ?? $"No permission to access family of {relationshipDto.TargetMemberName}.");
                        }
                    }
                    else
                    {
                        relationshipDto.ValidationErrors.Add($"Target member '{relationshipDto.TargetMemberName}' not found.");
                    }
                }

                // Validate AIRelationshipDto
                ValidationResult validationResult = await _aiRelationshipDtoValidator.ValidateAsync(relationshipDto, cancellationToken);
                if (!validationResult.IsValid)
                {
                    relationshipDto.ValidationErrors.AddRange(validationResult.Errors.Where(e => e.ErrorMessage != null).Select(e => e.ErrorMessage!));
                }
            }
            return Result<List<AIRelationshipDto>>.Success(aiResponse.Relationships);
        }
        catch (JsonException ex)
        {
            return Result<List<AIRelationshipDto>>.Failure($"AI generated invalid JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<List<AIRelationshipDto>>.Failure($"An unexpected error occurred while processing AI response: {ex.Message}");
        }
    }

    private class AIResponseData
    {
        public List<AIRelationshipDto>? Relationships { get; set; }
    }
}
