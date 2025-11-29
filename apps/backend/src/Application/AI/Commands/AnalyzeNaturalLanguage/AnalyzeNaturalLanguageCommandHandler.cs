using System.Text.Json;
using backend.Application.AI.Models; // UPDATED USING
using backend.Application.AI.Prompts;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums; // Add using directive

namespace backend.Application.AI.Commands.AnalyzeNaturalLanguage;

/// <summary>
/// Xử lý lệnh phân tích văn bản ngôn ngữ tự nhiên.
/// </summary>
public class AnalyzeNaturalLanguageCommandHandler : IRequestHandler<AnalyzeNaturalLanguageCommand, Result<AnalyzedResultDto>>
{
    private readonly IN8nService _n8nService;
    private readonly IApplicationDbContext _context;

    public AnalyzeNaturalLanguageCommandHandler(IN8nService n8nService, IApplicationDbContext context)
    {
        _n8nService = n8nService;
        _context = context;
    }

    public async Task<Result<AnalyzedResultDto>> Handle(AnalyzeNaturalLanguageCommand request, CancellationToken cancellationToken)
    {
        var sessionId = request.SessionId;

        // 1. Xây dựng prompt cho AI Agent
        var aiPrompt = PromptBuilder.BuildNaturalLanguageAnalysisPrompt(request.Content);

        // 2. Gửi prompt đến n8n Service chat
        var n8nResult = await _n8nService.CallChatWebhookAsync(sessionId, aiPrompt, cancellationToken);

        if (!n8nResult.IsSuccess)
        {
            throw new Common.Exceptions.ValidationException(n8nResult.Error ?? "Lỗi không xác định từ N8nService.");
        }

        // 3. Phân tích phản hồi từ AI (dự kiến là JSON)
        if (string.IsNullOrWhiteSpace(n8nResult.Value))
        {
            return Result<AnalyzedResultDto>.Failure("Phản hồi từ AI trống hoặc không hợp lệ.", "AIResponse");
        }

        try
        {
            var analyzedData = JsonSerializer.Deserialize<AnalyzedDataDto>(n8nResult.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (analyzedData == null)
            {
                return Result<AnalyzedResultDto>.Failure("Phản hồi từ AI không hợp lệ hoặc trống.", "AIResponse");
            }

            // Initialize ID mapping for AI's temporary IDs to actual GUIDs
            var aiIdMap = new Dictionary<string, Guid>();
            var processedMembers = new List<MemberResultDto>();
            var processedEvents = new List<EventResultDto>();

            // 4. Process Members: Resolve/Generate GUIDs and map data
            var familyId = request.FamilyId;
            var memberValidator = new MemberDataDtoValidator();

            foreach (var memberData in analyzedData.Members)
            {
                Guid memberGuid;
                Domain.Entities.Member? existingMember = null;

                // Try to find existing member by Code if provided
                if (!string.IsNullOrWhiteSpace(memberData.Code))
                {
                    existingMember = await _context.Members
                        .AsNoTracking()
                        .FirstOrDefaultAsync(m => m.Code == memberData.Code && m.FamilyId == familyId, cancellationToken);
                }

                if (existingMember != null)
                {
                    memberGuid = existingMember.Id;
                    memberData.IsExisting = true; // Mark as existing
                }
                else
                {
                    memberGuid = Guid.NewGuid(); // Generate new GUID for new members
                }

                // Store mapping from AI's temporary ID to actual GUID
                if (!string.IsNullOrWhiteSpace(memberData.Id))
                {
                    aiIdMap[memberData.Id] = memberGuid;
                }

                // Validate member data
                var validationResult = await memberValidator.ValidateAsync(memberData, cancellationToken);
                if (!validationResult.IsValid)
                {
                    memberData.ErrorMessage = string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage));
                }

                // Create MemberResultDto
                processedMembers.Add(new MemberResultDto
                {
                    Id = memberGuid,
                    Code = memberData.Code,
                    LastName = memberData.LastName,
                    FirstName = memberData.FirstName,
                    DateOfBirth = memberData.DateOfBirth,
                    DateOfDeath = memberData.DateOfDeath,
                    Gender = memberData.Gender,
                    Order = memberData.Order,
                    IsExisting = memberData.IsExisting,
                    ErrorMessage = memberData.ErrorMessage
                });
            }

            // 5. Process Relationships: Create RelationshipResultDto objects
            var processedRelationships = new List<RelationshipResultDto>();
            foreach (var relationshipData in analyzedData.Relationships)
            {
                string? errorMessage = null;
                Guid sourceMemberGuid = Guid.Empty;
                Guid targetMemberGuid = Guid.Empty;
                RelationshipType parsedType = Domain.Enums.RelationshipType.Father; // Default value

                // Validate SourceMemberId
                if (string.IsNullOrWhiteSpace(relationshipData.SourceMemberId))
                {
                    errorMessage += "SourceMemberId is missing. ";
                }
                else if (!aiIdMap.TryGetValue(relationshipData.SourceMemberId, out sourceMemberGuid))
                {
                    errorMessage += $"SourceMemberId '{relationshipData.SourceMemberId}' not mapped. ";
                }

                // Validate TargetMemberId
                if (string.IsNullOrWhiteSpace(relationshipData.TargetMemberId))
                {
                    errorMessage += "TargetMemberId is missing. ";
                }
                else if (!aiIdMap.TryGetValue(relationshipData.TargetMemberId, out targetMemberGuid))
                {
                    errorMessage += $"TargetMemberId '{relationshipData.TargetMemberId}' not mapped. ";
                }

                // Validate RelationshipType
                if (string.IsNullOrWhiteSpace(relationshipData.Type))
                {
                    errorMessage += "RelationshipType is missing. ";
                }
                else
                {
                    // Try parsing as enum name (string)
                    if (Enum.TryParse(relationshipData.Type, true, out parsedType))
                    {
                        // Check if the parsed type is one of the allowed types
                        if (!Enum.IsDefined(typeof(RelationshipType), parsedType))
                        {
                            errorMessage += $"Invalid RelationshipType: '{relationshipData.Type}'. Expected one of: {string.Join(", ", Enum.GetNames(typeof(RelationshipType)))}. ";
                        }
                    }
                    // Try parsing as integer value
                    else if (int.TryParse(relationshipData.Type, out int typeInt) && Enum.IsDefined(typeof(RelationshipType), typeInt))
                    {
                        parsedType = (RelationshipType)typeInt;
                    }
                    else
                    {
                        errorMessage += $"Invalid RelationshipType: '{relationshipData.Type}'. Expected one of: {string.Join(", ", Enum.GetNames(typeof(RelationshipType)))} or integer values. ";
                    }
                }

                if (errorMessage == null)
                {
                    processedRelationships.Add(new RelationshipResultDto
                    {
                        SourceMemberId = sourceMemberGuid,
                        TargetMemberId = targetMemberGuid,
                        Type = parsedType,
                        Order = relationshipData.Order
                    });
                }
            }

            // 6. Process Events: Generate GUIDs and map related member IDs
            var eventValidator = new EventDataDtoValidator();
            foreach (var eventData in analyzedData.Events)
            {
                // Validate event data
                var validationResult = await eventValidator.ValidateAsync(eventData, cancellationToken);
                if (!validationResult.IsValid)
                {
                    eventData.ErrorMessage = string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage));
                }

