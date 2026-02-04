using System.Text;
using backend.Application.Services.GraphGeneration;

namespace backend.Application.Services.GraphGeneration;

/// <summary>
/// Dịch vụ tạo nội dung tệp Graphviz .dot từ cấu trúc cây gia phả.
/// </summary>
public class DotFileGenerator
{
    /// <summary>
    /// Tạo nội dung tệp Graphviz .dot.
    /// </summary>
    /// <param name="nodes">Các node (thành viên) của cây gia phả.</param>
    /// <param name="edges">Các cạnh (mối quan hệ cha-con) của cây gia phả.</param>
    /// <returns>Chuỗi nội dung của tệp .dot.</returns>
    public string GenerateDotFileContent(Dictionary<Guid, FamilyTreeNode> nodes, List<(Guid ParentId, Guid ChildId)> edges)
    {
        var dotContent = new StringBuilder();

        dotContent.AppendLine("digraph FamilyTree {");
        dotContent.AppendLine("    rankdir=LR; // Hướng từ trái sang phải"); 
        dotContent.AppendLine("    nodesep=0.5; // Khoảng cách giữa các node"); 
        dotContent.AppendLine("    ranksep=0.75; // Khoảng cách giữa các rank"); 
        dotContent.AppendLine("    compound=true; // Cho phép các cụm (cluster)"); 
        dotContent.AppendLine("    graph [pad=\"0.5,0.5\", viewport=\"1122,793,75\", size=\"A0\"]; // A0 size: 1189mm x 841mm"); 

        // Định nghĩa kiểu cho các node
        dotContent.AppendLine("    node [shape=box, style=\"filled\", fillcolor=\"#f0f0f0\", fontname=\"Helvetica\"];");

        // Thêm các node vào Graph
        foreach (var node in nodes.Values.OrderBy(n => n.Generation).ThenBy(n => n.FullName)) // Sắp xếp để hiển thị đẹp hơn
        {
            // member_123 [label="member_123\nNguyen Van A\nGen 3"];
            dotContent.AppendLine($"    \"{node.Id}\" [label=\"{node.Id}\\n{node.FullName}\\nGen {node.Generation}\"];");
        }

        // Thêm các cạnh vào Graph
        foreach (var edge in edges)
        {
            // parent -> child;
            dotContent.AppendLine($"    \"{edge.ParentId}\" -> \"{edge.ChildId}\";");
        }

        dotContent.AppendLine("}");

        return dotContent.ToString();
    }
}
