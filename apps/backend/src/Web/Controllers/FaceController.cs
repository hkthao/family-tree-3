using backend.Application.Common.Models;
using backend.Application.Faces.Commands.DeleteFacesByMemberId;
using backend.Application.Faces.Commands.DetectFaces;
using backend.Application.Faces.Commands.SaveFaceLabels;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;
/// <summary>
/// Bộ điều khiển xử lý các yêu cầu liên quan đến nhận diện và quản lý khuôn mặt.
/// </summary>
/// <param name="mediator">Đối tượng IMediator để gửi các lệnh và truy vấn.</param>
[ApiController]
[Route("api/face")]
public class FaceController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Đối tượng IMediator để gửi các lệnh và truy vấn.
    /// </summary>
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Xử lý POST request để phát hiện khuôn mặt trong một hình ảnh.
    /// </summary>
    /// <param name="file">Tệp hình ảnh cần xử lý.</param>
    /// <param name="returnCrop">Có trả về ảnh cắt của khuôn mặt đã phát hiện hay không.</param>
    /// <returns>Đối tượng chứa thông tin về các khuôn mặt đã phát hiện.</returns>
    [HttpPost("detect")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<FaceDetectionResponseDto>> DetectFaces([FromForm] IFormFile file, [FromQuery] bool resizeImageForAnalysis = false, [FromQuery] bool returnCrop = true)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No image file uploaded.");
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var imageBytes = memoryStream.ToArray();

        var command = new DetectFacesCommand
        {
            ImageBytes = imageBytes,
            ContentType = file.ContentType,
            ReturnCrop = returnCrop,
            ResizeImageForAnalysis =resizeImageForAnalysis
        };

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý POST request để lưu nhãn cho các khuôn mặt đã phát hiện.
    /// </summary>
    /// <param name="command">Lệnh lưu nhãn khuôn mặt.</param>
    /// <returns>Kết quả của thao tác lưu.</returns>
    [HttpPost("labels")]
    public async Task<ActionResult<Result<Unit>>> SaveFaceLabels([FromBody] SaveFaceLabelsCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<Result<Unit>>)Ok(result) : (ActionResult<Result<Unit>>)BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý DELETE request để xóa tất cả dữ liệu khuôn mặt cho một thành viên.
    /// </summary>
    /// <param name="memberId">ID của thành viên.</param>
    /// <returns>Kết quả của thao tác xóa.</returns>
    [HttpDelete("member/{memberId}")]
    public async Task<ActionResult<Result<Unit>>> DeleteFacesByMemberId(Guid memberId)
    {
        var command = new DeleteFacesByMemberIdCommand(memberId);
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<Result<Unit>>)Ok(result) : (ActionResult<Result<Unit>>)BadRequest(result.Error);
    }
}
