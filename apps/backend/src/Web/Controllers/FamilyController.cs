using backend.Application.Common.Constants;
using backend.Application.Common.Models;
using backend.Application.Families.Commands.CreateFamilies;
using backend.Application.Families.Commands.CreateFamily;
using backend.Application.Families.Commands.DeleteFamily;
using backend.Application.Families.Commands.GenerateFamilyData; // ADDED: New using directive
using backend.Application.Families.Commands.ResetFamilyAiChatQuota; // ADDED
using backend.Application.Families.Commands.UpdateFamily;
using backend.Application.Families.Commands.UpdateFamilyLimitConfiguration;
using backend.Application.Families.Commands.UpdatePrivacyConfiguration;
using backend.Application.Families.Queries;
using backend.Application.Families.Queries.GetFamiliesByIds;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Application.Families.Queries.GetPrivacyConfiguration;
using backend.Application.Families.Queries.GetUserFamilyAccessQuery;
using backend.Application.Families.Queries.SearchFamilies;
using backend.Application.Families.Queries.SearchPublicFamilies; // NEW
using backend.Application.Families.Commands.Import;
using backend.Application.Families.Commands.RecalculateFamilyStats; // NEW
using backend.Application.Families.Commands.FixFamilyRelationships; // NEW
using backend.Application.Families.Queries.GetFamilyTreeData; // NEWLY ADDED
using backend.Application.Members.Commands.UpdateDenormalizedFields;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace backend.Web.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các yêu cầu liên quan đến gia đình.
/// </summary>
/// <param name="mediator">Đối tượng IMediator để gửi các lệnh và truy vấn.</param>
[Authorize]
[ApiController]
[Route("api/family")]
[EnableRateLimiting(RateLimitConstants.PerUserPolicy)]
public class FamilyController(IMediator mediator, ILogger<FamilyController> logger) : ControllerBase
{
    /// <summary>
    /// Đối tượng IMediator để gửi các lệnh và truy vấn.
    /// </summary>
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Đối tượng ILogger để ghi log.
    /// </summary>
    private readonly ILogger<FamilyController> _logger = logger;

