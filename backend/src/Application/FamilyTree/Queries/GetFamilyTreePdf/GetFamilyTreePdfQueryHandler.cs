using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.FamilyTree.Queries.GetFamilyTreePdf;

public class GetFamilyTreePdfQueryHandler : IRequestHandler<GetFamilyTreePdfQuery, byte[]>
{
    private readonly IApplicationDbContext _context;

    public GetFamilyTreePdfQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> Handle(GetFamilyTreePdfQuery request, CancellationToken cancellationToken)
    {
        var family = await _context.Families
            .Where(f => f.Id == request.FamilyId)
            .FirstOrDefaultAsync(cancellationToken);

        if (family == null)
        {
            throw new backend.Application.Common.Exceptions.NotFoundException(nameof(Family), request.FamilyId);
        }

        // This is a placeholder. Actual PDF generation would involve a library like QuestPDF or iTextSharp.
        // For now, return a dummy PDF content.
        var dummyPdfContent = System.Text.Encoding.UTF8.GetBytes($"Dummy PDF content for Family Tree of {family.Name} (ID: {family.Id})");

        return await Task.FromResult(dummyPdfContent);
    }
}
