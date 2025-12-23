using System.Text;
using backend.Application.AI.DTOs;
using backend.Application.Common.Interfaces;
using backend.Application.Families.Commands.IncrementFamilyAiChatUsage; // ADDED
using backend.Application.Prompts.Queries.GetPromptById; // Add this
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Interfaces;
using backend.Domain.ValueObjects;
using Microsoft.Extensions.Logging; // Add this using directive

namespace backend.Application.Services;

public class RelationshipDetectionService : IRelationshipDetectionService
{
    private readonly IApplicationDbContext _context;
    private readonly IRelationshipGraph _relationshipGraph;
    private readonly IAiGenerateService _aiGenerateService;
    private readonly IRelationshipRuleEngine _ruleEngine;
    private readonly IMediator _mediator;
    private readonly ILogger<RelationshipDetectionService> _logger; // Inject ILogger

    public RelationshipDetectionService(IApplicationDbContext context, IRelationshipGraph relationshipGraph, IAiGenerateService aiGenerateService, IRelationshipRuleEngine ruleEngine, IMediator mediator, ILogger<RelationshipDetectionService> logger)
    {
        _context = context;
        _relationshipGraph = relationshipGraph;
        _aiGenerateService = aiGenerateService;
        _ruleEngine = ruleEngine;
        _mediator = mediator;
        _logger = logger; // Initialize _logger
    }

    public async Task<RelationshipDetectionResult> DetectRelationshipAsync(Guid familyId, Guid memberAId, Guid memberBId, CancellationToken cancellationToken)
    {
        var members = await _context.Members
                                    .Where(m => m.FamilyId == familyId)
                                    .ToListAsync(cancellationToken);

        var relationships = await _context.Relationships
                                        .Where(r => r.FamilyId == familyId)
                                        .ToListAsync(cancellationToken);

        var allMembers = members.ToDictionary(m => m.Id);

        _relationshipGraph.BuildGraph(members, relationships);

        var pathToB = _relationshipGraph.FindShortestPath(memberAId, memberBId);
        var pathToA = _relationshipGraph.FindShortestPath(memberBId, memberAId);

        string inferredDescription = "unknown";

        var memberA = allMembers.GetValueOrDefault(memberAId);
        var memberB = allMembers.GetValueOrDefault(memberBId);

        if (memberA == null || memberB == null)
        {
            return new RelationshipDetectionResult
            {
                Description = "unknown",
                Path = new List<Guid>(),
                Edges = new List<string>()
            };
        }

        // --- Step 1: Try to infer relationships using local rules first ---
        string localFromAToB = "unknown";
        string localFromBToA = "unknown";

        if (pathToB.NodeIds.Any())
        {
            localFromAToB = _ruleEngine.InferRelationship(pathToB, allMembers);
        }
        if (pathToA.NodeIds.Any())
        {
            localFromBToA = _ruleEngine.InferRelationship(pathToA, allMembers);
        }

        // If both relationships are found by local rules and are not "unknown", return immediately
        if (localFromAToB != "unknown" && localFromBToA != "unknown")
        {
            // Construct a descriptive string from local results
            inferredDescription = $"{memberA.FullName} là {localFromAToB} của {memberB.FullName} và {memberB.FullName} là {localFromBToA} của {memberA.FullName}.";

            return new RelationshipDetectionResult
            {
                Description = inferredDescription,
                Path = pathToB.NodeIds,
                Edges = pathToB.Edges.Select(e => e.Type.ToString()).ToList()
            };
        }
        // --- End of local inference ---


        var combinedPromptBuilder = new StringBuilder();

        combinedPromptBuilder.AppendLine($"Bạn hãy xác định mối quan hệ gia đình giữa Thành viên {memberA.FullName}{GetVietnameseAgeTerm(memberA.DateOfBirth)} {GetVietnameseGenderTerm(memberA.Gender)} và Thành viên {memberB.FullName}{GetVietnameseAgeTerm(memberB.DateOfBirth)} {GetVietnameseGenderTerm(memberB.Gender)} trong cây gia phả dựa trên các đường dẫn sau:");

        if (pathToB.NodeIds.Any())
        {
            combinedPromptBuilder.AppendLine($"- Đường dẫn từ {memberA.FullName} đến {memberB.FullName}: {_DescribePathInNaturalLanguageConcise(pathToB, allMembers)}");
        }
        else
        {
            combinedPromptBuilder.AppendLine($"- Không có đường dẫn trực tiếp từ {memberA.FullName} đến {memberB.FullName}.");
        }

        if (pathToA.NodeIds.Any())
        {
            combinedPromptBuilder.AppendLine($"- Đường dẫn từ {memberB.FullName} đến {memberA.FullName}: {_DescribePathInNaturalLanguageConcise(pathToA, allMembers)}");
        }
        else
        {
            combinedPromptBuilder.AppendLine($"- Không có đường dẫn trực tiếp từ {memberB.FullName} đến {memberA.FullName}.");
        }
        combinedPromptBuilder.AppendLine("Dựa vào thông tin trên, hãy suy luận mối quan hệ trực tiếp trong gia đình giữa hai thành viên và mô tả một cách NGẮN GỌN, SÚC TÍCH bằng tiếng Việt. Tránh liệt kê lại chi tiết đường dẫn.");
        combinedPromptBuilder.AppendLine("Ví dụ: 'A là cha của B và B là con của A.'");


        // Fallback to AI call if local rules couldn't determine both relationships
        if (pathToB.NodeIds.Any() || pathToA.NodeIds.Any())
        {
            // NEW: Check and increment AI chat usage quota
            var incrementUsageCommand = new IncrementFamilyAiChatUsageCommand { FamilyId = familyId };
            var incrementUsageResult = await _mediator.Send(incrementUsageCommand, cancellationToken);

            if (!incrementUsageResult.IsSuccess)
            {
                _logger.LogWarning("AI Chat usage increment failed for family {FamilyId}: {Error}", familyId, incrementUsageResult.Error);
                return new RelationshipDetectionResult
                {
                    Description = incrementUsageResult.Error ?? "Lỗi hạn mức AI.",
                    Path = new List<Guid>(),
                    Edges = new List<string>()
                };
            }

            const string RELATIONSHIP_AI_SYSTEM_PROMPT_CODE = "RELATIONSHIP_AI_SYSTEM_PROMPT";
            string systemPromptContent;

            var promptQuery = new GetPromptByIdQuery { Code = RELATIONSHIP_AI_SYSTEM_PROMPT_CODE };
            var promptResult = await _mediator.Send(promptQuery, cancellationToken);

            if (promptResult.IsSuccess && promptResult.Value != null)
            {
                systemPromptContent = promptResult.Value.Content;
                _logger.LogInformation("Successfully fetched system prompt '{PromptCode}' from database.", RELATIONSHIP_AI_SYSTEM_PROMPT_CODE);
            }
            else
            {
                _logger.LogError("Could not fetch system prompt '{PromptCode}' from database. Aborting AI generation. Error: {Error}", RELATIONSHIP_AI_SYSTEM_PROMPT_CODE, promptResult.Error);
                return new RelationshipDetectionResult
                {
                    Description = "Lỗi: Không thể tải cấu hình system prompt AI.",
                    Path = new List<Guid>(),
                    Edges = new List<string>()
                };
            }

            var aiRequest = new GenerateRequest
            {
                SystemPrompt = systemPromptContent,
                ChatInput = combinedPromptBuilder.ToString(),
                SessionId = Guid.NewGuid().ToString(),
                Metadata = new Dictionary<string, object>
                {
                    { "familyId", familyId },
                    { "memberAId", memberAId },
                    { "memberBId", memberBId }
                }
            };

            var aiResponseResult = await _aiGenerateService.GenerateDataAsync<RelationshipInferenceResultDto>(aiRequest, cancellationToken);

            if (aiResponseResult.IsSuccess && aiResponseResult.Value != null)
            {
                inferredDescription = aiResponseResult.Value.InferredRelationship;
            }
        }
        else
        {
            inferredDescription = "Không tìm thấy đường dẫn quan hệ.";
        }


        var edgesToString = pathToB.Edges.Select(e => e.Type.ToString()).ToList();

        return new RelationshipDetectionResult
        {
            Description = inferredDescription,
            Path = pathToB.NodeIds,
            Edges = edgesToString
        };
    }

