using MediatR;
using backend.Application.Common.Models;
using backend.Application.Knowledge.DTOs;

namespace backend.Application.Knowledge.Queries.SearchKnowledgeBase
{
    public class SearchKnowledgeBaseQuery : IRequest<Result<List<KnowledgeSearchResultDto>>>
    {
        public Guid FamilyId { get; set; }
        public string QueryString { get; set; } = null!;
        public int TopK { get; set; } = 5; // Default to 5 results
        public List<string> AllowedVisibility { get; set; } = new() { "public", "private" }; // Default visibility
    }
}
