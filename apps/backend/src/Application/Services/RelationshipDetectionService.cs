using backend.Application.Common.Interfaces;
using backend.Domain.Interfaces;
using System.Text;
using backend.Application.AI.DTOs;
using backend.Application.AI;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq; // For String.Join

namespace backend.Application.Services;

public class RelationshipDetectionService : IRelationshipDetectionService
{
    private readonly IApplicationDbContext _context;
    private readonly IRelationshipGraph _relationshipGraph;
    private readonly IAiGenerateService _aiGenerateService;
    private readonly IRelationshipRuleEngine _ruleEngine; // Reintroduce IRelationshipRuleEngine

    public RelationshipDetectionService(IApplicationDbContext context, IRelationshipGraph relationshipGraph, IAiGenerateService aiGenerateService, IRelationshipRuleEngine ruleEngine) // Update constructor
    {
        _context = context;
        _relationshipGraph = relationshipGraph;
        _aiGenerateService = aiGenerateService;
        _ruleEngine = ruleEngine; // Initialize ruleEngine
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
        
        // Simpler, more direct prompt for AI
        combinedPromptBuilder.AppendLine($"Xác định mối quan hệ gia đình giữa Thành viên '{memberA.FullName}' (ID: {memberAId}) và Thành viên '{memberB.FullName}' (ID: {memberBId}) trong một cây gia phả.");
        combinedPromptBuilder.AppendLine("Đường dẫn từ A đến B:");
        if (pathToB.NodeIds.Any())
        {
            combinedPromptBuilder.AppendLine("  " + _DescribePathInNaturalLanguageConcise(pathToB, allMembers)); // Use concise description
        }
        else
        {
            combinedPromptBuilder.AppendLine($"  Không có đường dẫn trực tiếp từ Thành viên '{memberA.FullName}' đến Thành viên '{memberB.FullName}'.");
        }

        combinedPromptBuilder.AppendLine("\nĐường dẫn từ B đến A:");
        if (pathToA.NodeIds.Any())
        {
            combinedPromptBuilder.AppendLine("  " + _DescribePathInNaturalLanguageConcise(pathToA, allMembers)); // Use concise description
        }
        else
        {
            combinedPromptBuilder.AppendLine($"  Không có đường dẫn trực tiếp từ Thành viên '{memberB.FullName}' đến Thành viên '{memberA.FullName}'.");
        }
        combinedPromptBuilder.AppendLine("Dựa vào thông tin trên, hãy suy luận mối quan hệ trực tiếp trong gia đình giữa hai thành viên và mô tả một cách NGẮN GỌN, SÚC TÍCH bằng tiếng Việt. Tránh liệt kê lại chi tiết đường dẫn.");
        combinedPromptBuilder.AppendLine("Ví dụ: 'A là cha của B và B là con của A.'");


        // Fallback to AI call if local rules couldn't determine both relationships
        if (pathToB.NodeIds.Any() || pathToA.NodeIds.Any())
        {
            var aiRequest = new GenerateRequest
            {
                SystemPrompt = "Bạn là một chuyên gia về các mối quan hệ gia đình Việt Nam. Phân tích các đường dẫn cây gia phả được cung cấp. Hãy suy luận mối quan hệ trực tiếp trong gia đình giữa các thành viên, và cung cấp một mô tả NGẮN GỌN, SÚC TÍCH về mối quan hệ của A với B và B với A bằng ngôn ngữ tự nhiên. Tránh liệt kê lại chi tiết đường dẫn trừ khi cần thiết để làm rõ. Kết quả phải là một đối tượng JSON có một trường: 'InferredRelationship', chứa chuỗi mô tả này. Sử dụng các thuật ngữ mối quan hệ tiếng Việt như 'cha', 'mẹ', 'con', 'anh', 'chị', 'em', 'chú', 'bác', 'cô', 'dì', 'cháu', 'ông', 'bà', 'chắt', 'chắt trai', 'chắt gái', 'vợ', 'chồng'. Nếu mối quan hệ không thể xác định được hoặc quá phức tạp, hãy trả về 'unknown' trong chuỗi. Ví dụ JSON: { \"InferredRelationship\": \"A là cha của B và B là con của A.\" }",
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

            if (sourceMember != null && targetMember != null)
            {
                descriptionBuilder.Append($"{sourceMember.FullName} ({sourceMember.Id}) là {GetVietnameseRelationshipTerm(path.Edges[i].Type)} của {targetMember.FullName} ({targetMember.Id})");
                if (i < path.Edges.Count - 1)
                {
                    descriptionBuilder.Append(", ");
                }
            } else if (sourceMember != null) {
                descriptionBuilder.Append($"{sourceMember.FullName} ({sourceMember.Id}) là {GetVietnameseRelationshipTerm(path.Edges[i].Type)} của Thành viên không rõ ({path.Edges[i].TargetMemberId})");
                if (i < path.Edges.Count - 1)
                {
                    descriptionBuilder.Append(", ");
                }
            } else if (targetMember != null) {
                descriptionBuilder.Append($"Thành viên không rõ ({path.Edges[i].SourceMemberId}) là {GetVietnameseRelationshipTerm(path.Edges[i].Type)} của {targetMember.FullName} ({targetMember.Id})");
                if (i < path.Edges.Count - 1)
                {
                    descriptionBuilder.Append(", ");
                }
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