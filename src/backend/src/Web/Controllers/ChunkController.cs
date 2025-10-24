using backend.Application.AI.Chunk.EmbedChunks;
using backend.Application.AI.Chunk.ProcessFile;
using backend.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers
{
    /// <summary>
    /// Bộ điều khiển xử lý các yêu cầu liên quan đến việc quản lý các đoạn văn bản (chunks) cho AI.
    /// </summary>
    /// <param name="mediator">Đối tượng IMediator để gửi các lệnh và truy vấn.</param>
    [ApiController]
    [Route("api/[controller]")]
    public class ChunkController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Đối tượng IMediator để gửi các lệnh và truy vấn.
        /// </summary>
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Xử lý POST request để tải lên một tệp và xử lý thành các đoạn văn bản.
        /// </summary>
        /// <param name="file">Tệp cần tải lên.</param>
        /// <param name="fileId">ID của tệp.</param>
        /// <param name="familyId">ID của gia đình liên quan.</param>
        /// <param name="category">Danh mục của tệp.</param>
        /// <param name="createdBy">Người tạo tệp.</param>
        /// <returns>Danh sách các đoạn văn bản đã được xử lý.</returns>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(List<TextChunk>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<TextChunk>>> Upload(
            IFormFile file,
            [FromForm] string fileId,
            [FromForm] string familyId,
            [FromForm] string category,
            [FromForm] string createdBy)
        {
            using var stream = file.OpenReadStream();
            var command = new ProcessFileCommand
            {
                FileStream = stream,
                FileName = file.FileName,
                FileId = fileId,
                FamilyId = familyId,
                Category = category,
                CreatedBy = createdBy
            }; var result = await _mediator.Send(command);

            return result.IsSuccess ? (ActionResult<List<TextChunk>>)Ok(result.Value) : (ActionResult<List<TextChunk>>)BadRequest(result.Error);
        }

        /// <summary>
        /// Xử lý POST request để chấp thuận và nhúng các đoạn văn bản đã xử lý.
        /// </summary>
        /// <param name="chunks">Danh sách các đoạn văn bản cần chấp thuận.</param>
        /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
        [HttpPost("approve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ApproveChunks([FromBody] List<TextChunk> chunks)
        {
            var command = new EmbedChunksCommand
            {
                Chunks = chunks
            };

            var result = await _mediator.Send(command);

            return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
        }
    }
}
