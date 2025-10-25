namespace backend.Application.Common.Interfaces;

/// <summary>
/// Định nghĩa giao diện cho dịch vụ render mẫu.
/// </summary>
public interface ITemplateRenderer
{
    /// <summary>
    /// Render một mẫu với dữ liệu được cung cấp.
    /// </summary>
    /// <param name="template">Chuỗi mẫu cần render.</param>
    /// <param name="data">Dữ liệu để điền vào các placeholder trong mẫu.</param>
    /// <returns>Chuỗi đã được render.</returns>
    string Render(string template, Dictionary<string, string> data);
}
