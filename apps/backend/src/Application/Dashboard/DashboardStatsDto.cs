namespace backend.Application.Dashboard;

public class DashboardStatsDto
{
    public int TotalFamilies { get; set; }
    public int TotalMembers { get; set; }
    public int TotalRelationships { get; set; }
    public int TotalEvents { get; set; } // From PublicDashboardDto
    public int TotalGenerations { get; set; } // From PublicDashboardDto

    public double MaleRatio { get; set; }
    public double FemaleRatio { get; set; }
    public int LivingMembersCount { get; set; }
    public int DeceasedMembersCount { get; set; }
    public int AverageAge { get; set; }
    public Dictionary<int, int> MembersPerGeneration { get; set; } = new();
}
