namespace backend.Application.AI.DTOs;

public class RelationshipInferenceResultDto
{
    public string FromAToB { get; set; } = "unknown";
    public string FromBToA { get; set; } = "unknown";
}
