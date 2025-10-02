using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace backend.Application.Search;

public class SearchService : ISearchService
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly ILogger<SearchService> _logger;

    public SearchService(IFamilyRepository familyRepository, IMemberRepository memberRepository, ILogger<SearchService> logger)
    {
        _familyRepository = familyRepository;
        _memberRepository = memberRepository;
        _logger = logger;
    }

    public async Task<Result<PaginatedList<SearchItem>>> SearchAsync(string keyword, int page, int itemsPerPage)
    {
        const string source = "SearchService.SearchAsync";
        try
        {
            var results = new List<SearchItem>();
            var lowerCaseKeyword = keyword.ToLower();

            // Search in Families
            var families = await _familyRepository.GetAllAsync();
            var matchingFamilies = families.Where(f =>
                f.Name.ToLower().Contains(lowerCaseKeyword) ||
                (f.Description != null && f.Description.ToLower().Contains(lowerCaseKeyword)))
                .Select(f => new SearchItem
                {
                    Id = f.Id.ToString(),
                    Type = "Family",
                    Title = f.Name,
                    Description = f.Description,
                    Url = $"/family/{f.Id}" // Example URL
                });
            results.AddRange(matchingFamilies);

            // Search in Members
            var members = await _memberRepository.GetAllAsync();
            var matchingMembers = members.Where(m =>
                m.FullName.ToLower().Contains(lowerCaseKeyword) ||
                (m.Biography != null && m.Biography.ToLower().Contains(lowerCaseKeyword)))
                .Select(m => new SearchItem
                {
                    Id = m.Id.ToString(),
                    Type = "Member",
                    Title = m.FullName,
                    Description = m.Biography,
                    Url = $"/member/{m.Id}" // Example URL
                });
            results.AddRange(matchingMembers);

            var totalCount = results.Count;
            var paginatedItems = results.Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToList();

            return Result<PaginatedList<SearchItem>>.Success(new PaginatedList<SearchItem>(paginatedItems, totalCount, page, itemsPerPage));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source} for keyword {Keyword}, page {Page}, itemsPerPage {ItemsPerPage}", source, keyword, page, itemsPerPage);
            return Result<PaginatedList<SearchItem>>.Failure(ex.Message, source: source);
        }
    }
}