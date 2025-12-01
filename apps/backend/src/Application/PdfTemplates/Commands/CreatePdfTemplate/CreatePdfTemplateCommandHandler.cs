using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.PdfTemplates.Dtos;
using backend.Domain.Entities;

namespace backend.Application.PdfTemplates.Commands.CreatePdfTemplate;

public class CreatePdfTemplateCommandHandler(
    IApplicationDbContext context,
    IMapper mapper,
    IAuthorizationService authorizationService)
    : IRequestHandler<CreatePdfTemplateCommand, Result<PdfTemplateDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<PdfTemplateDto>> Handle(CreatePdfTemplateCommand request, CancellationToken cancellationToken)
    {
        // Authorization: Ensure the user can access the family this template is for
        if (!_authorizationService.CanAccessFamily(request.FamilyId))
        {
            return Result<PdfTemplateDto>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var entity = PdfTemplate.Create(
            request.Name,
            request.HtmlContent,
            request.CssContent,
            request.Placeholders,
            request.FamilyId
        );

        _context.PdfTemplates.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<PdfTemplateDto>(entity);
        return Result<PdfTemplateDto>.Success(dto);
    }
}
