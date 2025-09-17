using backend.Domain.Common;
using NUnit.Framework;
using Shouldly;

namespace backend.Domain.UnitTests.Common;

public class BaseEntityTests
{
    private class TestEvent : BaseEvent { }

    private class TestEntity : BaseEntity
    {
        public string? Name { get; set; }
    }

    [Test]
    public void AddDomainEvent_ShouldAddEventToList()
    {
        var entity = new TestEntity();
        var domainEvent = new TestEvent();

        entity.AddDomainEvent(domainEvent);

        entity.DomainEvents.ShouldContain(domainEvent);
    }

    [Test]
    public void RemoveDomainEvent_ShouldRemoveEventFromList()
    {
        var entity = new TestEntity();
        var domainEvent = new TestEvent();
        entity.AddDomainEvent(domainEvent);

        entity.RemoveDomainEvent(domainEvent);

        entity.DomainEvents.ShouldNotContain(domainEvent);
    }

    [Test]
    public void ClearDomainEvents_ShouldClearAllEvents()
    {
        var entity = new TestEntity();
        entity.AddDomainEvent(new TestEvent());
        entity.AddDomainEvent(new TestEvent());

        entity.ClearDomainEvents();

        entity.DomainEvents.ShouldBeEmpty();
    }

    [Test]
    public void Id_ShouldBeSetOnCreation()
    {
        var entity = new TestEntity();
        entity.Id.ToString().ShouldNotBe(MongoDB.Bson.ObjectId.Empty.ToString());
    }
}