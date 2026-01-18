using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Knowledge.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Application.Knowledge.Queries.SearchKnowledgeBase;

public class SearchKnowledgeBaseQueryHandler : IRequestHandler<SearchKnowledgeBaseQuery, Result<List<KnowledgeSearchResultDto>>>
{
    private readonly IKnowledgeService _knowledgeService;
    private readonly ILogger<SearchKnowledgeBaseQueryHandler> _logger;

    public SearchKnowledgeBaseQueryHandler(IKnowledgeService knowledgeService, ILogger<SearchKnowledgeBaseQueryHandler> logger)
    {
        _knowledgeService = knowledgeService;
        _logger = logger;
    }

    public async Task<Result<List<KnowledgeSearchResultDto>>> Handle(SearchKnowledgeBaseQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching knowledge base for FamilyId: {FamilyId}, Query: {QueryString}", request.FamilyId, request.QueryString);

        var results = await _knowledgeService.SearchKnowledgeBase(
            request.FamilyId,
            request.QueryString,
            request.TopK,
            request.AllowedVisibility
        );

        return Result<List<KnowledgeSearchResultDto>>.Success(results);
    }
}
