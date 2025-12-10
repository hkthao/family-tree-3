using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.MemberStories.Commands.UpdateMemberStory;

public record UpdateMemberStoryCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public Guid MemberId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Story { get; init; } = string.Empty;
    public int? Year { get; init; }
    public string? TimeRangeDescription { get; init; }
    public bool IsYearEstimated { get; init; }
    public LifeStage LifeStage { get; init; }
    public string? Location { get; init; }
    public Guid? StorytellerId { get; init; }
    public CertaintyLevel CertaintyLevel { get; init; }
}
