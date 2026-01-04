namespace backend.Application.Common.Models.ImageRestoration;

public class StartImageRestorationRequestDto
{
    public string ImageUrl { get; set; } = string.Empty;
    public bool UseCodeformer { get; set; } // Added
}
