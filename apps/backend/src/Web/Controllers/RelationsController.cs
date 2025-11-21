using backend.Application.Common.Models;
using backend.Application.Relations;
using backend.Application.Relations.Queries;
using backend.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

public class RelationsController : ApiControllerBase
{
    /// <summary>
    /// Lấy tất cả các quan hệ.
    /// </summary>
    /// <param name="query">Đối tượng truy vấn chứa thông tin phân trang.</param>
    /// <returns>Danh sách các quan hệ được phân trang.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<RelationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<RelationDto>>> GetRelations([FromQuery] GetRelationsQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Lấy chi tiết một quan hệ theo ID.
    /// </summary>
    /// <param name="id">ID của quan hệ.</param>
    /// <returns>Chi tiết quan hệ hoặc NotFound nếu không tìm thấy.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RelationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RelationDto>> GetRelationById(string id)
    {
        var relation = await Mediator.Send(new GetRelationByIdQuery(id));
        return relation == null ? NotFound() : Ok(relation);
    }

    /// <summary>
    /// Tìm kiếm các quan hệ theo tiêu chí.
    /// </summary>
    /// <param name="query">Đối tượng truy vấn chứa các tiêu chí tìm kiếm.</param>
    /// <returns>Danh sách các quan hệ phù hợp được phân trang.</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PaginatedList<RelationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<RelationDto>>> SearchRelations([FromQuery] SearchRelationsQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Lấy danh sách các quan hệ đặc biệt.
    /// </summary>
    /// <param name="query">Đối tượng truy vấn chứa thông tin phân trang.</param>
    /// <returns>Danh sách các quan hệ đặc biệt được phân trang.</returns>
    [HttpGet("special")]
    [ProducesResponseType(typeof(PaginatedList<RelationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<RelationDto>>> GetSpecialRelations([FromQuery] GetSpecialRelationsQuery query)
    {
        return await Mediator.Send(query);
    }
}
