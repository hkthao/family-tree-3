using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.Application.FamilyLinks.Queries; // New using directive

namespace backend.Application.FamilyLinkRequests.Queries.GetFamilyLinkRequests;

public class GetFamilyLinkRequestsQueryHandler : IRequestHandler<GetFamilyLinkRequestsQuery, Result<List<FamilyLinkRequestDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUser _currentUser;

    public GetFamilyLinkRequestsQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser currentUser)
    {
        _context = context;
        _mapper = mapper;
        _authorizationService = authorizationService;
        _currentUser = currentUser;
    }

    public async Task<Result<List<FamilyLinkRequestDto>>> Handle(GetFamilyLinkRequestsQuery request, CancellationToken cancellationToken)
    {
        // 1. Authorization: User must be a member of the family to view its links
        if (!_authorizationService.CanAccessFamily(request.FamilyId))
        {
            return Result<List<FamilyLinkRequestDto>>.Forbidden("Bạn không có quyền xem các yêu cầu liên kết của gia đình này.");
        }

        var requests = await _context.FamilyLinkRequests
            .Include(flr => flr.RequestingFamily)
            .Include(flr => flr.TargetFamily)
            .Where(flr => flr.RequestingFamilyId == request.FamilyId || flr.TargetFamilyId == request.FamilyId)
            .ProjectTo<FamilyLinkRequestDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<FamilyLinkRequestDto>>.Success(requests);
    }
}
