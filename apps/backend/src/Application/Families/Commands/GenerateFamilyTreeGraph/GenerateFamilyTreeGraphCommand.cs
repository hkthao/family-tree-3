using backend.Application.Common.Models;

namespace backend.Application.Families.Commands.GenerateFamilyTreeGraph;

/// <summary>
/// Command để tạo Graphviz .dot file cho cây gia phả.
/// </summary>
public record GenerateFamilyTreeGraphCommand : IRequest<Result<byte[]>>
{
    /// <summary>
    /// ID của gia đình.
    /// </summary>
    public Guid FamilyId { get; init; }

    /// <summary>
    /// ID của thành viên gốc để bắt đầu xây dựng cây.
    /// </summary>
    public Guid? RootMemberId { get; init; }

    /// <summary>
    /// Kích thước trang PDF (ví dụ: "A0", "Letter").
    /// </summary>
    public string PageSize { get; init; } = "A0";

    /// <summary>
    /// Hướng của đồ thị (ví dụ: "LR" cho Left to Right, "TB" cho Top to Bottom).
    /// </summary>
    public string Direction { get; init; } = "LR";
}