    private string GetVietnameseAgeTerm(DateTime? dateOfBirth)
    {
        if (!dateOfBirth.HasValue)
        {
            return "";
        }

        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Value.Year;
        if (dateOfBirth.Value.Date > today.AddYears(-age))
        {
            age--;
        }

        return age > 0 ? $" {age} tuổi" : "";
    }

    // New helper method to get Vietnamese gender term
    private string GetVietnameseGenderTerm(string? genderString)
    {
        if (string.IsNullOrWhiteSpace(genderString))
        {
            return "";
        }

        if (Enum.TryParse<Gender>(genderString, true, out var gender))
        {
            return gender switch
            {
                Gender.Male => "(nam)",
                Gender.Female => "(nữ)",
                _ => ""
            };
        }
        return "";
    }

    // New helper method for concise path description
    private string _DescribePathInNaturalLanguageConcise(RelationshipPath path, IReadOnlyDictionary<Guid, Member> allMembers)
    {
        if (!path.NodeIds.Any() || path.NodeIds.Count != path.Edges.Count + 1)
        {
            return "Đường dẫn không hợp lệ hoặc không có thành viên nào.";
        }

        var descriptionBuilder = new StringBuilder();
        // A -> ... -> B. Mô tả A là gì của người kế tiếp, v.v.
        for (int i = 0; i < path.Edges.Count; i++)
        {
            var sourceMember = allMembers.GetValueOrDefault(path.Edges[i].SourceMemberId);
            var targetMember = allMembers.GetValueOrDefault(path.Edges[i].TargetMemberId);

            string sourceName = $"{sourceMember?.FullName}{GetVietnameseAgeTerm(sourceMember?.DateOfBirth)} {GetVietnameseGenderTerm(sourceMember?.Gender)}".Trim();
            string targetName = $"{targetMember?.FullName}{GetVietnameseAgeTerm(targetMember?.DateOfBirth)} {GetVietnameseGenderTerm(targetMember?.Gender)}".Trim();

            // Ensure we handle cases where sourceMember or targetMember might be null after trimming
            if (string.IsNullOrWhiteSpace(sourceName)) sourceName = $"Thành viên không rõ ({path.Edges[i].SourceMemberId})";
            if (string.IsNullOrWhiteSpace(targetName)) targetName = $"Thành viên không rõ ({path.Edges[i].TargetMemberId})";

            descriptionBuilder.Append($"{sourceName} là {GetVietnameseRelationshipTerm(path.Edges[i].Type)} của {targetName}");
            if (i < path.Edges.Count - 1)
            {
                descriptionBuilder.Append(", ");
            }
        }

        return descriptionBuilder.ToString() + ".";
    }


    private string GetVietnameseRelationshipTerm(RelationshipType type)
    {
        return type switch
        {
            RelationshipType.Father => "cha",
            RelationshipType.Mother => "mẹ",
            RelationshipType.Child => "con",
            RelationshipType.Husband => "chồng",
            RelationshipType.Wife => "vợ",
            _ => type.ToString()
        };
    }
}
