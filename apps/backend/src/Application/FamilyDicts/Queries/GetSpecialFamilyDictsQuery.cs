using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.Application.Common.Extensions; // New using statement
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.FamilyDicts.Queries;

public record GetSpecialFamilyDictsQuery : IRequest<PaginatedList<FamilyDictDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetSpecialFamilyDictsQueryHandler : IRequestHandler<GetSpecialFamilyDictsQuery, PaginatedList<FamilyDictDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetSpecialFamilyDictsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<FamilyDictDto>> Handle(GetSpecialFamilyDictsQuery request, CancellationToken cancellationToken)
    {
        return await _context.FamilyDicts
            .Where(r => r.SpecialRelation)
            .ProjectTo<FamilyDictDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
