using MediatR;
using MongoDB.Bson;

namespace backend.Application.FamilyTree.Queries.GetFamilyTreePdf;

public record GetFamilyTreePdfQuery(string FamilyId) : IRequest<byte[]>;
