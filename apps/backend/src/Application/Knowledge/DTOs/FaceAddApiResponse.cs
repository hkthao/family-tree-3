namespace backend.Application.Knowledge.DTOs;

public class FaceAddApiResponse
{
    public string? FaceId { get; set; }
    public string? MemberId { get; set; }
    public string? Message { get; set; }
    public string? VectorDbId { get; set; } // VectorDbId is expected in the response again
}
