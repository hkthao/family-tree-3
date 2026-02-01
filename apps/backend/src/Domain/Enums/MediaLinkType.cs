namespace backend.Domain.Enums;

public enum MediaLinkType
{
    /// <summary>
    /// Default or unspecified link type.
    /// </summary>
    Other = 0,

    /// <summary>
    /// Media linked as an avatar or profile picture.
    /// </summary>
    Avatar = 1,

    /// <summary>
    /// Media linked as a gallery image.
    /// </summary>
    GalleryImage = 2,

    /// <summary>
    /// Media linked as a document.
    /// </summary>
    Document = 3,

    /// <summary>
    /// Media linked as a background image.
    /// </summary>
    BackgroundImage = 4
}
