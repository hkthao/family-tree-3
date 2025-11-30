using backend.Application.Common.Constants; // Added to resolve ErrorSources
using backend.Application.Common.Models;
using backend.Application.MemberFaces.Commands.CreateMemberFace;
using backend.Application.MemberFaces.Commands.DeleteMemberFace;
using backend.Application.MemberFaces.Commands.UpdateMemberFace;
using backend.Application.MemberFaces.Queries.GetMemberFaceById;
using backend.Application.MemberFaces.Queries.MemberFaces; // For MemberFaceDto
using backend.Application.MemberFaces.Queries.SearchMemberFaces;
using MediatR; // Required for Unit.Value
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/memberfaces")]
public class MemberFacesController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<Result<PaginatedList<MemberFaceDto>>>> SearchMemberFaces([FromQuery] SearchMemberFacesQuery query)
    {
        return await _mediator.Send(query);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<MemberFaceDto>>> GetMemberFaceById(Guid id)
    {
        return await _mediator.Send(new GetMemberFaceByIdQuery { Id = id });
    }

    [HttpPost]
    public async Task<ActionResult<Result<Guid>>> CreateMemberFace(CreateMemberFaceCommand command)
    {
        return await _mediator.Send(command);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Result<Unit>>> UpdateMemberFace(Guid id, UpdateMemberFaceCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(Result<Unit>.Failure("Mismatched ID in URL and request body.", ErrorSources.Validation));
        }
        return await _mediator.Send(command);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Result<Unit>>> DeleteMemberFace(Guid id)
    {
        return await _mediator.Send(new DeleteMemberFaceCommand { Id = id });
    }
}
