using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

public interface ISearchService
{
    Task<Result<SearchResult>> SearchAsync(string keyword);
}