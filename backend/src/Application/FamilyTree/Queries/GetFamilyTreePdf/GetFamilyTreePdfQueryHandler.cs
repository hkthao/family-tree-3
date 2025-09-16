using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;

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
        var familyObjectId = ObjectId.Parse(request.FamilyId);
        var family = await _context.Families.Find(Builders<Family>.Filter.Eq("_id", familyObjectId)).FirstOrDefaultAsync(cancellationToken);

        if (family == null)
        {
            throw new NotFoundException(nameof(Family), request.FamilyId);
        }

        // This is a placeholder. Actual PDF generation would involve a library like QuestPDF or iTextSharp.
        // For now, return a dummy PDF content.
        var dummyPdfContent = System.Text.Encoding.UTF8.GetBytes($"Dummy PDF content for Family Tree of {family.Name} (ID: {family.Id})");

        return await Task.FromResult(dummyPdfContent);
    }
}
