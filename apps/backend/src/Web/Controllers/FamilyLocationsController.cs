using backend.Application.Common.Models;
using backend.Application.FamilyLocations.Queries.GetFamilyLocationById;
using backend.Application.FamilyLocations.Queries.SearchFamilyLocations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend.Application.FamilyLocations.Commands.CreateFamilyLocation;
using backend.Application.FamilyLocations.Commands.UpdateFamilyLocation;
using backend.Application.FamilyLocations.Commands.DeleteFamilyLocation;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/family-locations")]
public class FamilyLocationsController(IMediator mediator, ILogger<FamilyLocationsController> logger) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<FamilyLocationsController> _logger = logger;

    /// <summary>
    /// Lấy FamilyLocation theo ID.
    /// </summary>
    /// <param name="id">ID của FamilyLocation.</param>
    /// <returns>FamilyLocationDto.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFamilyLocationById(Guid id)
    {
        var result = await _mediator.Send(new GetFamilyLocationByIdQuery(id));
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Tìm kiếm và lấy danh sách các FamilyLocation.
    /// </summary>
    /// <param name="query">Các tiêu chí tìm kiếm và phân trang.</param>
    /// <returns>Danh sách FamilyLocation được phân trang.</returns>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchFamilyLocations([FromQuery] SearchFamilyLocationsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Tạo một FamilyLocation mới.
    /// </summary>
    /// <param name="command">Dữ liệu FamilyLocation cần tạo.</param>
    /// <returns>ID của FamilyLocation mới được tạo.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFamilyLocation(CreateFamilyLocationCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 201, nameof(GetFamilyLocationById), new { id = result.Value });
    }

    /// <summary>
    /// Cập nhật thông tin FamilyLocation hiện có.
    /// </summary>
    /// <param name="id">ID của FamilyLocation cần cập nhật.</param>
    /// <param name="command">Dữ liệu FamilyLocation cần cập nhật.</param>
    /// <returns>ActionResult.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFamilyLocation(Guid id, UpdateFamilyLocationCommand command)
    {
        if (id != command.Id)
        {
            _logger.LogWarning("ID trong URL ({IdInUrl}) và ID trong body ({IdInBody}) không khớp cho UpdateFamilyLocationCommand từ {RemoteIpAddress}", id, command.Id, HttpContext.Connection.RemoteIpAddress);
            return BadRequest(Result.Failure("ID trong URL và ID trong body không khớp."));
        }

        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 204);
    }

    /// <summary>
    /// Xóa một FamilyLocation.
    /// </summary>
    /// <param name="id">ID của FamilyLocation cần xóa.</param>
    /// <returns>ActionResult.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFamilyLocation(Guid id)
    {
        var result = await _mediator.Send(new DeleteFamilyLocationCommand(id));
        return result.ToActionResult(this, _logger, 204);
    }
}