    /// <summary>
    /// Lấy danh sách các gia đình mà người dùng hiện tại có quyền truy cập (Manager hoặc Viewer).
    /// </summary>
    /// <returns>Danh sách các đối tượng FamilyAccessDto.</returns>
    [HttpGet("my-access")]
    public async Task<IActionResult> GetUserFamilyAccess()
    {
        var result = await _mediator.Send(new GetUserFamilyAccessQuery());
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Xử lý GET request để lấy thông tin chi tiết của một gia đình theo ID.
    /// </summary>
    /// <param name="id">ID của gia đình cần lấy.</param>
    /// <returns>Thông tin chi tiết của gia đình.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFamilyById(Guid id)
    {
        var result = await _mediator.Send(new GetFamilyByIdQuery(id));
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Xử lý GET request để lấy thông tin chi tiết của một gia đình theo mã Code.
    /// </summary>
    /// <param name="code">Mã Code của gia đình cần lấy.</param>
    /// <returns>Thông tin chi tiết của gia đình.</returns>
    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetFamilyByCode(string code)
    {
        var result = await _mediator.Send(new GetFamilyByCodeQuery(code));
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Xử lý GET request để tìm kiếm gia đình dựa trên các tiêu chí được cung cấp.
    /// </summary>
    /// <param name="query">Đối tượng chứa các tiêu chí tìm kiếm và phân trang.</param>
    /// <returns>Một PaginatedList chứa danh sách các gia đình tìm được.</returns>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] SearchFamiliesQuery query)
    {
        var result = await _mediator.Send(query);
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Xử lý GET request để tìm kiếm gia đình công khai dựa trên các tiêu chí được cung cấp.
    /// </summary>
    /// <param name="query">Đối tượng chứa các tiêu chí tìm kiếm và phân trang.</param>
    /// <returns>Một PaginatedList chứa danh sách các gia đình công khai tìm được.</returns>
    [HttpGet("public-search")]
    public async Task<IActionResult> SearchPublic([FromQuery] SearchPublicFamiliesQuery query)
    {
        var result = await _mediator.Send(query);
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Xử lý POST request để tạo một gia đình mới.
    /// </summary>
    /// <param name="command">Lệnh tạo gia đình với thông tin chi tiết.</param>
    /// <returns>ID của gia đình vừa được tạo.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateFamily([FromBody] CreateFamilyCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 201, nameof(GetFamilyById), new { id = result.Value });
    }

    /// <summary>
    /// Xử lý POST request để tạo nhiều gia đình cùng lúc.
    /// </summary>
    /// <param name="command">Lệnh tạo nhiều gia đình với danh sách thông tin chi tiết.</param>
    /// <returns>Danh sách ID của các gia đình vừa được tạo.</returns>
    [HttpPost("bulk-create")]
    public async Task<IActionResult> CreateFamilies([FromBody] CreateFamiliesCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Xử lý POST request để nhập dữ liệu gia đình từ file JSON.
    /// Endpoint này chỉ tạo mới thông tin gia đình, không hỗ trợ cập nhật.
    /// </summary>
    /// <param name="familyData">Dữ liệu gia đình cần nhập.</param>
    /// <param name="clearExistingData">Tùy chọn có xóa dữ liệu hiện có (chỉ áp dụng khi cập nhật, nhưng ở đây luôn tạo mới).</param>
    /// <returns>ID của gia đình vừa được tạo.</returns>
    [HttpPost("import")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ImportFamily([FromBody] FamilyImportDto familyData, [FromQuery] bool clearExistingData = true)
    {
        var command = new ImportFamilyCommand
        {
            FamilyId = null, // Luôn tạo mới gia đình
            FamilyData = familyData,
            ClearExistingData = clearExistingData // Tùy chọn này sẽ bị bỏ qua khi tạo mới
        };
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger);
    }



    /// <summary>
    /// Xử lý PUT request để cập nhật thông tin của một gia đình.
    /// </summary>
    /// <param name="id">ID của gia đình cần cập nhật.</param>
    /// <param name="command">Lệnh cập nhật gia đình với thông tin mới.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFamily([FromRoute] Guid id, [FromBody] UpdateFamilyCommand command)
    {
        if (id != command.Id)
        {
            _logger.LogWarning("Mismatched ID in URL ({Id}) and request body ({CommandId}) for UpdateFamilyCommand from {RemoteIpAddress}", id, command.Id, HttpContext.Connection.RemoteIpAddress);
            return BadRequest();
        }
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 204);
    }

    /// <summary>
    /// Xử lý DELETE request để xóa một gia đình.
    /// </summary>
    /// <param name="id">ID của gia đình cần xóa.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFamily(Guid id)
    {
        var result = await _mediator.Send(new DeleteFamilyCommand(id));
        return result.ToActionResult(this, _logger, 204);
    }

    /// <summary>
    /// Xử lý GET request để lấy danh sách các gia đình theo nhiều ID.
    /// </summary>
    /// <param name="ids">Chuỗi chứa các ID gia đình, phân tách bằng dấu phẩy.</param>
    /// <returns>Danh sách các đối tượng FamilyDto.</returns>
    [HttpGet("by-ids")]
    public async Task<IActionResult> GetFamiliesByIds([FromQuery] string ids)
    {
        if (string.IsNullOrEmpty(ids))
            return Result<List<FamilyDto>>.Success([]).ToActionResult(this, _logger);

        var guids = ids.Split(',').Select(Guid.Parse).ToList();
        var result = await _mediator.Send(new GetFamiliesByIdsQuery(guids));
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Cập nhật các trường mối quan hệ đã chuẩn hóa cho tất cả các thành viên trong một gia đình cụ thể.
    /// </summary>
    /// <param name="familyId">ID của gia đình cần cập nhật.</param>
    /// <returns>Kết quả của hoạt động cập nhật.</returns>
    [HttpPost("{familyId}/update-denormalized-fields")]
    public async Task<IActionResult> UpdateDenormalizedFields([FromRoute] Guid familyId)
    {
        var result = await _mediator.Send(new UpdateDenormalizedFieldsCommand(familyId));
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Lấy cấu hình riêng tư cho một gia đình cụ thể.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <returns>Cấu hình riêng tư của gia đình.</returns>
    [HttpGet("{familyId}/privacy-configuration")]
    public async Task<IActionResult> GetPrivacyConfiguration(Guid familyId)
    {
        var result = await _mediator.Send(new GetPrivacyConfigurationQuery(familyId));
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Cập nhật cấu hình riêng tư cho một gia đình cụ thể.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="command">Lệnh chứa dữ liệu cấu hình riêng tư đã cập nhật.</param>
    /// <returns>Kết quả của hoạt động cập nhật.</returns>
    [HttpPut("{familyId}/privacy-configuration")]
    public async Task<IActionResult> UpdatePrivacyConfiguration(Guid familyId, [FromBody] UpdatePrivacyConfigurationCommand command)
    {
        if (familyId != command.FamilyId)
        {
            _logger.LogWarning("Mismatched FamilyId in URL ({FamilyId}) and command body ({CommandFamilyId}) for UpdatePrivacyConfigurationCommand from {RemoteIpAddress}", familyId, command.FamilyId, HttpContext.Connection.RemoteIpAddress);
            return BadRequest("FamilyId in URL does not match command body.");
        }
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 204);
    }

    /// <summary>
    /// Lấy cấu hình giới hạn cho một gia đình cụ thể.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <returns>Cấu hình giới hạn của gia đình.</returns>
    [HttpGet("{familyId}/limit-configuration")]
    public async Task<IActionResult> GetFamilyLimitConfiguration(Guid familyId)
    {
        var result = await _mediator.Send(new GetFamilyLimitConfigurationQuery { FamilyId = familyId });
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Cập nhật cấu hình giới hạn cho một gia đình cụ thể.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="command">Lệnh chứa dữ liệu cấu hình giới hạn đã cập nhật.</param>
    /// <returns>Kết quả của hoạt động cập nhật.</returns>
    [HttpPut("{familyId}/limit-configuration")]
    public async Task<IActionResult> UpdateFamilyLimitConfiguration(Guid familyId, [FromBody] UpdateFamilyLimitConfigurationCommand command)
    {
        if (familyId != command.FamilyId)
        {
            _logger.LogWarning("Mismatched FamilyId in URL ({FamilyId}) and command body ({CommandFamilyId}) for UpdateFamilyLimitConfigurationCommand from {RemoteIpAddress}", familyId, command.FamilyId, HttpContext.Connection.RemoteIpAddress);
            return BadRequest("FamilyId in URL does not match command body.");
        }
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 204);
    }

    /// <summary>
    /// Tạo nội dung AI dựa trên đầu vào của người dùng và loại yêu cầu.
    /// </summary>
    /// <param name="familyId">ID của gia đình liên quan đến yêu cầu.</param>
    /// <param name="command">Lệnh chứa FamilyId, ChatInput, và ContentType.</param>
    /// <returns>Nội dung được tạo bởi AI (JSON hoặc văn bản).</returns>
    [HttpPost("{familyId}/generate-data")]
    public async Task<IActionResult> GenerateFamilyData([FromRoute] Guid familyId, [FromBody] GenerateFamilyDataCommand command)
    {
        if (familyId != command.FamilyId)
        {
            _logger.LogWarning("Mismatched FamilyId in URL ({FamilyId}) and command body ({CommandFamilyId}) for GenerateFamilyDataCommand from {RemoteIpAddress}", familyId, command.FamilyId, HttpContext.Connection.RemoteIpAddress);
            return BadRequest("FamilyId in URL does not match command body.");
        }
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Đặt lại hạn ngạch trò chuyện AI hàng tháng cho một gia đình cụ thể.
    /// </summary>
    /// <param name="familyId">ID của gia đình cần đặt lại hạn ngạch.</param>
    /// <returns>Kết quả của hoạt động.</returns>
    [HttpPost("{familyId}/reset-ai-chat-quota")]
    public async Task<IActionResult> ResetFamilyAiChatQuota(Guid familyId)
    {
        var result = await _mediator.Send(new ResetFamilyAiChatQuotaCommand { FamilyId = familyId });
        return result.ToActionResult(this, _logger, 204);
    }

    /// <summary>
    /// Xử lý POST request để tính toán lại tổng số thành viên và tổng số thế hệ cho một gia đình.
    /// </summary>
    /// <param name="familyId">ID của gia đình cần tính toán lại.</param>
    /// <returns>Thống kê gia đình đã được cập nhật.</returns>
    [HttpPost("{familyId}/recalculate-stats")]
    public async Task<IActionResult> RecalculateFamilyStats([FromRoute] Guid familyId)
    {
        var command = new RecalculateFamilyStatsCommand { FamilyId = familyId };
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Lấy dữ liệu cây gia phả (thành viên và mối quan hệ) cho một gia đình.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="initialMemberId">ID thành viên ban đầu để làm gốc. Nếu null, sẽ lấy thành viên gốc của gia phả.</param>
    /// <returns>Dữ liệu cây gia phả.</returns>
    [HttpGet("{familyId}/tree-data")]
    public async Task<IActionResult> GetFamilyTreeData(
        [FromRoute] Guid familyId,
        [FromQuery] Guid? initialMemberId)
    {
        var query = new GetFamilyTreeDataQuery(familyId, initialMemberId);
        var result = await _mediator.Send(query);
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Sửa lỗi quan hệ trong gia đình (ví dụ: hoán đổi Father/Mother nếu giới tính sai, suy luận cha/mẹ từ vợ/chồng).
    /// </summary>
    /// <param name="familyId">ID của gia đình cần sửa lỗi quan hệ.</param>
    /// <returns>Kết quả của hoạt động sửa lỗi.</returns>
    [HttpPost("{familyId}/fix-relationships")]
    public async Task<IActionResult> FixRelationships([FromRoute] Guid familyId)
    {
        var command = new FixFamilyRelationshipsCommand { FamilyId = familyId };
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 204); // 204 No Content for successful operation with no return value
    }
}
