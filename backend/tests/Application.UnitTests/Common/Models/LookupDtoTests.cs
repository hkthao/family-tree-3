using backend.Application.Common.Models;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Common.Models;

public class LookupDtoTests
{
    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Title";

        // Act
        var dto = new LookupDto { Id = id, Title = title };

        // Assert
        dto.Id.Should().Be(id);
        dto.Title.Should().Be(title);
    }

}
