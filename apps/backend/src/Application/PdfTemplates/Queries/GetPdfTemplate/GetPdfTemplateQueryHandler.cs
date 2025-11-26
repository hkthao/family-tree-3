using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.PdfTemplates.Dtos;

namespace backend.Application.PdfTemplates.Queries.GetPdfTemplate;

public class GetPdfTemplateQueryHandler(
    IApplicationDbContext context,
    IMapper mapper,
    IAuthorizationService authorizationService)
    : IRequestHandler<GetPdfTemplateQuery, Result<PdfTemplateDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<PdfTemplateDto>> Handle(GetPdfTemplateQuery request, CancellationToken cancellationToken)
    {
        // Authorization: Ensure the user can access the family this template belongs to
        if (!_authorizationService.CanAccessFamily(request.FamilyId))
        {
            return Result<PdfTemplateDto>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var entity = await _context.PdfTemplates
            .AsNoTracking()
            .Where(t => t.Id == request.Id && t.FamilyId == request.FamilyId)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return Result<PdfTemplateDto>.Failure($"PdfTemplate with ID {request.Id} not found or does not belong to Family ID {request.FamilyId}.", ErrorSources.NotFound);
        }

        var dto = _mapper.Map<PdfTemplateDto>(entity);
        return Result<PdfTemplateDto>.Success(dto);
    }
}
