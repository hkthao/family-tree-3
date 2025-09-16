using backend.Application.Common.Interfaces;
using backend.Application.FamilyTree.Queries.GetFamilyTreeJson;
using backend.Application.FamilyTree.Queries.GetFamilyTreePdf;
using backend.Application.FamilyTree;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Threading.Tasks;

namespace backend.Web.Endpoints;

public class FamilyTree : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapGet("/{familyId}", GetFamilyTreeJson);
        group.MapGet("/{familyId}/pdf", GetFamilyTreePdf);
    }

    public async Task<Ok<FamilyTreeDto>> GetFamilyTreeJson(ISender sender, string familyId)
    {
        var result = await sender.Send(new GetFamilyTreeJsonQuery(familyId));
        return TypedResults.Ok(result);
    }

    public async Task<FileContentHttpResult> GetFamilyTreePdf(ISender sender, string familyId)
    {
        var result = await sender.Send(new GetFamilyTreePdfQuery(familyId));
        return TypedResults.File(result, "application/pdf", $"family-tree-{familyId}.pdf");
    }
}
