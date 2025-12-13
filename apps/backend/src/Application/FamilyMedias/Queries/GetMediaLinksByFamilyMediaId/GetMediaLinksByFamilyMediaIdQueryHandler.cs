using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.FamilyMedias.DTOs;
using backend.Application.FamilyMedias.Queries.Specifications;

namespace backend.Application.FamilyMedias.Queries.GetMediaLinksByFamilyMediaId;

public class GetMediaLinksByFamilyMediaIdQueryHandler : IRequestHandler<GetMediaLinksByFamilyMediaIdQuery, Result<List<MediaLinkDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMapper _mapper;

    public GetMediaLinksByFamilyMediaIdQueryHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMapper mapper)
    {
        _context = context;
        _authorizationService = authorizationService;
        _mapper = mapper;
    }

    public async Task<Result<List<MediaLinkDto>>> Handle(GetMediaLinksByFamilyMediaIdQuery request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.CanViewFamily(request.FamilyId))
        {
            return Result<List<MediaLinkDto>>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // First, check if the FamilyMedia itself exists and belongs to the family
        var familyMediaExists = await _context.FamilyMedia
            .AnyAsync(fm => fm.Id == request.FamilyMediaId && fm.FamilyId == request.FamilyId && !fm.IsDeleted, cancellationToken);

        if (!familyMediaExists)
        {
            return Result<List<MediaLinkDto>>.Failure(string.Format(ErrorMessages.NotFound, $"FamilyMedia with ID {request.FamilyMediaId}"), ErrorSources.NotFound);
        }

        var specification = new MediaLinksByFamilyMediaIdSpecification(request.FamilyMediaId);

        var mediaLinks = await _context.MediaLinks
            .WithSpecification(specification)
            .AsNoTracking()
            .ProjectTo<MediaLinkDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<List<MediaLinkDto>>.Success(mediaLinks);
    }
}
