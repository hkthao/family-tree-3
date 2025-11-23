namespace backend.Application.Dashboard.Queries;

public class DashboardStatsDto
{
    public int TotalFamilies { get; set; }
    public int TotalMembers { get; set; }
    public int TotalRelationships { get; set; }
    public int TotalGenerations { get; set; }
    public double MaleRatio { get; set; }
    public double FemaleRatio { get; set; }
    public int LivingMembersCount { get; set; }
    public int DeceasedMembersCount { get; set; }
    public double AverageAge { get; set; }
}
