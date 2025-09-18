using backend.Domain.Entities;

namespace backend.Application.FamilyTree.Queries.GetFamilyTreePdf;

public class GetFamilyTreePdfQueryHandler : IRequestHandler<GetFamilyTreePdfQuery, byte[]>
{
    public GetFamilyTreePdfQueryHandler()
    {
    }

    public async Task<byte[]> Handle(GetFamilyTreePdfQuery request, CancellationToken cancellationToken)
    {
        Family? family = null;

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
