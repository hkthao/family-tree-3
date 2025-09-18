namespace backend.Application.FamilyTree.Queries.GetFamilyTreePdf;

public record GetFamilyTreePdfQuery(Guid FamilyId) : IRequest<byte[]>;
