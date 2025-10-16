using backend.Application.Common.Interfaces;
using backend.Application.Common.Services; // Added
using backend.Application.Common.Models;
using backend.Application.Events.Queries;
using backend.Domain.Enums;
using System.Text.Json;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace backend.Application.Events.Commands.GenerateEventData;

public class GenerateEventDataCommandHandler : IRequestHandler<GenerateEventDataCommand, Result<List<AIEventDto>>>
{
    private readonly IChatProviderFactory _chatProviderFactory;
    private readonly IValidator<AIEventDto> _aiEventDtoValidator;
    private readonly IApplicationDbContext _context;
    private readonly FamilyAuthorizationService _familyAuthorizationService;

    public GenerateEventDataCommandHandler(IChatProviderFactory chatProviderFactory, IValidator<AIEventDto> aiEventDtoValidator, IApplicationDbContext context, FamilyAuthorizationService familyAuthorizationService)
    {
        _chatProviderFactory = chatProviderFactory;
        _aiEventDtoValidator = aiEventDtoValidator;
        _context = context;
        _familyAuthorizationService = familyAuthorizationService;
    }

    public async Task<Result<List<AIEventDto>>> Handle(GenerateEventDataCommand request, CancellationToken cancellationToken)
    {
        var chatProvider = _chatProviderFactory.GetProvider(ChatAIProvider.Local);

        string systemPrompt = "You are an AI assistant that generates JSON data for event entities based on natural language descriptions. The output must always be a single JSON object containing one array: \"events\". Each object in the \"events\" array should have the following fields: - \"name\" - \"type\" (Birth, Marriage, Death, Migration, Other) - \"startDate\" (YYYY-MM-DD) - \"endDate\" (YYYY-MM-DD) - \"location\" - \"description\" - \"familyName\" - \"relatedMembers\" (an array of strings, where each string can be a member's full name or code). If \"type\" is not explicitly mentioned, default it to \"Other\". If only a year is provided for \"startDate\" or \"endDate\", default the date to \"YYYY-01-01\". If details are missing, use placeholders (\"Unknown\" or null) instead of leaving fields empty. Infer the entity type (Event) from the prompt. If the prompt describes multiple entities, include them in the respective arrays. Example input: Thêm sự kiện tên Đám cưới của Trần Văn A và Nguyễn Thị B, ngày 2000-01-01, tại Hà Nội, thuộc gia đình Nguyễn. Thành viên liên quan: Trần Văn A, Nguyễn Thị B. Example output: { \"events\": [ { \"name\": \"Đám cưới của Trần Văn A và Nguyễn Thị B\", \"type\": \"Marriage\", \"startDate\": \"2000-01-01\", \"location\": \"Hà Nội\", \"familyName\": \"Nguyễn\", \"relatedMembers\": [\"Trần Văn A\", \"Nguyễn Thị B\"] } ] }. Always respond with ONLY the JSON object. Do not include any conversational text.";

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
            return Result<List<AIEventDto>>.Failure("AI did not return a response.");
        }

        try
        {
            var aiResponse = JsonSerializer.Deserialize<AIResponseData>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (aiResponse == null || aiResponse.Events == null || aiResponse.Events.Count == 0)
            {
                return Result<List<AIEventDto>>.Success([]); // Return empty list if no events generated
            }

            foreach (var eventDto in aiResponse.Events)
            {
                eventDto.ValidationErrors ??= [];

                // Default Type if missing
                if (string.IsNullOrWhiteSpace(eventDto.Type))
                    eventDto.Type = EventType.Other.ToString();

                // Resolve FamilyId from FamilyName
                if (!string.IsNullOrWhiteSpace(eventDto.FamilyName))
                {
                    var families = await _context.Families
                        .Where(f => f.Name == eventDto.FamilyName || f.Code == eventDto.FamilyName) // Map by name or code
                        .ToListAsync(cancellationToken);

                    if (families.Count == 1)
                    {
                        var family = families.First();
                        var authResult = await _familyAuthorizationService.AuthorizeFamilyAccess(family.Id, cancellationToken);
                        if (authResult.IsSuccess)
                        {
                            eventDto.FamilyId = family.Id;
                        }
                        else if (authResult.Error != null)
                        {
                            eventDto.ValidationErrors.Add(authResult.Error!);
                        }
                    }
                    else if (families.Count == 0)
                    {
                        eventDto.ValidationErrors.Add($"Family '{eventDto.FamilyName}' not found or you do not have permission to manage it.");
                    }
                    else
                    {
                        eventDto.ValidationErrors.Add($"Multiple families found with name or code '{eventDto.FamilyName}'. Please specify.");
                    }
                }

                // Resolve RelatedMembers from names or codes
                if (eventDto.RelatedMembers != null && eventDto.RelatedMembers.Any() && eventDto.FamilyId.HasValue)
                {
                    var memberIds = new List<Guid>();
                    foreach (var memberIdentifier in eventDto.RelatedMembers)
                    {
                        var members = await _context.Members
                            .Where(m => m.FamilyId == eventDto.FamilyId.Value && (m.FullName == memberIdentifier || m.Code == memberIdentifier))
                            .ToListAsync(cancellationToken);

                        if (members.Count == 1)
                        {
                            memberIds.Add(members.First().Id);
                        }
                        else if (members.Count == 0)
                        {
                            eventDto.ValidationErrors.Add($"Related member '{memberIdentifier}' not found in family '{eventDto.FamilyName}'.");
                        }
                        else
                        {
                            eventDto.ValidationErrors.Add($"Multiple members found with name or code '{memberIdentifier}' in family '{eventDto.FamilyName}'. Please specify.");
                        }
                    }
                    // Note: We are not storing memberIds directly in AIEventDto, but this is where you would typically map them to the Event entity.
                    // For now, we just validate their existence.
                }

                // Validate AIEventDto
                ValidationResult validationResult = await _aiEventDtoValidator.ValidateAsync(eventDto, cancellationToken);
                if (!validationResult.IsValid)
                {
                    eventDto.ValidationErrors.AddRange(validationResult.Errors.Where(e => e.ErrorMessage != null).Select(e => e.ErrorMessage!));
                }
            }

            return Result<List<AIEventDto>>.Success(aiResponse.Events);
        }
        catch (JsonException ex)
        {
            return Result<List<AIEventDto>>.Failure($"AI generated invalid JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<List<AIEventDto>>.Failure($"An unexpected error occurred while processing AI response: {ex.Message}");
        }
    }
    private class AIResponseData
    {
        public List<AIEventDto>? Events { get; set; }
    }
}
