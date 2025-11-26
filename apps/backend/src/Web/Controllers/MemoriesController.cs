using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
// using backend.Application.Memories.Commands.AnalyzePhoto; // REMOVED
using backend.Application.Memories.Commands.CreateMemory; // Import CreateMemoryCommand
using backend.Application.Memories.Commands.DeleteMemory; // Import DeleteMemoryCommand
using backend.Application.Memories.Commands.GenerateStory; // Import GenerateStoryCommand
using backend.Application.Memories.Commands.UpdateMemory; // Import UpdateMemoryCommand
using backend.Application.Memories.DTOs;
using backend.Application.Memories.Queries.GetMemoriesByMemberId; // Import GetMemoriesByMemberIdQuery
using backend.Application.Memories.Queries.GetMemoryDetail; // Import GetMemoryDetailQuery
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/memories")]
public class MemoriesController : ControllerBase
{
    // private readonly IPhotoAnalysisService _photoAnalysisService; // Removed
    // private readonly IStoryGenerationService _storyGenerationService; // Removed
    private readonly IMediator _mediator; // Injected IMediator

    public MemoriesController(IMediator mediator)
    {
        _mediator = mediator; // Initialized IMediator
    }

    /// <summary>
    /// Generates a story using AI based on provided context.
    /// </summary>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(GenerateStoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GenerateStoryResponseDto>> GenerateStory([FromBody] GenerateStoryCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }

    /// <summary>
    /// Creates a new memory.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateMemory([FromBody] CreateMemoryCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetMemoryDetail), new { memoryId = result.Value }, result.Value);
        }
        return BadRequest(result.Error);
    }

    /// <summary>
    /// Updates an existing memory.
    /// </summary>
    [HttpPut("{memoryId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateMemory(Guid memoryId, [FromBody] UpdateMemoryCommand command, CancellationToken cancellationToken)
    {
        if (memoryId != command.Id)
        {
            return BadRequest("Memory ID in URL does not match body.");
        }
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes a memory.
    /// </summary>
    [HttpDelete("{memoryId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteMemory(Guid memoryId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteMemoryCommand(memoryId), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Gets a list of memories for a specific member.
    /// </summary>
    [HttpGet("member/{memberId}")]
    [ProducesResponseType(typeof(PaginatedList<MemoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedList<MemoryDto>>> GetMemoriesByMemberId(Guid memberId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetMemoriesByMemberIdQuery(memberId, pageNumber, pageSize);
        var result = await _mediator.Send(query, HttpContext.RequestAborted);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return NotFound(result.Error);
    }

    /// <summary>
    /// Gets detailed information for a specific memory.
    /// </summary>
    [HttpGet("detail/{memoryId}")]
    [ProducesResponseType(typeof(MemoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MemoryDto>> GetMemoryDetail(Guid memoryId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMemoryDetailQuery(memoryId), cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return NotFound(result.Error);
    }
}
