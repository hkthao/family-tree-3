using backend.Application.Common.Interfaces;
using backend.Domain.Interfaces;
using System.Text;
using backend.Application.AI.DTOs;
using backend.Application.AI;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Services;

public class RelationshipDetectionService : IRelationshipDetectionService
{
    private readonly IApplicationDbContext _context;
    private readonly IRelationshipGraph _relationshipGraph;
    private readonly IAiGenerateService _aiGenerateService;

    public RelationshipDetectionService(IApplicationDbContext context, IRelationshipGraph relationshipGraph, IAiGenerateService aiGenerateService)
    {
        _context = context;
        _relationshipGraph = relationshipGraph;
        _aiGenerateService = aiGenerateService;
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

        string fromAToB = "unknown";
        string fromBToA = "unknown";

        var combinedPromptBuilder = new StringBuilder();

        // Prompt for A to B
        if (pathToB.NodeIds.Any())
        {
            var memberA = allMembers.GetValueOrDefault(memberAId);
            var memberB = allMembers.GetValueOrDefault(memberBId);

            if (memberA != null && memberB != null)
            {
                combinedPromptBuilder.AppendLine($"Dựa trên cây gia phả, Thành viên A (ID: {memberAId}, Tên: {memberA.FullName}) được kết nối với Thành viên B (ID: {memberBId}, Tên: {memberB.FullName}) thông qua đường dẫn quan hệ sau:");

                for (int i = 0; i < pathToB.NodeIds.Count; i++)
                {
                    var nodeId = pathToB.NodeIds[i];
                    var member = allMembers.GetValueOrDefault(nodeId);
                    combinedPromptBuilder.Append(member?.FullName ?? $"Thành viên không rõ ({{nodeId}})");

                    if (i < pathToB.Edges.Count)
                    {
                        combinedPromptBuilder.Append($" --({{pathToB.Edges[i].Type}})--> ");
                    }
                }
                combinedPromptBuilder.AppendLine();
                combinedPromptBuilder.AppendLine("Suy luận mối quan hệ trực tiếp trong gia đình từ Thành viên A đến Thành viên B.");
            }
        }
        else
        {
            combinedPromptBuilder.AppendLine($"Không tìm thấy đường dẫn từ Thành viên A (ID: {memberAId}) đến Thành viên B (ID: {memberBId}).");
        }

        combinedPromptBuilder.AppendLine("\n---"); // Separator for clarity in AI prompt

        // Prompt for B to A
        if (pathToA.NodeIds.Any())
        {
            var memberA = allMembers.GetValueOrDefault(memberAId);
            var memberB = allMembers.GetValueOrDefault(memberBId);

            if (memberA != null && memberB != null)
            {
                combinedPromptBuilder.AppendLine($"Dựa trên cây gia phả, Thành viên B (ID: {memberBId}, Tên: {memberB.FullName}) được kết nối với Thành viên A (ID: {memberAId}, Tên: {memberA.FullName}) thông qua đường dẫn quan hệ sau:");
                
                for (int i = 0; i < pathToA.NodeIds.Count; i++)
                {
                    var nodeId = pathToA.NodeIds[i];
                    var member = allMembers.GetValueOrDefault(nodeId);
                    combinedPromptBuilder.Append(member?.FullName ?? $"Thành viên không rõ ({{nodeId}})");

                    if (i < pathToA.Edges.Count)
                    {
                        combinedPromptBuilder.Append($" --({{pathToA.Edges[i].Type}})--> ");
                    }
                }
                combinedPromptBuilder.AppendLine();
                combinedPromptBuilder.AppendLine("Suy luận mối quan hệ trực tiếp trong gia đình từ Thành viên B đến Thành viên A.");
            }
        }
        else
        {
            combinedPromptBuilder.AppendLine($"Không tìm thấy đường dẫn từ Thành viên B (ID: {memberBId}) đến Thành viên A (ID: {memberAId}).");
        }

        // Single AI call
        if (pathToB.NodeIds.Any() || pathToA.NodeIds.Any())
        {
            var aiRequest = new GenerateRequest
            {
                SystemPrompt = "Bạn là một chuyên gia về các mối quan hệ gia đình Việt Nam. Phân tích các đường dẫn cây gia phả được cung cấp và suy luận mối quan hệ trực tiếp trong gia đình giữa các thành viên. Kết quả phải là một đối tượng JSON có hai trường: 'FromAToB' và 'FromBToA', mỗi trường chứa một chuỗi mối quan hệ suy luận. Sử dụng các thuật ngữ mối quan hệ tiếng Việt như 'cha', 'mẹ', 'con', 'anh', 'chị', 'em', 'chú', 'bác', 'cô', 'dì', 'cháu', 'ông', 'bà', 'chắt', 'chắt trai', 'chắt gái', 'vợ', 'chồng'. Nếu mối quan hệ không thể xác định được hoặc quá phức tạp, hãy trả về 'unknown' cho trường đó. Ví dụ: { \"FromAToB\": \"cha\", \"FromBToA\": \"con\" }",
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
                fromAToB = aiResponseResult.Value.FromAToB;
                fromBToA = aiResponseResult.Value.FromBToA;
            }
        }

        var edgesToString = pathToB.Edges.Select(e => e.Type.ToString()).ToList();

        return new RelationshipDetectionResult
        {
            FromAToB = fromAToB,
            FromBToA = fromBToA,
            Path = pathToB.NodeIds,
            Edges = edgesToString
        };
    }
}
