using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Interfaces.Services;
using backend.Application.Common.Models;
using Newtonsoft.Json;

namespace backend.Application.Members.Queries.ExportMembers;

public class ExportMembersQueryHandler : IRequestHandler<ExportMembersQuery, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPrivacyService _privacyService;

    public ExportMembersQueryHandler(IApplicationDbContext context, IMapper mapper, IPrivacyService privacyService)
    {
        _context = context;
        _mapper = mapper;
        _privacyService = privacyService;
    }

    public async Task<Result<string>> Handle(ExportMembersQuery request, CancellationToken cancellationToken)
    {
        var members = await _context.Members
            .Where(m => m.FamilyId == request.FamilyId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (!members.Any())
        {
            return Result<string>.Failure("Không tìm thấy thành viên nào cho gia đình này.");
        }

        var memberDtos = _mapper.Map<List<MemberDto>>(members); // Changed to MemberDto
        var filteredMemberDtos = new List<MemberDto>();
        foreach (var memberDto in memberDtos)
        {
            filteredMemberDtos.Add(await _privacyService.ApplyPrivacyFilter(memberDto, request.FamilyId, cancellationToken));
        }
        var json = JsonConvert.SerializeObject(filteredMemberDtos, Formatting.Indented);

        return Result<string>.Success(json);
    }
}
