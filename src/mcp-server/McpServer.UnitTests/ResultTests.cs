using Xunit;
using McpServer.Common.Models;

namespace McpServer.UnitTests;

/// <summary>
/// Các bài kiểm tra đơn vị cho lớp Result và Result<T>.
/// </summary>
public class ResultTests
{
    /// <summary>
    /// Kiểm tra xem phương thức Success() của Result có trả về kết quả thành công đúng không.
    /// </summary>
    [Fact]
    public void Result_Success_ReturnsSuccessfulResult()
    {
        // Arrange & Act
        var result = Result.Success();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.Null(result.ErrorSource);
    }

    /// <summary>
    /// Kiểm tra xem phương thức Failure() của Result có trả về kết quả thất bại đúng không.
    /// </summary>
    [Fact]
    public void Result_Failure_ReturnsFailedResult()
    {
        // Arrange
        var errorMessage = "Something went wrong.";
        var errorSource = "TestSource";

        // Act
        var result = Result.Failure(errorMessage, errorSource);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(errorMessage, result.Error);
        Assert.Equal(errorSource, result.ErrorSource);
    }

    /// <summary>
    /// Kiểm tra xem phương thức Failure() của Result có trả về kết quả thất bại với nguồn lỗi mặc định đúng không.
    /// </summary>
    [Fact]
    public void Result_Failure_WithDefaultErrorSource_ReturnsFailedResult()
    {
        // Arrange
        var errorMessage = "Another error.";

        // Act
        var result = Result.Failure(errorMessage);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(errorMessage, result.Error);
        Assert.Equal("Unknown", result.ErrorSource);
    }

    /// <summary>
    /// Kiểm tra xem phương thức Success(T value) của Result<T> có trả về kết quả thành công với giá trị đúng không.
    /// </summary>
    [Fact]
    public void ResultT_Success_ReturnsSuccessfulResultWithValue()
    {
        // Arrange
        var expectedValue = "Test Value";

        // Act
        var result = Result<string>.Success(expectedValue);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedValue, result.Value);
        Assert.Null(result.Error);
        Assert.Null(result.ErrorSource);
    }

    /// <summary>
    /// Kiểm tra xem phương thức Failure(string error, string errorSource) của Result<T> có trả về kết quả thất bại đúng không.
    /// </summary>
    [Fact]
    public void ResultT_Failure_ReturnsFailedResult()
    {
        // Arrange
        var errorMessage = "Operation failed.";
        var errorSource = "DataLayer";

        // Act
        var result = Result<int>.Failure(errorMessage, errorSource);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(errorMessage, result.Error);
        Assert.Equal(errorSource, result.ErrorSource);
        Assert.Equal(default(int), result.Value); // Default value for int
    }

    /// <summary>
    /// Kiểm tra xem phương thức Failure(string error) của Result<T> có trả về kết quả thất bại với nguồn lỗi mặc định đúng không.
    /// </summary>
    [Fact]
    public void ResultT_Failure_WithDefaultErrorSource_ReturnsFailedResult()
    {
        // Arrange
        var errorMessage = "Validation error.";

        // Act
        var result = Result<bool>.Failure(errorMessage);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(errorMessage, result.Error);
        Assert.Equal("Unknown", result.ErrorSource);
        Assert.Equal(default(bool), result.Value); // Default value for bool
    }
}
