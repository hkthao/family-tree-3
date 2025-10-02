using backend.Domain.Common;
using FluentAssertions;
using Xunit;

namespace backend.Domain.UnitTests.Common;

public class BaseAuditableEntityTests
{
    private class TestAuditableEntity : BaseAuditableEntity
    {
        public string? SomeProperty { get; set; }
    }

    [Fact]
    public void Created_ShouldBeSetOnCreation()
    {
        var entity = new TestAuditableEntity();
        entity.Created.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Created_SetterShouldWork()
    {
        var entity = new TestAuditableEntity();
        var newCreatedDate = DateTime.UtcNow.AddDays(-1);
        entity.Created = newCreatedDate;
        entity.Created.Should().Be(newCreatedDate);
    }

    [Fact]
    public void CreatedBy_ShouldBeSettableAndGettable()
    {
        var entity = new TestAuditableEntity();
        var createdBy = "testuser";
        entity.CreatedBy = createdBy;
        entity.CreatedBy.Should().Be(createdBy);
    }

    [Fact]
    public void LastModified_ShouldBeSettableAndGettable()
    {
        var entity = new TestAuditableEntity();
        var lastModified = DateTime.UtcNow.AddHours(-1);
        entity.LastModified = lastModified;
        entity.LastModified.Should().Be(lastModified);
    }

    [Fact]
    public void LastModifiedBy_ShouldBeSettableAndGettable()
    {
        var entity = new TestAuditableEntity();
        var lastModifiedBy = "anotheruser";
        entity.LastModifiedBy = lastModifiedBy;
        entity.LastModifiedBy.Should().Be(lastModifiedBy);
    }
}
