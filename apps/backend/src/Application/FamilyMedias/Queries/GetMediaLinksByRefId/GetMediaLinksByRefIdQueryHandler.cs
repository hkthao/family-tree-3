using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.FamilyMedias.DTOs;
using backend.Application.FamilyMedias.Queries.Specifications;

namespace backend.Application.FamilyMedias.Queries.GetMediaLinksByRefId;

public class GetMediaLinksByRefIdQueryHandler : IRequestHandler<GetMediaLinksByRefIdQuery, Result<List<MediaLinkDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMapper _mapper;

    public GetMediaLinksByRefIdQueryHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMapper mapper)
    {
        _context = context;
        _authorizationService = authorizationService;
        _mapper = mapper;
    }

    public async Task<Result<List<MediaLinkDto>>> Handle(GetMediaLinksByRefIdQuery request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.CanViewFamily(request.FamilyId))
        {
            return Result<List<MediaLinkDto>>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var specification = new MediaLinksByRefIdSpecification(request.RefId, request.RefType, request.FamilyId);

        var mediaLinks = await _context.MediaLinks
            .WithSpecification(specification)
            .AsNoTracking()
            .ProjectTo<MediaLinkDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<MediaLinkDto>>.Success(mediaLinks);
    }
}
