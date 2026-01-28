using System.Text;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Interfaces.Services.LLMGateway; // NEW
using backend.Application.Common.Models.LLMGateway; // NEW
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
    private readonly ILLMGatewayService _llmGatewayService;

    private readonly IMediator _mediator;
    private readonly ILogger<RelationshipDetectionService> _logger; // Inject ILogger

    public RelationshipDetectionService(IApplicationDbContext context, IRelationshipGraph relationshipGraph, ILLMGatewayService llmGatewayService, IMediator mediator, ILogger<RelationshipDetectionService> logger)
    {
        _context = context;
        _relationshipGraph = relationshipGraph;
        _llmGatewayService = llmGatewayService;

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

        var pathToB = _relationshipGraph.FindShortestPath(memberAId, memberBId) ?? new RelationshipPath();
        var pathToA = _relationshipGraph.FindShortestPath(memberBId, memberAId) ?? new RelationshipPath();

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

            string systemPromptContent;

            var promptQuery = new GetPromptByIdQuery { Code = PromptConstants.RELATIONSHIP_AI_SYSTEM_PROMPT };
            var promptResult = await _mediator.Send(promptQuery, cancellationToken);

            if (promptResult.IsSuccess && promptResult.Value != null)
            {
                systemPromptContent = promptResult.Value.Content;
                _logger.LogInformation("Successfully fetched system prompt '{PromptCode}' from database.", PromptConstants.RELATIONSHIP_AI_SYSTEM_PROMPT);
            }
            else
            {
                _logger.LogError("Could not fetch system prompt '{PromptCode}' from database. Aborting AI generation. Error: {Error}", PromptConstants.RELATIONSHIP_AI_SYSTEM_PROMPT, promptResult.Error);
                return new RelationshipDetectionResult
                {
                    Description = "Lỗi: Không thể tải cấu hình system prompt AI.",
                    Path = new List<Guid>(),
                    Edges = new List<string>()
                };
            }

            var llmRequest = new LLMChatCompletionRequest
            {
                Model = "gpt-3.5-turbo", // TODO: Make configurable
                Messages = new List<LLMMessage>
                {
                    new LLMMessage { Role = "system", Content = systemPromptContent },
                    new LLMMessage { Role = "user", Content = combinedPromptBuilder.ToString() }
                },
                User = familyId.ToString(), // Use FamilyId as user identifier for LLM Gateway
                Metadata = new Dictionary<string, object>
                {
                    { "familyId", familyId.ToString() },
                    { "memberAId", memberAId.ToString() },
                    { "memberBId", memberBId.ToString() }
                }
            };

            var llmResponseResult = await _llmGatewayService.GetChatCompletionAsync(llmRequest, cancellationToken);

            if (llmResponseResult.IsSuccess && llmResponseResult.Value != null && llmResponseResult.Value.Choices.Any())
            {
                inferredDescription = llmResponseResult.Value.Choices.First().Message.Content;
            }
        }
        else
        {
            inferredDescription = "Không tìm thấy đường dẫn quan hệ.";
        }


        var edgesToString = pathToB?.Edges?.Select(e => e.Type.ToString()).ToList() ?? new List<string>();

        return new RelationshipDetectionResult
        {
            Description = inferredDescription,
            Path = pathToB?.NodeIds ?? new List<Guid>(),
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
