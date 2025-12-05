using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.Application.FamilyLinks.Queries; // New using directive

namespace backend.Application.FamilyLinkRequests.Queries.GetFamilyLinkRequestById;

public class GetFamilyLinkRequestByIdQueryHandler : IRequestHandler<GetFamilyLinkRequestByIdQuery, Result<FamilyLinkRequestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUser _currentUser;

    public GetFamilyLinkRequestByIdQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, ICurrentUser currentUser)
    {
        _context = context;
        _mapper = mapper;
        _authorizationService = authorizationService;
        _currentUser = currentUser;
    }

    public async Task<Result<FamilyLinkRequestDto>> Handle(GetFamilyLinkRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var familyLinkRequest = await _context.FamilyLinkRequests
            .Include(flr => flr.RequestingFamily)
            .Include(flr => flr.TargetFamily)
            .ProjectTo<FamilyLinkRequestDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(flr => flr.Id == request.Id, cancellationToken);

        if (familyLinkRequest == null)
        {
            return Result<FamilyLinkRequestDto>.NotFound("Không tìm thấy yêu cầu liên kết gia đình.");
        }

        // Authorization check: User must be a member of either requesting or target family
        if (!_authorizationService.CanAccessFamily(familyLinkRequest.RequestingFamilyId) &&
            !_authorizationService.CanAccessFamily(familyLinkRequest.TargetFamilyId))
        {
            return Result<FamilyLinkRequestDto>.Forbidden("Bạn không có quyền xem yêu cầu liên kết gia đình này.");
        }

        return Result<FamilyLinkRequestDto>.Success(familyLinkRequest);
    }
}
