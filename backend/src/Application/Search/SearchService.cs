using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.Search;

public class SearchService : ISearchService
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IMemberRepository _memberRepository;

    public SearchService(IFamilyRepository familyRepository, IMemberRepository memberRepository)
    {
        _familyRepository = familyRepository;
        _memberRepository = memberRepository;
    }

    public async Task<SearchResult> SearchAsync(string keyword)
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

        return new SearchResult
        {
            Items = results,
            TotalCount = results.Count
        };
    }
}