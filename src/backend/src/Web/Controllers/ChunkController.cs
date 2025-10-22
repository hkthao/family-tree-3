using backend.Application.AI.Chunk.EmbedChunks;
using backend.Application.AI.Chunk.ProcessFile;
using backend.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChunkController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

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
