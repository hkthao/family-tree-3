using backend.Application.Files.Commands.ProcessFile;
using backend.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChunkController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ChunkController(IMediator mediator)
        {
            _mediator = mediator;
        }

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
                            using (var stream = file.OpenReadStream())
                            {
                                var command = new ProcessFileCommand
                                {
                                    FileStream = stream,
                                    FileName = file.FileName,
                                    FileId = fileId,
                                    FamilyId = familyId,
                                    Category = category,
                                    CreatedBy = createdBy
                                };                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    return Ok(result.Value);
                }
                else
                {
                    return BadRequest(result.Error);
                }
            }
        }
    }
}
