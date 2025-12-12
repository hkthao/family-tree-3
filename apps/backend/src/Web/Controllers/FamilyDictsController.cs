using backend.Application.Common.Models;
using backend.Application.Common.Security;
using backend.Application.FamilyDicts;
using backend.Application.FamilyDicts.Commands.CreateFamilyDict;
using backend.Application.FamilyDicts.Commands.DeleteFamilyDict;
using backend.Application.FamilyDicts.Commands.ImportFamilyDicts;
using backend.Application.FamilyDicts.Commands.UpdateFamilyDict;
using backend.Application.FamilyDicts.Queries;
using Microsoft.AspNetCore.Mvc;
using backend.Web.Infrastructure; // Added

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/family-dict")]
public class FamilyDictsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;


    /// <summary>
    /// Lấy chi tiết một FamilyDict theo ID.
    /// </summary>
    /// <param name="id">ID của FamilyDict.</param>
    /// <returns>Chi tiết FamilyDict hoặc NotFound nếu không tìm thấy.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FamilyDictDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFamilyDictById(Guid id)
    {
        var result = await _mediator.Send(new GetFamilyDictByIdQuery(id));
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Tìm kiếm các FamilyDict theo tiêu chí.
    /// </summary>
    /// <param name="query">Đối tượng truy vấn chứa các tiêu chí tìm kiếm.</param>
    /// <returns>Danh sách các FamilyDict phù hợp được phân trang.</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PaginatedList<FamilyDictDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchFamilyDicts([FromQuery] SearchFamilyDictsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Tạo mới một FamilyDict.
    /// </summary>
    /// <param name="command">Đối tượng chứa thông tin để tạo FamilyDict.</param>
    /// <returns>ID của FamilyDict vừa tạo.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFamilyDict([FromBody] CreateFamilyDictCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, 201, nameof(GetFamilyDictById), new { id = result.Value });
    }

    /// <summary>
    /// Import một danh sách FamilyDict mới.
    /// </summary>
    /// <param name="command">Đối tượng chứa danh sách các FamilyDict để import.</param>
    /// <returns>Danh sách ID của các FamilyDict vừa tạo.</returns>
    [HttpPost("import")]
    [ProducesResponseType(typeof(IEnumerable<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportFamilyDicts([FromBody] ImportFamilyDictsCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Cập nhật một FamilyDict hiện có.
    /// </summary>
    /// <param name="id">ID của FamilyDict cần cập nhật.</param>
    /// <param name="command">Đối tượng chứa thông tin cập nhật cho FamilyDict.</param>
    /// <returns>NoContent nếu cập nhật thành công.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFamilyDict(Guid id, [FromBody] UpdateFamilyDictCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, 204);
    }

    /// <summary>
    /// Xóa một FamilyDict hiện có.
    /// </summary>
    /// <param name="id">ID của FamilyDict cần xóa.</param>
    /// <returns>NoContent nếu xóa thành công.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFamilyDict(Guid id)
    {
        var result = await _mediator.Send(new DeleteFamilyDictCommand(id));
        return result.ToActionResult(this, 204);
    }
}
