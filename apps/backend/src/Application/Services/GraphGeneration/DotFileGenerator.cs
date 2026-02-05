using System.Text;
using backend.Domain.Entities;

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
    /// <param name="couples">Danh sách các cặp vợ chồng đã xác định trong cây con.</param>
    /// <param name="allMembers">Tất cả các thành viên trong gia đình (để xác định giới tính).</param>
    /// <returns>Chuỗi nội dung của tệp .dot.</returns>
    public string GenerateDotFileContent(Dictionary<Guid, FamilyTreeNode> nodes, List<(Guid ParentId, Guid ChildId)> edges, IReadOnlyCollection<Tuple<Guid, Guid>> couples, IReadOnlyCollection<Member> allMembers)
    {
        var dotContent = new StringBuilder();

        AppendGraphStyles(dotContent);

        // Dictionary to store generated node IDs for couples and individuals
        var generatedNodeIds = new Dictionary<Guid, string>();
        var coupleNodeMap = new Dictionary<Tuple<Guid, Guid>, string>(); // Maps original couple (sorted IDs) to generated couple node ID

        // Determine min and max generation for coloring and relative generation logic
        int minGeneration = nodes.Values.Any() ? nodes.Values.Min(n => n.Generation) : 0;

        // Group all members by generation
        var membersByGeneration = allMembers.Where(m => nodes.ContainsKey(m.Id)) // Only consider members present in the current graph nodes
                                             .GroupBy(m => nodes[m.Id].Generation)
                                             .OrderBy(g => g.Key)
                                             .ToDictionary(g => g.Key, g => g.ToList());

        // Define a mapping for generation to fill color based on the example and typical family tree structures
        var generationColorMap = new Dictionary<int, string>
        {
            { minGeneration, "#E3F2FD" },       // Often for the oldest generation (grandparents)
            { minGeneration + 1, "#E8F5E9" },   // Next generation (parents)
            { minGeneration + 2, "#FFFDE7" }    // Children
            // Add more colors/logic for deeper trees as needed, or a default
        };

        var processedMembers = new HashSet<Guid>(); // To track members already part of a couple node

        dotContent.AppendLine("\n    /* ===== GENERATIONAL NODES (COUPLES) ===== */");
        ProcessCoupleNodes(dotContent, couples, allMembers, nodes, generatedNodeIds, coupleNodeMap, processedMembers, membersByGeneration, generationColorMap);

        dotContent.AppendLine("\n    /* ===== INDIVIDUAL MEMBERS ===== */");
        ProcessIndividualNodes(dotContent, nodes, generatedNodeIds, processedMembers, generationColorMap);

        dotContent.AppendLine("\n    /* ===== CONNECTIONS ===== */");
        var childToParentsMap = GenerateChildToParentMap(edges);

        var siblingGroups = new Dictionary<string, List<string>>(); // Key: ParentNodeId, Value: List of ChildNodeIds (for rank=same)
        ProcessEdgesAndSiblingGroups(dotContent, childToParentsMap, nodes, generatedNodeIds, coupleNodeMap, allMembers, siblingGroups);

        dotContent.AppendLine("\n    /* ===== SAME RANK FOR SIBLINGS ===== */");
        AppendSiblingRankings(dotContent, siblingGroups);

        dotContent.AppendLine("}");

        return dotContent.ToString();
    }

    /// <summary>
    /// Appends standard Graphviz styles for the family tree graph, nodes, and edges.
    /// </summary>
    /// <param name="dotContent">The StringBuilder to append the styles to.</param>
    private void AppendGraphStyles(StringBuilder dotContent)
    {
        dotContent.AppendLine("digraph FamilyTree {");
        dotContent.AppendLine("    rankdir=TB; // Changed to Top-to-Bottom for generational view");
        dotContent.AppendLine("    nodesep=1.3;");
        dotContent.AppendLine("    ranksep=2.0;");
        dotContent.AppendLine("");
        dotContent.AppendLine("    graph [");
        dotContent.AppendLine("        bgcolor=\"white\"");
        dotContent.AppendLine("        splines=true");
        dotContent.AppendLine("    ];");
        dotContent.AppendLine("");
        dotContent.AppendLine("    node [");
        dotContent.AppendLine("        shape=box");
        dotContent.AppendLine("        style=\"rounded,filled\"");
        dotContent.AppendLine("        fontname=\"Helvetica\"");
        dotContent.AppendLine("        fontsize=12");
        dotContent.AppendLine("    ];");
        dotContent.AppendLine("");
        dotContent.AppendLine("    edge [");
        dotContent.AppendLine("        color=\"#607D8B\"");
        dotContent.AppendLine("        penwidth=1.4");
        dotContent.AppendLine("        arrowhead=vee");
        dotContent.AppendLine("        arrowsize=0.7");
        dotContent.AppendLine("    ];");
        dotContent.AppendLine("");
    }

    /// <summary>
    /// Generates a formatted string for a member's birth and death years.
    /// </summary>
    /// <param name="birthYear">The member's birth year.</param>
    /// <param name="deathYear">The member's death year.</param>
    /// <returns>A string like " (YYYY - YYYY)" or " (YYYY)" if only birth year is present, otherwise an empty string.</returns>
    private string GetMemberYearInfo(int? birthYear, int? deathYear)
    {
        var yearInfoParts = new List<string>();
        if (birthYear.HasValue) yearInfoParts.Add(birthYear.Value.ToString());
        if (deathYear.HasValue) yearInfoParts.Add(deathYear.Value.ToString());

        if (yearInfoParts.Any())
        {
            return $" ({string.Join(" - ", yearInfoParts)})";
        }
        return "";
    }

    /// <summary>
    /// Processes couples and generates their corresponding nodes in the Dot file content.
    /// </summary>
    private void ProcessCoupleNodes(
        StringBuilder dotContent,
        IReadOnlyCollection<Tuple<Guid, Guid>> couples,
        IReadOnlyCollection<Member> allMembers,
        Dictionary<Guid, FamilyTreeNode> nodes,
        Dictionary<Guid, string> generatedNodeIds,
        Dictionary<Tuple<Guid, Guid>, string> coupleNodeMap,
        HashSet<Guid> processedMembers,
        Dictionary<int, List<Member>> membersByGeneration,
        Dictionary<int, string> generationColorMap)
    {
        // Process couples first to create couple nodes for each relevant generation
        foreach (var genGroup in membersByGeneration)
        {
            var currentGeneration = genGroup.Key;
            var membersInThisGeneration = genGroup.Value.ToHashSet(); // For efficient lookup

            string generationFillColor = generationColorMap.ContainsKey(currentGeneration) 
                                        ? generationColorMap[currentGeneration] 
                                        : "#f5f5f5"; // Default light grey for other generations

            // Sort couples to ensure consistent processing order and node ID generation
            foreach (var couple in couples.OrderBy(c => c.Item1).ThenBy(c => c.Item2))
            {
                var spouse1Id = couple.Item1;
                var spouse2Id = couple.Item2;

                Member? spouse1 = allMembers.FirstOrDefault(m => m.Id == spouse1Id);
                Member? spouse2 = allMembers.FirstOrDefault(m => m.Id == spouse2Id);

                // Ensure both spouses exist and are part of the current generational group and not already processed
                if (spouse1 == null || spouse2 == null || !membersInThisGeneration.Contains(spouse1) || !membersInThisGeneration.Contains(spouse2) ||
                    processedMembers.Contains(spouse1.Id) || processedMembers.Contains(spouse2.Id))
                {
                    continue; 
                }

                // At this point, spouse1 and spouse2 are guaranteed to be non-null.
                // Let's create non-nullable local variables for clarity and to satisfy the compiler.
                Member nonNullableSpouse1 = spouse1;
                Member nonNullableSpouse2 = spouse2;

                // Ensure consistent order for husband and wife for ID generation and labeling
                Member husband;
                Member wife;

                if (nonNullableSpouse1.Gender == "Male" && nonNullableSpouse2.Gender == "Female")
                {
                    husband = nonNullableSpouse1;
                    wife = nonNullableSpouse2;
                }
                else if (nonNullableSpouse1.Gender == "Female" && nonNullableSpouse2.Gender == "Male")
                {
                    husband = nonNullableSpouse2;
                    wife = nonNullableSpouse1;
                }
                else
                {
                    // Fallback for unexpected gender combinations or same-sex couples,
                    // prioritize by ID for consistent ordering in the graph.
                    if (nonNullableSpouse1.Id.CompareTo(nonNullableSpouse2.Id) < 0)
                    {
                        husband = nonNullableSpouse1;
                        wife = nonNullableSpouse2;
                    }
                    else
                    {
                        husband = nonNullableSpouse2;
                        wife = nonNullableSpouse1;
                    }
                }

                var coupleNodeId = $"couple_{currentGeneration}_{husband.Id.ToString().Replace("-", "")[..8]}_{wife.Id.ToString().Replace("-", "")[..8]}"; // Shorter unique ID
                
                string husbandYearInfo = GetMemberYearInfo(husband.DateOfBirth?.Year, husband.DateOfDeath?.Year);
                string wifeYearInfo = GetMemberYearInfo(wife.DateOfBirth?.Year, wife.DateOfDeath?.Year);

                var husbandLabel = $"{husband.FullName}{husbandYearInfo}";
                var wifeLabel = $"{wife.FullName}{wifeYearInfo}";

                var coupleLabel = $"{husbandLabel}\\n{wifeLabel}"; // Use \\n for newline in dot label

                // Store mapping for children connection
                coupleNodeMap.Add(couple, coupleNodeId);
                generatedNodeIds.Add(husband.Id, coupleNodeId); // Map husband and wife to their couple node ID
                generatedNodeIds.Add(wife.Id, coupleNodeId);

                dotContent.AppendLine($"    \"{coupleNodeId}\" [label=\"{coupleLabel}\", fillcolor=\"{generationFillColor}\"];");
                
                processedMembers.Add(husband.Id);
                processedMembers.Add(wife.Id);
            }
        }
    }

    /// <summary>
    /// Processes individual member nodes (those not part of a couple node) and adds them to the Dot file content.
    /// </summary>
    private void ProcessIndividualNodes(
        StringBuilder dotContent,
        Dictionary<Guid, FamilyTreeNode> nodes,
        Dictionary<Guid, string> generatedNodeIds,
        HashSet<Guid> processedMembers,
        Dictionary<int, string> generationColorMap)
    {
        // Process individual member nodes (those not part of a couple node)
        foreach (var node in nodes.Values.OrderBy(n => n.Generation).ThenBy(n => n.FullName))
        {
            if (processedMembers.Contains(node.Id)) continue; // Skip if already part of a couple node

            string individualFillColor = generationColorMap.ContainsKey(node.Generation) 
                                        ? generationColorMap[node.Generation] 
                                        : "#FFFDE7"; // Default for individual children (yellow from example) or other generations

            var yearString = GetMemberYearInfo(node.BirthYear, node.DeathYear);
            var label = $"{node.FullName}{yearString}";

            var individualNodeId = $"member_{node.Generation}_{node.Id.ToString().Replace("-", "")[..8]}"; // Shorter unique ID
            generatedNodeIds.Add(node.Id, individualNodeId);

            dotContent.AppendLine($"    \"{individualNodeId}\" [label=\"{label}\", fillcolor=\"{individualFillColor}\"];");
            processedMembers.Add(node.Id); // Mark as processed
        }
    }

    /// <summary>
    /// Generates a map of child IDs to a list of their parent IDs.
    /// </summary>
    /// <param name="edges">The list of parent-child edges.</param>
    /// <returns>A dictionary mapping child IDs to a list of parent IDs.</returns>
    private Dictionary<Guid, List<Guid>> GenerateChildToParentMap(List<(Guid ParentId, Guid ChildId)> edges)
    {
        var childToParentsMap = new Dictionary<Guid, List<Guid>>();
        foreach (var edge in edges)
        {
            if (!childToParentsMap.ContainsKey(edge.ChildId))
            {
                childToParentsMap[edge.ChildId] = new List<Guid>();
            }
            childToParentsMap[edge.ChildId].Add(edge.ParentId);
        }
        return childToParentsMap;
    }

    /// <summary>
    /// Processes child-to-parent relationships, adds edges to the Dot file content, and groups siblings.
    /// </summary>
    private void ProcessEdgesAndSiblingGroups(
        StringBuilder dotContent,
        Dictionary<Guid, List<Guid>> childToParentsMap,
        Dictionary<Guid, FamilyTreeNode> nodes,
        Dictionary<Guid, string> generatedNodeIds,
        Dictionary<Tuple<Guid, Guid>, string> coupleNodeMap,
        IReadOnlyCollection<Member> allMembers,
        Dictionary<string, List<string>> siblingGroups)
    {
        // Iterate through children to create edges
        foreach (var childEntry in childToParentsMap)
        {
            var childId = childEntry.Key;
            var parentsOriginalIds = childEntry.Value.Where(pId => nodes.ContainsKey(pId)).ToList();

            // Find the generated node ID for the child
            string? childNodeId = generatedNodeIds.ContainsKey(childId) ? generatedNodeIds[childId] : null;

            if (childNodeId == null) continue; // Skip if child node was not generated (e.g., filtered out)

            // Determine the parent node ID to connect from
            string? parentNodeId = DetermineParentNodeId(childId, parentsOriginalIds, generatedNodeIds, coupleNodeMap, allMembers);

            // Draw the edge if both parent and child nodes are valid
            if (parentNodeId != null)
            {
                dotContent.AppendLine($"    \"{parentNodeId}\" -> \"{childNodeId}\";");

                // Group children by their parent node for rank=same processing
                if (!siblingGroups.ContainsKey(parentNodeId))
                {
                    siblingGroups[parentNodeId] = new List<string>();
                }
                siblingGroups[parentNodeId].Add(childNodeId);
            }
        }
    }

    /// <summary>
    /// Determines the appropriate parent node ID for a given child, prioritizing couple nodes.
    /// </summary>
    /// <param name="childId">The ID of the child member.</param>
    /// <param name="parentsOriginalIds">A list of original parent IDs for the child.</param>
    /// <param name="generatedNodeIds">A dictionary mapping original member IDs to their generated graph node IDs.</param>
    /// <param name="coupleNodeMap">A dictionary mapping couple tuples to their generated couple node IDs.</param>
    /// <param name="allMembers">A collection of all members for gender information lookup.</param>
    /// <returns>The generated node ID of the parent (couple or individual), or null if no parent node is found.</returns>
    private string? DetermineParentNodeId(
        Guid childId,
        List<Guid> parentsOriginalIds,
        Dictionary<Guid, string> generatedNodeIds,
        Dictionary<Tuple<Guid, Guid>, string> coupleNodeMap,
        IReadOnlyCollection<Member> allMembers)
    {
        string? parentNodeId = null;
        
        // Try to find a couple parent first
        if (parentsOriginalIds.Count >= 2)
        {
            Guid? actualFatherId = null;
            Guid? actualMotherId = null;

            foreach (var pId in parentsOriginalIds)
            {
                var parentMember = allMembers.FirstOrDefault(m => m.Id == pId);
                if (parentMember != null)
                {
                    if (parentMember.Gender == "Male") actualFatherId = pId;
                    else if (parentMember.Gender == "Female") actualMotherId = pId;
                }
            }

            if (actualFatherId.HasValue && actualMotherId.HasValue)
            {
                var coupleKey = actualFatherId.Value.CompareTo(actualMotherId.Value) < 0
                    ? Tuple.Create(actualFatherId.Value, actualMotherId.Value)
                    : Tuple.Create(actualMotherId.Value, actualFatherId.Value);

                if (coupleNodeMap.TryGetValue(coupleKey, out var coupleNodeGeneratedId))
                {
                    parentNodeId = coupleNodeGeneratedId;
                }
            }
        }

        // If not connected to a couple node, try connecting to a single parent
        if (parentNodeId == null && parentsOriginalIds.Any())
        {
            var singleParentId = parentsOriginalIds.First(); // Connect to the first parent found if not a couple
            if (generatedNodeIds.ContainsKey(singleParentId))
            {
                parentNodeId = generatedNodeIds[singleParentId];
            }
        }
        return parentNodeId;
    }

    /// <summary>
    /// Appends commands to the Dot file content to ensure siblings are ranked on the same level.
    /// </summary>
    /// <param name="dotContent">The StringBuilder to append the commands to.</param>
    /// <param name="siblingGroups">A dictionary where the key is the parent node ID and the value is a list of child node IDs.</param>
    private void AppendSiblingRankings(StringBuilder dotContent, Dictionary<string, List<string>> siblingGroups)
    {
        foreach (var parentNodeId in siblingGroups.Keys)
        {
            var childrenNodes = siblingGroups[parentNodeId].Distinct().ToList(); // Ensure unique children
            if (childrenNodes.Count > 1)
            {
                dotContent.AppendLine($"    {{ rank=same; {string.Join("; ", childrenNodes.Select(id => $"\"{id}\""))} }}");
            }
        }
    }
}
