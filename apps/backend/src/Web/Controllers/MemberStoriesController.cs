using backend.Application.Common.Models;
using backend.Application.MemberStories.Commands.CreateMemberStory; // Updated
using backend.Application.MemberStories.Commands.DeleteMemberStory; // Updated
using backend.Application.MemberStories.Commands.GenerateStory; // Updated
using backend.Application.MemberStories.Commands.UpdateMemberStory; // Updated
using backend.Application.MemberStories.DTOs; // Updated
using backend.Application.MemberStories.Queries.GetMemberStoryDetail;
using backend.Application.MemberStories.Queries.SearchStories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/member-stories")] // Updated
public class MemberStoriesController : ControllerBase // Updated
{
    // private readonly IPhotoAnalysisService _photoAnalysisService; // Removed
    // private readonly IStoryGenerationService _storyGenerationService; // Removed
    private readonly IMediator _mediator; // Injected IMediator

    public MemberStoriesController(IMediator mediator) // Updated
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
    /// Creates a new member story.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateMemberStory([FromBody] CreateMemberStoryCommand command, CancellationToken cancellationToken) // Updated
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetMemberStoryDetail), new { memberStoryId = result.Value }, result.Value); // Updated
        }
        return BadRequest(result.Error);
    }

    /// <summary>
    /// Updates an existing member story.
    /// </summary>
    [HttpPut("{memberStoryId}")] // Updated
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateMemberStory(Guid memberStoryId, [FromBody] UpdateMemberStoryCommand command, CancellationToken cancellationToken) // Updated
    {
        if (memberStoryId != command.Id)
        {
            return BadRequest("MemberStory ID in URL does not match body."); // Updated
        }
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes a member story.
    /// </summary>
    [HttpDelete("{memberStoryId}")] // Updated
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteMemberStory(Guid memberStoryId, CancellationToken cancellationToken) // Updated
    {
        await _mediator.Send(new DeleteMemberStoryCommand(memberStoryId), cancellationToken); // Updated
        return NoContent();
    }

    /// <summary>
    /// Gets a paginated list of member stories based on various filters.
    /// </summary>
    [HttpGet("search")] // Route sẽ là api/member-stories/search
    [ProducesResponseType(typeof(PaginatedList<MemberStoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedList<MemberStoryDto>>> SearchMemberStories(
        [FromQuery] SearchStoriesQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }

    /// <summary>
    /// Gets detailed information for a specific member story.
    /// </summary>
    [HttpGet("detail/{memberStoryId}")] // Updated
    [ProducesResponseType(typeof(MemberStoryDto), StatusCodes.Status200OK)] // Updated
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MemberStoryDto>> GetMemberStoryDetail(Guid memberStoryId, CancellationToken cancellationToken) // Updated
    {
        var result = await _mediator.Send(new GetMemberStoryDetailQuery(memberStoryId), cancellationToken); // Updated
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return NotFound(result.Error);
    }
}
