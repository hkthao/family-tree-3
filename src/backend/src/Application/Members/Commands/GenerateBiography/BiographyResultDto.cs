namespace backend.Application.Members.Commands.GenerateBiography;

/// <summary>
/// Represents the result of a biography generation request for the API.
/// </summary>
public class BiographyResultDto
{
    public string Content { get; set; } = null!;
}
