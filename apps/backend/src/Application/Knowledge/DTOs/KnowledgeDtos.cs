using System.Collections.Generic;
using backend.Application.Common.Models; // For Result
using backend.Application.Knowledge.DTOs; // For KnowledgeSearchResultDto


namespace backend.Application.Knowledge.DTOs;

public class GenericKnowledgeDto
{
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    // Summary is a condensed text representation of the knowledge.
    public string Summary { get; set; } = string.Empty;
}

public class KnowledgeIndexRequest
{
    public GenericKnowledgeDto Data { get; set; } = new GenericKnowledgeDto();
    public string Action { get; set; } = "index"; // "index" or "delete"
}

public class KnowledgeSearchResultDto
{
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    public string Summary { get; set; } = string.Empty; // Corresponds to summary
    public double Score { get; set; } // Relevance score
}
