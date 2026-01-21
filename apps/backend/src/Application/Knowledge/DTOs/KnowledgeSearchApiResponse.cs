using System.Collections.Generic;

namespace backend.Application.Knowledge.DTOs;

public class KnowledgeSearchApiResponse
{
    public List<KnowledgeSearchResultDto>? Results { get; set; }
}
