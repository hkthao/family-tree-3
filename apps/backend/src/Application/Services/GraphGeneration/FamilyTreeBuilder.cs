using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Services.GraphGeneration;

/// <summary>
/// Đại diện cho một node trong cây gia phả đã được xây dựng, bao gồm thông tin thế hệ.
/// </summary>
public record FamilyTreeNode(
    Guid Id,
    string FullName,
    int Generation,
    int? BirthYear,
    int? DeathYear
);

/// <summary>
/// Dịch vụ xây dựng cây gia phả con (sub-tree) từ một thành viên gốc.
/// </summary>
public class FamilyTreeBuilder
{
    /// <summary>
    /// Xây dựng cây gia phả con bắt đầu từ rootMemberId, chỉ duyệt các hậu duệ.
    /// Thế hệ của rootMemberId sẽ là 0.
    /// </summary>
    /// <param name="allMembers">Tất cả các thành viên trong gia đình.</param>
    /// <param name="allRelationships">Tất cả các mối quan hệ trong gia đình.</param>
    /// <param name="rootMemberId">ID của thành viên gốc để bắt đầu xây dựng cây.</param>
    /// <param name="maxDepth">Độ sâu tối đa của cây để ngăn chặn vòng lặp vô hạn.</param>
    /// <returns>Một từ điển chứa các node (thành viên) trong cây và danh sách các cặp (parent, child) đại diện cho các cạnh.</returns>
    public (Dictionary<Guid, FamilyTreeNode> Nodes, List<(Guid ParentId, Guid ChildId)> Edges, List<Tuple<Guid, Guid>> Couples) BuildSubTree(
        IReadOnlyCollection<Member> allMembers,
        IReadOnlyCollection<Relationship> allRelationships,
        Guid rootMemberId,
        int maxDepth = 10 // Giới hạn độ sâu mặc định để tránh vòng lặp vô hạn và vòng lặp tuần hoàn
    )
    {
        var nodes = new Dictionary<Guid, FamilyTreeNode>();
        var edges = new List<(Guid ParentId, Guid ChildId)>();
        var visited = new HashSet<Guid>(); // Theo dõi các thành viên đã ghé thăm để tránh vòng lặp và trùng lặp
        var queue = new Queue<(Guid MemberId, int CurrentDepth)>();
        var couples = new List<Tuple<Guid, Guid>>(); // Danh sách các cặp vợ chồng đã xác định trong cây con

        var rootMember = allMembers.FirstOrDefault(m => m.Id == rootMemberId);
        if (rootMember == null)
        {
            return (nodes, edges, couples);
        }

        // rootMember là thế hệ 0
        nodes.Add(rootMember.Id, new FamilyTreeNode(rootMember.Id, rootMember.FullName, 0, rootMember.DateOfBirth?.Year, rootMember.DateOfDeath?.Year));

        queue.Enqueue((rootMember.Id, 0));
        visited.Add(rootMember.Id);

        while (queue.Any())
        {
            var (currentMemberId, currentGeneration) = queue.Dequeue();

            if (currentGeneration >= maxDepth)
            {
                continue; // Dừng nếu đạt đến độ sâu tối đa
            }

            // --- Xử lý mối quan hệ vợ chồng (Spouse) ---
            var spouseRelationships = allRelationships
                .Where(r => (r.SourceMemberId == currentMemberId && (r.Type == RelationshipType.Husband || r.Type == RelationshipType.Wife)) ||
                            (r.TargetMemberId == currentMemberId && (r.Type == RelationshipType.Husband || r.Type == RelationshipType.Wife)))
                .ToList();

            foreach (var rel in spouseRelationships)
            {
                var spouseId = (rel.SourceMemberId == currentMemberId) ? rel.TargetMemberId : rel.SourceMemberId;
                var spouseMember = allMembers.FirstOrDefault(m => m.Id == spouseId);

                if (spouseMember != null && nodes.ContainsKey(currentMemberId)) // Ensure spouse is relevant to current path
                {
                    // Add spouse to nodes if not already there, with same generation as currentMember
                    if (!nodes.ContainsKey(spouseId))
                    {
                        nodes.Add(spouseId, new FamilyTreeNode(spouseMember.Id, spouseMember.FullName, currentGeneration, spouseMember.DateOfBirth?.Year, spouseMember.DateOfDeath?.Year));
                        // No need to add spouse to queue for traversal if we only traverse descendants.
                        // We add them to nodes for rendering and to identify couples.
                    }

                    // Add couple to list if not already present, ensuring consistent order
                    var key = currentMemberId.CompareTo(spouseId) < 0
                        ? Tuple.Create(currentMemberId, spouseId)
                        : Tuple.Create(spouseId, currentMemberId);

                    if (!couples.Contains(key))
                    {
                        couples.Add(key);
                    }
                }
            }

            // --- Tìm con của thành viên hiện tại ---
            // Các mối quan hệ có SourceMemberId là cha/mẹ và TargetMemberId là con.
            var childrenRelationships = allRelationships
                .Where(r => r.SourceMemberId == currentMemberId &&
                            (r.Type == RelationshipType.Father || r.Type == RelationshipType.Mother))
                .ToList();

            foreach (var rel in childrenRelationships)
            {
                var childId = rel.TargetMemberId;
                var childMember = allMembers.FirstOrDefault(m => m.Id == childId);

                if (childMember != null)
                {
                    // Nếu node con chưa được thêm vào danh sách hoặc cần cập nhật thế hệ
                    if (!nodes.ContainsKey(childId))
                    {
                        nodes.Add(childId, new FamilyTreeNode(childMember.Id, childMember.FullName, currentGeneration + 1, childMember.DateOfBirth?.Year, childMember.DateOfDeath?.Year));
                    }
                    // Thêm cạnh (cha/mẹ -> con)
                    // Note: Edges are now processed in DotFileGenerator to account for marriage nodes
                    // So, we collect original edges here but DotFileGenerator will adjust them.
                    edges.Add((currentMemberId, childId));

                    // Nếu chưa ghé thăm con này, thêm vào hàng đợi để tiếp tục duyệt
                    if (!visited.Contains(childId))
                    {
                        visited.Add(childId);
                        queue.Enqueue((childId, currentGeneration + 1));
                    }
                }
            }
        }

        return (nodes, edges, couples);
    }
}