using backend.Application.Common.Models;

namespace backend.Application.Families.Commands.GenerateFamilyTreeGraph;

/// <summary>
/// Command để tạo Graphviz .dot file cho cây gia phả.
/// </summary>
public record GenerateFamilyTreeGraphCommand : IRequest<Result<GraphGenerationJobDto>>
{
    /// <summary>
    /// ID của gia đình.
    /// </summary>
    public Guid FamilyId { get; init; }

    /// <summary>
    /// ID của thành viên gốc để bắt đầu xây dựng cây.
    /// </summary>
    public Guid RootMemberId { get; init; }
}
