using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Services;
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace backend.Application.Families;

public class FamilyService : BaseCrudService<Family, IFamilyRepository>, IFamilyService
{
    public FamilyService(IFamilyRepository familyRepository, ILogger<FamilyService> logger)
        : base(familyRepository, logger)
    {
    }

    public async Task<Result<List<Family>>> GetFamiliesByIdsAsync(IEnumerable<Guid> ids)
    {
        const string source = "FamilyService.GetFamiliesByIdsAsync";
        try
        {
            var families = await _repository.GetByIdsAsync(ids);
            return Result<List<Family>>.Success(families.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source} for IDs {Ids}", source, string.Join(",", ids));
            return Result<List<Family>>.Failure(ex.Message, source: source);
        }
    }

    public async Task<Result<PaginatedList<Family>>> SearchFamiliesAsync(string? searchQuery, Guid? familyId, int page, int itemsPerPage)
    {
        const string source = "FamilyService.SearchFamiliesAsync";
        try
        {
            var families = await _repository.GetAllAsync();
            var query = families.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(f => f.Name.Contains(searchQuery) || (f.Description != null && f.Description.Contains(searchQuery)));
            }

            if (familyId.HasValue)
            {
                query = query.Where(f => f.Id == familyId.Value);
            }

            var totalCount = query.Count();
            var items = query.Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToList();

            return Result<PaginatedList<Family>>.Success(new PaginatedList<Family>(items, totalCount, page, itemsPerPage));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source} for searchQuery {SearchQuery}, familyId {FamilyId}, page {Page}, itemsPerPage {ItemsPerPage}", source, searchQuery, familyId, page, itemsPerPage);
            return Result<PaginatedList<Family>>.Failure(ex.Message, source: source);
        }
    }
}