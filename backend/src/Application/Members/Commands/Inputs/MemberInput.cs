namespace backend.Application.Members.Inputs
{
    public record MemberInput
    {
        public string LastName { get; init; } = null!;
        public string FirstName { get; init; } = null!;
        public string? Nickname { get; init; }
        public DateTime? DateOfBirth { get; init; }
        public DateTime? DateOfDeath { get; init; }
        public string? PlaceOfBirth { get; init; }
        public string? PlaceOfDeath { get; init; }
        public string? Gender { get; init; }
        public string? AvatarUrl { get; init; }
        public string? Occupation { get; init; }
        public string? Biography { get; init; }
        public Guid FamilyId { get; init; }
        public bool IsRoot { get; init; }
    }
}
