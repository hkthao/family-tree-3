using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.Application.Common.Extensions; // New using statement
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.FamilyDicts.Queries;

public record GetFamilyDictsQuery : IRequest<PaginatedList<FamilyDictDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetFamilyDictsQueryHandler : IRequestHandler<GetFamilyDictsQuery, PaginatedList<FamilyDictDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFamilyDictsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<FamilyDictDto>> Handle(GetFamilyDictsQuery request, CancellationToken cancellationToken)
    {
        return await _context.FamilyDicts
            .ProjectTo<FamilyDictDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
