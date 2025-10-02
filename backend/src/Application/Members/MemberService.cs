using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Members;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;

    public MemberService(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<List<Member>> GetAllMembersAsync()
    {
        var members = await _memberRepository.GetAllAsync();
        return members.ToList();
    }

    public async Task<Member?> GetMemberByIdAsync(Guid id)
    {
        return await _memberRepository.GetByIdAsync(id);
    }

    public async Task<List<Member>> GetMembersByIdsAsync(IEnumerable<Guid> ids)
    {
        var members = await _memberRepository.GetByIdsAsync(ids);
        return members.ToList();
    }

    public async Task<Member> CreateMemberAsync(Member member)
    {
        return await _memberRepository.AddAsync(member);
    }

    public async Task UpdateMemberAsync(Member member)
    {
        await _memberRepository.UpdateAsync(member);
    }

    public async Task DeleteMemberAsync(Guid id)
    {
        var member = await _memberRepository.GetByIdAsync(id);
        if (member != null)
        {
            await _memberRepository.DeleteAsync(member);
        }
    }
}