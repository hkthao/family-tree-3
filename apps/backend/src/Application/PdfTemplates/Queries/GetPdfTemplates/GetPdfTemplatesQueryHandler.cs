using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.PdfTemplates.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.PdfTemplates.Queries.GetPdfTemplates;

public class GetPdfTemplatesQueryHandler(
    IApplicationDbContext context,
    IMapper mapper,
    IAuthorizationService authorizationService)
    : IRequestHandler<GetPdfTemplatesQuery, Result<List<PdfTemplateDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<List<PdfTemplateDto>>> Handle(GetPdfTemplatesQuery request, CancellationToken cancellationToken)
    {
        // Authorization: Ensure the user can access the family whose templates are being requested
        if (!_authorizationService.CanAccessFamily(request.FamilyId))
        {
            return Result<List<PdfTemplateDto>>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var entities = await _context.PdfTemplates
            .AsNoTracking()
            .Where(t => t.FamilyId == request.FamilyId)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<PdfTemplateDto>>(entities);
        return Result<List<PdfTemplateDto>>.Success(dtos);
    }
}
