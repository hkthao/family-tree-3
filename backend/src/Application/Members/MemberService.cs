using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Services;
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace backend.Application.Members;

public class MemberService : BaseCrudService<Member, IMemberRepository>, IMemberService
{
    public MemberService(IMemberRepository memberRepository, ILogger<MemberService> logger)
        : base(memberRepository, logger)
    {
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
}