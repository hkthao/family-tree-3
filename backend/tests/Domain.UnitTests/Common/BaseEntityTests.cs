using backend.Domain.Common;
using Xunit;
using FluentAssertions;
using MongoDB.Bson;

namespace backend.Domain.UnitTests.Common;

public class BaseEntityTests
{
    private class TestEvent : BaseEvent { }

    private class TestEntity : BaseEntity
    {
        public string? Name { get; set; }
    }

    [Fact]
    public void AddDomainEvent_ShouldAddEventToList()
    {
        var entity = new TestEntity();
        var domainEvent = new TestEvent();

        entity.AddDomainEvent(domainEvent);

        entity.DomainEvents.Should().Contain(domainEvent);
    }

    [Fact]
    public void RemoveDomainEvent_ShouldRemoveEventFromList()
    {
        var entity = new TestEntity();
        var domainEvent = new TestEvent();
        entity.AddDomainEvent(domainEvent);

        entity.RemoveDomainEvent(domainEvent);

        entity.DomainEvents.Should().NotContain(domainEvent);
    }

    [Fact]
    public void ClearDomainEvents_ShouldClearAllEvents()
    {
        var entity = new TestEntity();
        entity.AddDomainEvent(new TestEvent());
        entity.AddDomainEvent(new TestEvent());

        entity.ClearDomainEvents();

        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Id_ShouldBeSetOnCreation()
    {
        var entity = new TestEntity();
        entity.Id.Should().NotBe(ObjectId.Empty.ToString());
    }
}
