using AutoMapper;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;

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
        var filter = Builders<Member>.Filter.Eq("_id", ObjectId.Parse(request.Id!));
        var member = await _context.Members.Find(filter).FirstOrDefaultAsync(cancellationToken);

        if (member == null)
        {
            throw new NotFoundException(nameof(Member), request.Id!);
        }

        return _mapper.Map<MemberDto>(member);
    }
}
