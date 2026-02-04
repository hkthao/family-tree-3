using System.Text;

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
        dotContent.AppendLine("    rankdir=LR;");
        dotContent.AppendLine("    compound=true;");

        dotContent.AppendLine("");
        dotContent.AppendLine("    graph [");
        dotContent.AppendLine("        size=\"46.8,33.1!\"");
        dotContent.AppendLine("        pad=\"1,1\"");
        dotContent.AppendLine("        dpi=72");
        dotContent.AppendLine("        bgcolor=\"white\"");
        dotContent.AppendLine("    ];");
        dotContent.AppendLine("");
        dotContent.AppendLine("    nodesep=0.5;");
        dotContent.AppendLine("    ranksep=0.8;");
        dotContent.AppendLine("");
        dotContent.AppendLine("    node [");
        dotContent.AppendLine("        shape=box");
        dotContent.AppendLine("        style=\"rounded,filled\"");
        dotContent.AppendLine("        fillcolor=\"#f5f5f5\"");
        dotContent.AppendLine("        fontname=\"Helvetica\"");
        dotContent.AppendLine("        fontsize=10");
        dotContent.AppendLine("    ];");
        dotContent.AppendLine("");

        // Thêm các node vào Graph
        foreach (var node in nodes.Values.OrderBy(n => n.Generation).ThenBy(n => n.FullName)) // Sắp xếp để hiển thị đẹp hơn
        {
            // "8cf2e95a-e7a0-42d4-bf8a-16389d447119" [label="Tú Anh\nGen 0"];
            dotContent.AppendLine($"    \"{node.Id}\" [label=\"{node.FullName}\\nGen {node.Generation}\"];");
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
