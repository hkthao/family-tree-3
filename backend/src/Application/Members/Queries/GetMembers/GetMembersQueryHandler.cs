using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Members;
using backend.Domain.Entities;
using MediatR;
using MongoDB.Driver;

namespace backend.Application.Members.Queries.GetMembers;

public class GetMembersQueryHandler : IRequestHandler<GetMembersQuery, List<MemberDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMembersQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<MemberDto>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var members = await _context.Members.Find(_ => true).ToListAsync(cancellationToken);
        return _mapper.Map<List<MemberDto>>(members);
    }
}
