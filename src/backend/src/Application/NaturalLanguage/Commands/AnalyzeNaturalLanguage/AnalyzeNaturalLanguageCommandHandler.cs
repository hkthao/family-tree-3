using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.AI.Prompts;
using backend.Application.NaturalLanguage.Models;
using System.Text.Json;
using backend.Application.Common.Models.AI;
using backend.Application.Common.Exceptions; // Add using directive
using Microsoft.EntityFrameworkCore; // Add using directive
using System.Collections.Generic; // Add using directive
using System.Linq; // Add using directive

namespace backend.Application.NaturalLanguage.Commands.AnalyzeNaturalLanguage;

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

            // 5. Process Relationships (FatherId, MotherId, HusbandId, WifeId) for processedMembers
            foreach (var memberResult in processedMembers)
            {
                var originalMemberData = analyzedData.Members.FirstOrDefault(m => aiIdMap.ContainsKey(m.Id ?? "") && aiIdMap[m.Id ?? ""] == memberResult.Id);

                if (originalMemberData != null)
                {
                    if (!string.IsNullOrWhiteSpace(originalMemberData.FatherId) && aiIdMap.TryGetValue(originalMemberData.FatherId, out var fatherGuid))
                    {
                        memberResult.FatherId = fatherGuid;
                    }
                    if (!string.IsNullOrWhiteSpace(originalMemberData.MotherId) && aiIdMap.TryGetValue(originalMemberData.MotherId, out var motherGuid))
                    {
                        memberResult.MotherId = motherGuid;
                    }
                    if (!string.IsNullOrWhiteSpace(originalMemberData.HusbandId) && aiIdMap.TryGetValue(originalMemberData.HusbandId, out var husbandGuid))
                    {
                        memberResult.HusbandId = husbandGuid;
                    }
                    if (!string.IsNullOrWhiteSpace(originalMemberData.WifeId) && aiIdMap.TryGetValue(originalMemberData.WifeId, out var wifeGuid))
                    {
                        memberResult.WifeId = wifeGuid;
                    }
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

                processedEvents.Add(new EventResultDto
                {
                    Id = Guid.NewGuid(), // Generate new GUID for the event
                    Type = eventData.Type,
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
