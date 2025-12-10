// Removed using backend.Domain.Common.Interfaces; assuming IAggregateRoot is in Common
// Removed using backend.Domain.ValueObjects; as it's not used here

using System.Collections.Generic;
using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class MemberStory : BaseAuditableEntity, ISoftDelete
{
    // Id is inherited from BaseAuditableEntity
    public Guid MemberId { get; set; }
    public string Title { get; set; } = string.Empty; // max 120
    public string Story { get; set; } = string.Empty; // long text

    public int? Year { get; set; }
    public string? TimeRangeDescription { get; set; }
    public bool IsYearEstimated { get; set; }

    public LifeStage LifeStage { get; set; }
    public string? Location { get; set; }

    public Guid? StorytellerId { get; set; }
    public CertaintyLevel CertaintyLevel { get; set; }

    // Navigation properties
    public Member Member { get; set; } = default!;
    public ICollection<MemberStoryImage> MemberStoryImages { get; set; } = new List<MemberStoryImage>();

    public void Update(string title, string story, int? year, string? timeRangeDescription, bool isYearEstimated, LifeStage lifeStage, string? location, Guid? storytellerId, CertaintyLevel certaintyLevel)
    {
        Title = title;
        Story = story;
        Year = year;
        TimeRangeDescription = timeRangeDescription;
        IsYearEstimated = isYearEstimated;
        LifeStage = lifeStage;
        Location = location;
        StorytellerId = storytellerId;
        CertaintyLevel = certaintyLevel;
    }
}


