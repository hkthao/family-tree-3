namespace backend.Application.Knowledge.DTOs;

/// <summary>
/// Represents a search result from the knowledge search service.
/// </summary>
public class KnowledgeSearchResultDto
{
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    public string Summary { get; set; } = string.Empty; // Corresponds to summary
    public double Score { get; set; } // Relevance score
}
