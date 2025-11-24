using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Application.PdfTemplates.Commands.UpdatePdfTemplate;

public class UpdatePdfTemplateCommandHandler(
    IApplicationDbContext context,
    IAuthorizationService authorizationService)
    : IRequestHandler<UpdatePdfTemplateCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<Result<Unit>> Handle(UpdatePdfTemplateCommand request, CancellationToken cancellationToken)
    {
        // Authorization: Ensure the user can access the family this template belongs to
        if (!_authorizationService.CanAccessFamily(request.FamilyId))
        {
            return Result<Unit>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var entity = await _context.PdfTemplates
            .Where(t => t.Id == request.Id && t.FamilyId == request.FamilyId)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return Result<Unit>.Failure($"PdfTemplate with ID {request.Id} not found or does not belong to Family ID {request.FamilyId}.", ErrorSources.NotFound);
        }

        entity.Update(
            request.Name,
            request.HtmlContent,
            request.CssContent,
            request.Placeholders
        );

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}