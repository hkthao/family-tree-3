namespace backend.Application.Dashboard.Queries.GetPublicDashboard;

public class PublicDashboardDto
{
    public int TotalPublicFamilies { get; set; }
    public int TotalPublicMembers { get; set; }
    public int TotalPublicRelationships { get; set; }
    public int TotalPublicGenerations { get; set; }
    public double PublicMaleRatio { get; set; }
    public double PublicFemaleRatio { get; set; }
    public int PublicLivingMembersCount { get; set; }
    public int PublicDeceasedMembersCount { get; set; }
    public double PublicAverageAge { get; set; }
    public Dictionary<int, int> PublicMembersPerGeneration { get; set; } = new();
    public int TotalPublicEvents { get; set; }
}
