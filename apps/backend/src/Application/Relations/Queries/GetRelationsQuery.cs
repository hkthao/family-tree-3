using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.Application.Common.Extensions; // New using statement
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Relations.Queries;

public record GetRelationsQuery : IRequest<PaginatedList<RelationDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetRelationsQueryHandler : IRequestHandler<GetRelationsQuery, PaginatedList<RelationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetRelationsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<RelationDto>> Handle(GetRelationsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Relations
            .ProjectTo<RelationDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
