using System.IO;
using System.Text;
using backend.Infrastructure.AI.TextExtractors;
using FluentAssertions;
using Xunit;

namespace backend.Infrastructure.IntegrationTests.AI.TextExtractors;

public class MdTextExtractorTests
{
    private readonly MdTextExtractor _extractor;

    public MdTextExtractorTests()
    {
        _extractor = new MdTextExtractor();
    }

    /// <summary>
    /// Mục tiêu của test: Đảm bảo rằng ExtractTextAsync trích xuất chính xác văn bản thuần túy từ nội dung Markdown, loại bỏ định dạng.
    /// </summary>
    /// <remarks>
    /// ⚙️ Các bước:
    /// 1. Arrange: Tạo một MemoryStream từ một chuỗi Markdown mẫu.
    /// 2. Act: Gọi phương thức ExtractTextAsync với MemoryStream đã tạo.
    /// 3. Assert: Kiểm tra xem văn bản được trích xuất có khớp với văn bản thuần túy mong đợi không.
    /// </remarks>
    /// <explanation>
    /// 💡 Giải thích: Test này xác nhận rằng MdTextExtractor có thể loại bỏ các yếu tố định dạng Markdown
    /// như tiêu đề, in đậm, in nghiêng, liên kết, hình ảnh, khối trích dẫn và danh sách,
    /// chỉ để lại văn bản thuần túy, điều này rất quan trọng cho việc xử lý AI.
    /// </explanation>
    [Fact]
    public async Task ExtractTextAsync_ShouldExtractPlainTextFromMarkdown()
    {
        // Arrange
        var markdownContent = """
# Title 1

## Subtitle 2

This is **bold** and *italic* text.

[Link Text](http://example.com)

![Alt Text](http://example.com/image.png)

> A blockquote.

- List item 1
- List item 2

```csharp
Console.WriteLine(\"Hello\");
```

Inline `code` here.
""";
        var expectedPlainText = "This is bold and italic text.\nLink Text\nAlt Text\nList item 1\nList item 2\nInline code here.";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(markdownContent));

        // Act
        var extractedText = await _extractor.ExtractTextAsync(stream);

        // Assert
        extractedText.Should().Be(expectedPlainText);
    }

    /// <summary>
    /// Mục tiêu của test: Đảm bảo rằng ExtractTextAsync xử lý một Stream trống một cách chính xác.
    /// </summary>
    /// <remarks>
    /// ⚙️ Các bước:
    /// 1. Arrange: Tạo một MemoryStream trống.
    /// 2. Act: Gọi phương thức ExtractTextAsync với MemoryStream trống.
    /// 3. Assert: Kiểm tra xem văn bản được trích xuất có phải là một chuỗi trống không.
    /// </remarks>
    /// <explanation>
    /// 💡 Giải thích: Test này đảm bảo rằng trình trích xuất có thể xử lý các tệp trống
    /// mà không gây ra lỗi và trả về một kết quả hợp lệ (chuỗi trống).
    /// </explanation>
    [Fact]
    public async Task ExtractTextAsync_ShouldReturnEmptyString_FromEmptyStream()
    {
        // Arrange
        using var stream = new MemoryStream();

        // Act
        var extractedText = await _extractor.ExtractTextAsync(stream);

        // Assert
        extractedText.Should().BeEmpty();
    }
}
