using AutoMapper;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Members.Queries.GetMemberById;

public class GetMemberByIdQueryHandler : IRequestHandler<GetMemberByIdQuery, MemberDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMemberByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MemberDto> Handle(GetMemberByIdQuery request, CancellationToken cancellationToken)
    {
        var member = await _context.Members.FindAsync(new object[] { request.Id }, cancellationToken);

        if (member == null)
        {
            throw new backend.Application.Common.Exceptions.NotFoundException(nameof(Member), request.Id);
        }

        return _mapper.Map<MemberDto>(member);
    }
}
