using System.Text.Json;
using backend.Application.AI.DTOs; // For GenerateRequest
using backend.Application.Common.Interfaces; // For IAiGenerateService, IApplicationDbContext
using backend.Application.Common.Models; // For Result
using backend.Application.Families.Commands.GenerateFamilyData; // For GenerateFamilyDataCommand
using backend.Application.Families.DTOs; // For AnalyzedDataDto, MemberResultDto, EventResultDto, RelationshipResultDto, MemberDataDtoValidator, EventDataDtoValidator
using backend.Domain.Enums; // For EventType, RelationshipType
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace backend.Application.Families.Commands.GenerateFamilyData;

/// <summary>
/// Xử lý lệnh tạo dữ liệu gia đình có cấu trúc từ văn bản ngôn ngữ tự nhiên bằng AI.
/// </summary>
public class GenerateFamilyDataCommandHandler : IRequestHandler<GenerateFamilyDataCommand, Result<AnalyzedResultDto>>
{
    private readonly IAiGenerateService _aiGenerateService;
    private readonly IApplicationDbContext _context;

    public GenerateFamilyDataCommandHandler(IAiGenerateService aiGenerateService, IApplicationDbContext context)
    {
        _aiGenerateService = aiGenerateService;
        _context = context;
    }

    public async Task<Result<AnalyzedResultDto>> Handle(GenerateFamilyDataCommand request, CancellationToken cancellationToken)
    {
        // 1. Chuẩn bị request cho AI Generate Service
        var generateRequest = new GenerateRequest
        {
            SessionId = request.SessionId,
            ChatInput = request.Content,
            SystemPrompt = BuildSystemPrompt(request.FamilyId), // Xây dựng SystemPrompt
            Metadata = new Dictionary<string, object>
            {
                { "familyId", request.FamilyId.ToString() }
            }
        };

        // 2. Gửi request đến AI Generate Service
        var generateResult = await _aiGenerateService.GenerateDataAsync<AnalyzedDataDto>(generateRequest, cancellationToken);

        if (!generateResult.IsSuccess)
        {
            throw new Common.Exceptions.ValidationException(generateResult.Error ?? "Lỗi không xác định từ AiGenerateService.");
        }

        // 3. Phân tích phản hồi từ AI (dự kiến là JSON AnalyzedDataDto)
        var analyzedData = generateResult.Value;

        if (analyzedData == null)
        {
            return Result<AnalyzedResultDto>.Failure("Phản hồi từ AI trống hoặc không hợp lệ.", "AIResponse");
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

    private string BuildSystemPrompt(Guid familyId)
    {
        return @$"Bạn là một bộ phân tích dữ liệu gia phả. Nhiệm vụ của bạn là **luôn trả về một đối tượng JSON duy nhất** theo schema dưới đây. 
**Tuyệt đối không thêm chữ, giải thích hay văn bản nào khác.**

QUAN TRỌNG:
- Suy luận giới tính từ các từ 'ông', 'bà', 'anh', 'chị', 'chú', 'cô', 'cậu', 'dì'.
- Nếu giới tính có thể suy luận, gán: ""Male"" hoặc ""Female"". Nếu không thể suy luận, gán ""Other"".
- Họ (lastName) là từ đầu tiên của tên tiếng Việt, tên còn lại là firstName.
- Gán ID tạm thời duy nhất cho mỗi thành viên: ""temp_A"", ""temp_B"", v.v. Nếu thành viên đã được đề cập trước đó, dùng lại ID đó.
- Chỉ tạo events đầy đủ (có ""type"", ""description"", ""date"").
- **Relationships chỉ được phép sử dụng giá trị sau cho type**: ""Father"", ""Mother"", ""Husband"", ""Wife"".
- Không tạo event trống hoặc relationship không hợp lệ.
- Trả đúng cấu trúc JSON, tuân theo schema dưới đây.

Schema JSON:

{{
  ""members"": [
    {{
      ""id"": ""string (temporary unique identifier) | null"",
      ""code"": ""string | null"",
      ""lastName"": ""string"",
      ""firstName"": ""string"",
      ""dateOfBirth"": ""string | null"",
      ""dateOfDeath"": ""string | null"",
      ""gender"": ""Male"" | ""Female"" | ""Other"",
      ""order"": ""number | null""
    }}
  ],
  ""events"": [
    {{
      ""type"": ""string"",
      ""description"": ""string"",
      ""date"": ""string | null"",
      ""location"": ""string | null"",
      ""relatedMemberIds"": [""string""]
    }}
  ],
  ""relationships"": [
    {{
      ""sourceMemberId"": ""string"",
      ""targetMemberId"": ""string"",
      ""type"": ""Father"" | ""Mother"" | ""Husband"" | ""Wife"",
      ""order"": ""number | null""
    }}
  ],
  ""feedback"": ""string | null""
}}

**BẮT BUỘC:**
- Trả **chỉ JSON hợp lệ**, không text nào khác, không xuống dòng thừa.
- Gender phải là **Male**, **Female**, hoặc **Other**.
- Event phải đầy đủ ""type"", ""description"", ""date"".
- Relationship type chỉ được phép là ""Father"", ""Mother"", ""Husband"", hoặc ""Wife"".";
    }
}
