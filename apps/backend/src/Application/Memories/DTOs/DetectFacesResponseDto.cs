namespace backend.Application.Memories.DTOs;

public class FaceLocationDto
{
    public int Top { get; set; }
    public int Right { get; set; }
    public int Bottom { get; set; }
    public int Left { get; set; }
}

public class DetectFacesResponseDto
{
    public string Filename { get; set; } = string.Empty;
    public List<FaceLocationDto> FaceLocations { get; set; } = new List<FaceLocationDto>();
}