                var relatedMemberGuids = new List<Guid>();
                foreach (var aiMemberId in eventData.RelatedMemberIds)
                {
                    if (aiIdMap.TryGetValue(aiMemberId, out var memberGuid))
                    {
                        relatedMemberGuids.Add(memberGuid);
                    }
                    // Optionally, handle cases where AI provided an ID that wasn't mapped
                }

                EventType parsedEventType = EventType.Other; // Default value

                if (!string.IsNullOrWhiteSpace(eventData.Type))
                {
                    if (Enum.TryParse(eventData.Type, true, out parsedEventType))
                    {
                        if (!Enum.IsDefined(typeof(EventType), parsedEventType))
                        {
                            // This case should ideally not happen if Enum.TryParse returns true
                            // but good for robustness.
                            parsedEventType = EventType.Other;
                        }
                    }
                    else
                    {
                        // If parsing by name fails, default to Other
                        parsedEventType = EventType.Other;
                    }
                }

                processedEvents.Add(new EventResultDto
                {
                    Id = Guid.NewGuid(), // Generate new GUID for the event
                    Type = parsedEventType, // Assign the parsed enum value
                    Description = eventData.Description,
                    Date = eventData.Date,
                    Location = eventData.Location,
                    RelatedMemberIds = relatedMemberGuids,
                    ErrorMessage = eventData.ErrorMessage
                });
            }

            // 7. Construct and return AnalyzedResultDto
            var analyzedResult = new AnalyzedResultDto
            {
                Members = processedMembers,
                Events = processedEvents,
                Relationships = processedRelationships,
                Feedback = analyzedData.Feedback
            };

            return Result<AnalyzedResultDto>.Success(analyzedResult);
        }
        catch (JsonException ex)
        {
            return Result<AnalyzedResultDto>.Failure($"Không thể phân tích phản hồi JSON từ AI: {ex.Message}", "AIResponseParsing");
        }
        catch (Exception ex)
        {
            return Result<AnalyzedResultDto>.Failure($"Đã xảy ra lỗi khi xử lý phản hồi từ AI: {ex.Message}", "AIResponseProcessing");
        }
    }
}
