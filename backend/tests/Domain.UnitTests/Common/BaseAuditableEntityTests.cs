using Xunit;
using FluentAssertions;
using System;
using backend.Domain.Common;

namespace backend.Domain.UnitTests.Common;

public class BaseAuditableEntityTests
{
    [Fact]
    public void CreatedBy_ShouldBeNullInitially()
    {
        var entity = new TestAuditableEntity();
        entity.CreatedBy.Should().BeNull();
    }

    [Fact]
    public void LastModifiedBy_ShouldBeNullInitially()
    {
        var entity = new TestAuditableEntity();
        entity.LastModifiedBy.Should().BeNull();
    }

    [Fact]
    public void SetCreatedBy_ShouldSetCreatedByProperty()
    {
        var entity = new TestAuditableEntity();
        var userId = "testUser";
        entity.SetCreatedBy(userId);
        entity.CreatedBy.Should().Be(userId);
    }

    [Fact]
    public void SetLastModifiedBy_ShouldSetLastModifiedByProperty()
    {
        var entity = new TestAuditableEntity();
        var userId = "testUser";
        entity.SetLastModifiedBy(userId);
        entity.LastModifiedBy.Should().Be(userId);
    }

    [Fact]
    public void SetLastModified_ShouldSetLastModifiedProperty()
    {
        var entity = new TestAuditableEntity();
        var date = DateTime.UtcNow;
        entity.SetLastModified(date);
        entity.LastModified.Should().Be(date);
    }

    [Fact]
    public void Created_ShouldHaveDefaultValueInitially_AndGetterShouldBeHit()
    {
        var entity = new TestAuditableEntity();
        entity.Created.Should().NotBe(default(DateTime));
    }

    [Fact]
    public void LastModified_ShouldHaveDefaultValueInitially_AndGetterShouldBeHit()
    {
        var entity = new TestAuditableEntity();
        entity.LastModified.Should().NotBe(default(DateTime));
    }

    // Helper class for testing
    private class TestAuditableEntity : BaseAuditableEntity
    {
        public void SetCreatedBy(string userId)
        {
            CreatedBy = userId;
        }

        public void SetLastModifiedBy(string userId)
        {
            LastModifiedBy = userId;
        }

        public void SetLastModified(DateTime date)
        {
            LastModified = date;
        }
    }
}
