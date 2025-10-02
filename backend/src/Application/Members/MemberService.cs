using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Services;
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace backend.Application.Members;

public class MemberService : BaseCrudService<Member, IMemberRepository>, IMemberService
{
    private readonly IFamilyRepository _familyRepository;

    public MemberService(IMemberRepository memberRepository, IFamilyRepository familyRepository, ILogger<MemberService> logger)
        : base(memberRepository, logger)
    {
        _familyRepository = familyRepository;
    }

    public async Task<Result<List<Member>>> GetMembersByIdsAsync(IEnumerable<Guid> ids)
    {
        const string source = "MemberService.GetMembersByIdsAsync";
        try
        {
            var members = await _repository.GetByIdsAsync(ids);
            return Result<List<Member>>.Success(members.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Source} for IDs {Ids}", source, string.Join(",", ids));
            return Result<List<Member>>.Failure(ex.Message, source: source);
        }
    }

    public override async Task<Result<Member>> CreateAsync(Member member)
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