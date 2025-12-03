namespace backend.Application.AI.DTOs.Embeddings
{
    // Base DTO for all embedding records
    public abstract class BaseEmbeddingsDto
    {
        public string Type { get; set; } = null!;
        public string FamilyId { get; set; } = null!;
        public string RecordId { get; set; } = null!;
        public string Text { get; set; } = null!;
    }

    // 1) MEMBER RECORD
    public class MemberMetadataDto
    {
        public string FullName { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public string BirthDate { get; set; } = null!; // Consider DateTime? if applicable
        public string DeathDate { get; set; } = null!; // Consider DateTime? if applicable
        public string Bio { get; set; } = null!;
        public List<string> Relationships { get; set; } = new();
        public List<string> Events { get; set; } = new();
    }

    public class MemberEmbeddingsDto : BaseEmbeddingsDto
    {
        public MemberMetadataDto Metadata { get; set; } = null!;

        public MemberEmbeddingsDto()
        {
            Type = "member";
        }
    }

    // 2) EVENT RECORD
    public class EventMetadataDto
    {
        public string EventType { get; set; } = null!;
        public string Date { get; set; } = null!; // Consider DateTime? if applicable
        public List<string> MembersInvolved { get; set; } = new();
        public string Location { get; set; } = null!;
    }

    public class EventEmbeddingsDto : BaseEmbeddingsDto
    {
        public EventMetadataDto Metadata { get; set; } = null!;

        public EventEmbeddingsDto()
        {
            Type = "event";
        }
    }

    // 3) STORY RECORD
    public class StoryMetadataDto
    {
        public string Title { get; set; } = null!;
        public string Summary { get; set; } = null!;
        public List<string> Characters { get; set; } = new();
    }

    public class StoryEmbeddingsDto : BaseEmbeddingsDto
    {
        public StoryMetadataDto Metadata { get; set; } = null!;

        public StoryEmbeddingsDto()
        {
            Type = "story";
        }
    }

    // 4) FAMILY OVERVIEW RECORD
    public class FamilyOverviewMetadataDto
    {
        public string FamilyName { get; set; } = null!;
        public string Origin { get; set; } = null!;
        public int MemberCount { get; set; }
        public int TotalGenerations { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public double AverageAge { get; set; }
        public int TotalMales { get; set; }
        public int TotalFemales { get; set; }
        public int LivingMembersCount { get; set; }
        public int DeceasedMembersCount { get; set; }
    }

    public class FamilyEmbeddingsDto : BaseEmbeddingsDto
    {
        public FamilyOverviewMetadataDto Metadata { get; set; } = null!;

        public FamilyEmbeddingsDto()
        {
            Type = "family";
        }
    }
}
