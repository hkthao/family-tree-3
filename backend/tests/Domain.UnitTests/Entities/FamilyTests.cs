using backend.Domain.Entities;
// using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Domain.UnitTests.Entities;

public class FamilyTests
{
    [Fact]
    public void Family_ShouldHaveCorrectDefaultVisibility()
    {
        var family = new Family();
        family.Visibility.Should().Be("Private");
    }

    [Fact]
    public void Family_ShouldAllowSettingAndGettingProperties()
    {
        var family = new Family
        {
            Name = "Test Family",
            Description = "A test description",
            AvatarUrl = "http://example.com/avatar.jpg",
            Visibility = "Public"
        };

        family.Name.Should().Be("Test Family");
        family.Description.Should().Be("A test description");
        family.AvatarUrl.Should().Be("http://example.com/avatar.jpg");
        family.Visibility.Should().Be("Public");
    }
}
