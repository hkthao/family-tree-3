using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Services;
using backend.Application.Families.Queries;
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace backend.Application.Families;

public class FamilyService : BaseCrudService<Family, IFamilyRepository, FamilyDto>, IFamilyService
{
    public FamilyService(IFamilyRepository familyRepository, ILogger<FamilyService> logger)
        : base(familyRepository, logger, FamilyDto.FromEntity)
    {
    }

    public async Task<Result<List<FamilyDto>>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        const string source = "FamilyService.GetByIdsAsync";
        try
        {
            var families = await _repository.GetByIdsAsync(ids);
            return Result<List<FamilyDto>>.Success(families.Select(FamilyDto.FromEntity).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source} for IDs {Ids}", source, string.Join(",", ids));
            return Result<List<FamilyDto>>.Failure(ex.Message, source: source);
        }
    }

    public async Task<Result<PaginatedList<FamilyDto>>> SearchAsync(FamilyFilterModel filter)
    {
        const string source = "FamilyService.SearchAsync";
        try
        {
            var families = await _repository.GetAllAsync();
            var query = families.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
            {
                query = query.Where(f => f.Name.Contains(filter.SearchQuery) || (f.Description != null && f.Description.Contains(filter.SearchQuery)));
            }

            if (filter.FamilyId.HasValue)
            {
                query = query.Where(f => f.Id == filter.FamilyId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Visibility))
            {
                query = query.Where(f => f.Visibility == filter.Visibility);
            }

            var totalCount = query.Count();
            var items = query.Skip((filter.Page - 1) * filter.ItemsPerPage).Take(filter.ItemsPerPage).ToList();

            return Result<PaginatedList<FamilyDto>>.Success(new PaginatedList<FamilyDto>(items.Select(FamilyDto.FromEntity).ToList(), totalCount, filter.Page, filter.ItemsPerPage));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source} for filter {@Filter}", source, filter);
            return Result<PaginatedList<FamilyDto>>.Failure(ex.Message, source: source);
        }
    }
}