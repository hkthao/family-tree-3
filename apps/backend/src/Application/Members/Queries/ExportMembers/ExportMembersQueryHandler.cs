using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Newtonsoft.Json;

namespace backend.Application.Members.Queries.ExportMembers;

public class ExportMembersQueryHandler : IRequestHandler<ExportMembersQuery, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ExportMembersQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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
        var json = JsonConvert.SerializeObject(memberDtos, Formatting.Indented);

        return Result<string>.Success(json);
    }
}
