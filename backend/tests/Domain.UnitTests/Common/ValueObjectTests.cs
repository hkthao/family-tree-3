using backend.Domain.Common;
using FluentAssertions;
using Xunit;

namespace backend.Domain.UnitTests.Common;

public class ValueObjectTests
{
    [Fact]
    public void ShouldBeEqual()
    {
        var vo1 = new TestValueObject(1, "test");
        var vo2 = new TestValueObject(1, "test");

        vo1.Should().Be(vo2);
    }

    [Fact]
    public void ShouldNotBeEqual()
    {
        var vo1 = new TestValueObject(1, "test");
        var vo2 = new TestValueObject(2, "test");

        vo1.Should().NotBe(vo2);
    }

    private class TestValueObject : ValueObject
    {
        public int Prop1 { get; }
        public string Prop2 { get; }

        public TestValueObject(int prop1, string prop2)
        {
            Prop1 = prop1;
            Prop2 = prop2;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Prop1;
            yield return Prop2;
        }
    }
}