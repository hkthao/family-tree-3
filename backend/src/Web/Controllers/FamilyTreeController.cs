using backend.Application.Common.Interfaces;
using backend.Application.FamilyTree;
using backend.Application.FamilyTree.Queries.GetFamilyTreeJson;
using backend.Application.FamilyTree.Queries.GetFamilyTreePdf;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FamilyTreeController : ControllerBase
{
    private readonly ISender _sender;

    public FamilyTreeController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{familyId}")]
    public async Task<ActionResult<FamilyTreeDto>> GetFamilyTreeJson(Guid familyId)
    {
        var result = await _sender.Send(new GetFamilyTreeJsonQuery(familyId));
        return Ok(result);
    }

    [HttpGet("{familyId}/pdf")]
    public async Task<IActionResult> GetFamilyTreePdf(Guid familyId)
    {
        var result = await _sender.Send(new GetFamilyTreePdfQuery(familyId));
        return File(result, "application/pdf", $"family-tree-{familyId}.pdf");
    }
}