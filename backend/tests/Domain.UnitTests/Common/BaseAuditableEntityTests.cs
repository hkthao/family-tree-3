using backend.Domain.Common;
using NUnit.Framework;
using Shouldly;
using System;

namespace backend.Domain.UnitTests.Common;

public class BaseAuditableEntityTests
{
    private class TestAuditableEntity : BaseAuditableEntity
    {
        public string? Name { get; set; }
    }

    [Test]
    public void CreatedBy_ShouldBeNullInitially()
    {
        var entity = new TestAuditableEntity();
        entity.CreatedBy.ShouldBeNull();
    }

    [Test]
    public void LastModifiedBy_ShouldBeNullInitially()
    {
        var entity = new TestAuditableEntity();
        entity.LastModifiedBy.ShouldBeNull();
    }

    [Test]
    public void SetCreatedBy_ShouldSetCreatedByProperty()
    {
        var entity = new TestAuditableEntity();
        var user = "testuser";
        entity.CreatedBy = user;
        entity.CreatedBy.ShouldBe(user);
    }

    [Test]
    public void SetLastModifiedBy_ShouldSetLastModifiedByProperty()
    {
        var entity = new TestAuditableEntity();
        var user = "testuser";
        entity.LastModifiedBy = user;
        entity.LastModifiedBy.ShouldBe(user);
    }

    [Test]
    public void SetLastModified_ShouldSetLastModifiedProperty()
    {
        var entity = new TestAuditableEntity();
        var now = DateTimeOffset.UtcNow;
        entity.LastModified = now;
        entity.LastModified.ShouldBe(now);
    }

    [Test]
    public void Created_ShouldHaveDefaultValueInitially_AndGetterShouldBeHit()
    {
        var entity = new TestAuditableEntity();
        var createdValue = entity.Created; // Explicitly hit the getter
        createdValue.ShouldBe(default(DateTimeOffset));
    }

    [Test]
    public void LastModified_ShouldHaveDefaultValueInitially_AndGetterShouldBeHit()
    {
        var entity = new TestAuditableEntity();
        var lastModifiedValue = entity.LastModified; // Explicitly hit the getter
        lastModifiedValue.ShouldBe(default(DateTimeOffset));
    }
}