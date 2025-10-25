using System.Collections.Generic;
using System.Text.RegularExpressions;
using backend.Application.Common.Interfaces;

namespace backend.Infrastructure.Services;

/// <summary>
/// Triển khai ITemplateRenderer sử dụng Regex để thay thế placeholder.
/// (Trong một ứng dụng thực tế, có thể sử dụng thư viện như Fluid.Core hoặc RazorEngine để xử lý template phức tạp hơn).
/// </summary>
public class FluidTemplateRenderer : ITemplateRenderer
{
    /// <summary>
    /// Render một mẫu với dữ liệu được cung cấp.
    /// Placeholder có định dạng {{Key}}.
    /// </summary>
    /// <param name="template">Chuỗi mẫu cần render.</param>
    /// <param name="data">Dữ liệu để điền vào các placeholder trong mẫu.</param>
    /// <returns>Chuỗi đã được render.</returns>
    public string Render(string template, Dictionary<string, string> data)
    {
        return Regex.Replace(template, @"\{\{(.*?)\}\}", match =>
        {
            var key = match.Groups[1].Value;
            return data.TryGetValue(key, out var value) ? value : match.Value; // Giữ nguyên placeholder nếu không tìm thấy key
        });
    }
}
