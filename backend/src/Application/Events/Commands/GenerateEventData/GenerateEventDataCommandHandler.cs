using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Queries;
using backend.Domain.Enums;
using System.Text.Json;
using FluentValidation.Results;
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Events.Commands.GenerateEventData;

public class GenerateEventDataCommandHandler : IRequestHandler<GenerateEventDataCommand, Result<List<AIEventDto>>>
{
    private readonly IChatProviderFactory _chatProviderFactory;
    private readonly IValidator<AIEventDto> _aiEventDtoValidator;
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<GenerateEventDataCommandHandler> _logger;

    public GenerateEventDataCommandHandler(IChatProviderFactory chatProviderFactory, IValidator<AIEventDto> aiEventDtoValidator, IApplicationDbContext context, IUser user, IAuthorizationService authorizationService, ILogger<GenerateEventDataCommandHandler> logger)
    {
        _chatProviderFactory = chatProviderFactory;
        _aiEventDtoValidator = aiEventDtoValidator;
        _context = context;
        _user = user;
        _authorizationService = authorizationService;
        _logger = logger;
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
                    var currentUserProfile = await _authorizationService.GetCurrentUserProfileAsync(cancellationToken);
                    var families = await _context.Families
                        .Include(x => x.FamilyUsers)
                        .Where(f => f.Name == eventDto.FamilyName || f.Code == eventDto.FamilyName) // Map by name or code
                        .ToListAsync(cancellationToken);

                    var accessibleFamilies = new List<Family>();
                    foreach (var family in families)
                    {
                        if (_user.Roles != null && _user.Roles.Contains(SystemRole.Admin.ToString()))
                        {
                            accessibleFamilies.Add(family);
                        }
                        else if (family.FamilyUsers.Any(u => u.Role == FamilyRole.Manager && u.UserProfileId == currentUserProfile!.Id))
                        {
                            accessibleFamilies.Add(family);
                        }
                    }

                    if (accessibleFamilies.Count == 1)
                    {
                        eventDto.FamilyId = accessibleFamilies.First().Id;
                    }
                    else if (accessibleFamilies.Count == 0)
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
                    eventDto.ValidationErrors.AddRange(validationResult.Errors.Select(e => e.ErrorMessage));
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
