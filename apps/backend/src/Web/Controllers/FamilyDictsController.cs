using backend.Application.Common.Models;
using backend.Application.FamilyDicts;
using backend.Application.FamilyDicts.Queries;
using backend.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

using MediatR; // New using statement

namespace backend.Web.Controllers;

public class FamilyDictsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Lấy tất cả các FamilyDict.
    /// </summary>
    /// <param name="query">Đối tượng truy vấn chứa thông tin phân trang.</param>
    /// <returns>Danh sách các FamilyDict được phân trang.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<FamilyDictDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<FamilyDictDto>>> GetFamilyDicts([FromQuery] GetFamilyDictsQuery query)
    {
        return await _mediator.Send(query);
    }

    /// <summary>
    /// Lấy chi tiết một FamilyDict theo ID.
    /// </summary>
    /// <param name="id">ID của FamilyDict.</param>
    /// <returns>Chi tiết FamilyDict hoặc NotFound nếu không tìm thấy.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FamilyDictDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FamilyDictDto>> GetFamilyDictById(string id)
    {
        var familyDict = await _mediator.Send(new GetFamilyDictByIdQuery(id));
        return familyDict == null ? NotFound() : Ok(familyDict);
    }

    /// <summary>
    /// Tìm kiếm các FamilyDict theo tiêu chí.
    /// </summary>
    /// <param name="query">Đối tượng truy vấn chứa các tiêu chí tìm kiếm.</param>
    /// <returns>Danh sách các FamilyDict phù hợp được phân trang.</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PaginatedList<FamilyDictDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<FamilyDictDto>>> SearchFamilyDicts([FromQuery] SearchFamilyDictsQuery query)
    {
        return await _mediator.Send(query);
    }

    /// <summary>
    /// Lấy danh sách các FamilyDict đặc biệt.
    /// </summary>
    /// <param name="query">Đối tượng truy vấn chứa thông tin phân trang.</param>
    /// <returns>Danh sách các FamilyDict đặc biệt được phân trang.</returns>
    [HttpGet("special")]
    [ProducesResponseType(typeof(PaginatedList<FamilyDictDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<FamilyDictDto>>> GetSpecialFamilyDicts([FromQuery] GetSpecialFamilyDictsQuery query)
    {
        return await _mediator.Send(query);
    }
}
