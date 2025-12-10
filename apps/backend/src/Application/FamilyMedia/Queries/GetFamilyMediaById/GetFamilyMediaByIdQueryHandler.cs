using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.FamilyMedia.DTOs;
using backend.Application.FamilyMedia.Queries.Specifications;


namespace backend.Application.FamilyMedia.Queries.GetFamilyMediaById;

public class GetFamilyMediaByIdQueryHandler : IRequestHandler<GetFamilyMediaByIdQuery, Result<FamilyMediaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMapper _mapper;

    public GetFamilyMediaByIdQueryHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IMapper mapper)
    {
        _context = context;
        _authorizationService = authorizationService;
        _mapper = mapper;
    }

    public async Task<Result<FamilyMediaDto>> Handle(GetFamilyMediaByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.CanViewFamily(request.FamilyId))
        {
            return Result<FamilyMediaDto>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var specification = new FamilyMediaByIdSpecification(request.Id, request.FamilyId);

        var familyMedia = await _context.FamilyMedia
            .WithSpecification(specification)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (familyMedia == null)
        {
            return Result<FamilyMediaDto>.Failure(string.Format(ErrorMessages.NotFound, $"FamilyMedia with ID {request.Id}"), ErrorSources.NotFound);
        }

        return Result<FamilyMediaDto>.Success(_mapper.Map<FamilyMediaDto>(familyMedia));
    }
}
