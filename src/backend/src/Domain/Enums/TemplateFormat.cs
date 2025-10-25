namespace backend.Domain.Enums;

/// <summary>
/// Định nghĩa các định dạng mẫu thông báo khác nhau.
/// </summary>
public enum TemplateFormat
{
    /// <summary>
    /// Định dạng văn bản thuần túy.
    /// </summary>
    PlainText = 0,

    /// <summary>
    /// Định dạng HTML.
    /// </summary>
    Html = 1,

    /// <summary>
    /// Định dạng Markdown.
    /// </summary>
    Markdown = 2
}
