using backend.Application.Common.Models;
using backend.Application.FamilyDicts; // Use existing DTOs
using backend.Application.FamilyDicts.Queries.Public; // Use new public queries
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers.Public;

[ApiController]
[Route("api/public/family-dict")]
public class PublicFamilyDictsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Lấy tất cả các FamilyDict công khai.
    /// </summary>
    /// <param name="query">Đối tượng truy vấn chứa thông tin phân trang.</param>
    /// <returns>Danh sách các FamilyDict công khai được phân trang.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<FamilyDictDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<FamilyDictDto>>> GetFamilyDicts([FromQuery] GetPublicFamilyDictsQuery query)
    {
        return await _mediator.Send(query);
    }

    /// <summary>
    /// Lấy chi tiết một FamilyDict công khai theo ID.
    /// </summary>
    /// <param name="id">ID của FamilyDict.</param>
    /// <returns>Chi tiết FamilyDict hoặc NotFound nếu không tìm thấy.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FamilyDictDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FamilyDictDto>> GetFamilyDictById(Guid id)
    {
        var familyDict = await _mediator.Send(new GetPublicFamilyDictByIdQuery(id));
        return familyDict == null ? NotFound() : Ok(familyDict);
    }
}
