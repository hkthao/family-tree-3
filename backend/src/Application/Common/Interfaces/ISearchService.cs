using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

public interface ISearchService
{
    Task<Result<PaginatedList<SearchItem>>> SearchAsync(string keyword, int page, int itemsPerPage);
}