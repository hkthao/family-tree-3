using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Services;
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace backend.Application.Members;

public class MemberService : BaseCrudService<Member, IMemberRepository, MemberDto>, IMemberService
{
    private readonly IFamilyRepository _familyRepository;

    public MemberService(IMemberRepository memberRepository, IFamilyRepository familyRepository, ILogger<MemberService> logger, IMapper _mapper)
        : base(memberRepository, logger, _mapper)
    {
        _familyRepository = familyRepository;
    }

    public async Task<Result<List<MemberDto>>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        const string source = "MemberService.GetByIdsAsync";
        try
        {
            var members = await _repository.GetByIdsAsync(ids);
            return Result<List<MemberDto>>.Success(_mapper.Map<List<MemberDto>>(members));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source} for IDs {Ids}", source, string.Join(",", ids));
            return Result<List<MemberDto>>.Failure(ex.Message, source: source);
        }
    }

    public async Task<Result<PaginatedList<MemberDto>>> SearchAsync(MemberFilterModel filter)
    {
        const string source = "MemberService.SearchAsync";
        try
        {
            var members = await _repository.GetAllAsync();
            var query = members.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
            {
                query = query.Where(m => m.FullName.Contains(filter.SearchQuery) || (m.Biography != null && m.Biography.Contains(filter.SearchQuery)));
            }

            if (!string.IsNullOrWhiteSpace(filter.Gender))
            {
                query = query.Where(m => m.Gender == filter.Gender);
            }


            if (filter.FamilyId.HasValue)
            {
                query = query.Where(m => m.FamilyId == filter.FamilyId.Value);
            }

            if (filter.Ids != null && filter.Ids.Any())
            {
                query = query.Where(m => filter.Ids.Contains(m.Id));
            }

            var totalCount = query.Count();
            var items = query.Skip((filter.Page - 1) * filter.ItemsPerPage).Take(filter.ItemsPerPage).ProjectTo<MemberDto>(_mapper.ConfigurationProvider).ToList();
            return Result<PaginatedList<MemberDto>>.Success(new PaginatedList<MemberDto>(items, totalCount, filter.Page, filter.ItemsPerPage));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source} for filter {@Filter}", source, filter);
            return Result<PaginatedList<MemberDto>>.Failure(ex.Message, source: source);
        }
    }

    public async Task<Result<MemberDto>> GetMemberDtoByIdAsync(Guid id)
    {
        const string source = "MemberService.GetMemberDtoByIdAsync";
        try
        {
            var memberResult = await base.GetByIdAsync(id);
            if (memberResult.IsSuccess && memberResult.Value != null)
            {
                return Result<MemberDto>.Success(memberResult.Value);
            }
            return Result<MemberDto>.Failure(memberResult.Error!, source: memberResult.Source);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source} for ID {Id}", source, id);
            return Result<MemberDto>.Failure(ex.Message, source: source);
        }
    }

    public override async Task<Result<MemberDto>> CreateAsync(Member member)
    {
        var result = await base.CreateAsync(member);
        if (result.IsSuccess)
        {
            await UpdateFamilyTotalMembers(member.FamilyId, 1); // Increment count
        }
        return result;
    }

    public override async Task<Result> UpdateAsync(Member member)
    {
        // No change in member count for update, but ensure familyId is not changed
        var existingMemberResult = await base.GetByIdAsync(member.Id);
        if (!existingMemberResult.IsSuccess) return existingMemberResult.ToResult();

        if (existingMemberResult.Value!.FamilyId != member.FamilyId)
        {
            // If familyId changes, update counts for both old and new families
            await UpdateFamilyTotalMembers(existingMemberResult.Value.FamilyId, -1);
            await UpdateFamilyTotalMembers(member.FamilyId, 1);
        }
        return await base.UpdateAsync(member);
    }

    public override async Task<Result> DeleteAsync(Guid id)
    {
        var memberResult = await base.GetByIdAsync(id);
        if (!memberResult.IsSuccess) return memberResult.ToResult();

        var result = await base.DeleteAsync(id);
        if (result.IsSuccess)
        {
            await UpdateFamilyTotalMembers(memberResult.Value!.FamilyId, -1); // Decrement count
        }
        return result;
    }

    private async Task UpdateFamilyTotalMembers(Guid familyId, int change)
    {
        const string source = "MemberService.UpdateFamilyTotalMembers";
        var familyResult = await _familyRepository.GetByIdAsync(familyId);
        if (familyResult != null)
        {
            familyResult.TotalMembers += change;
            await _familyRepository.UpdateAsync(familyResult);
        }
        else
        {
            _logger.LogWarning("Family with ID {FamilyId} not found in {Source} when updating TotalMembers.", familyId, source);
        }
    }
}