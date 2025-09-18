using backend.Domain.Common;
using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace backend.Domain.UnitTests.Common;

public class ValueObjectTests
{
    private class TestValueObject : ValueObject
    {
        public string Property1 { get; set; }
        public int Property2 { get; set; }

        public TestValueObject(string property1, int property2)
        {
            Property1 = property1;
            Property2 = property2;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Property1;
            yield return Property2;
        }
    }

    [Fact]
    public void EqualOperator_ShouldReturnTrue_WhenBothNull()
    {
        TestValueObject? left = null;
        TestValueObject? right = null;
        (left == right).Should().BeTrue();
    }

    [Fact]
    public void EqualOperator_ShouldReturnFalse_WhenOneNullAndOtherNotNull()
    {
        TestValueObject? left = null;
        TestValueObject right = new TestValueObject("a", 1);
        (left == right).Should().BeFalse();
        (right == left).Should().BeFalse();
    }

    [Fact]
    public void EqualOperator_ShouldReturnTrue_WhenBothEqual()
    {
        var left = new TestValueObject("a", 1);
        var right = new TestValueObject("a", 1);
        (left == right).Should().BeTrue();
    }

    [Fact]
    public void EqualOperator_ShouldReturnFalse_WhenNotEqual()
    {
        var left = new TestValueObject("a", 1);
        var right = new TestValueObject("b", 2);
        (left == right).Should().BeFalse();
    }

    [Fact]
    public void NotEqualOperator_ShouldReturnTrue_WhenNotEqual()
    {
        var left = new TestValueObject("a", 1);
        var right = new TestValueObject("b", 2);
        (left != right).Should().BeTrue();
    }

    [Fact]
    public void NotEqualOperator_ShouldReturnFalse_WhenBothEqual()
    {
        var left = new TestValueObject("a", 1);
        var right = new TestValueObject("a", 1);
        (left != right).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenObjIsNull()
    {
        var valueObject = new TestValueObject("a", 1);
        valueObject.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenObjIsDifferentType()
    {
        var valueObject = new TestValueObject("a", 1);
        valueObject.Equals("some string").Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenObjectsAreEqual()
    {
        var valueObject1 = new TestValueObject("a", 1);
        var valueObject2 = new TestValueObject("a", 1);
        valueObject1.Equals(valueObject2).Should().BeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenObjectsAreNotEqual()
    {
        var valueObject1 = new TestValueObject("a", 1);
        var valueObject2 = new TestValueObject("b", 2);
        valueObject1.Equals(valueObject2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_ShouldReturnSameHashCode_ForEqualObjects()
    {
        var valueObject1 = new TestValueObject("a", 1);
        var valueObject2 = new TestValueObject("a", 1);
        valueObject1.GetHashCode().Should().Be(valueObject2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_ShouldReturnDifferentHashCode_ForUnequalObjects()
    {
        var valueObject1 = new TestValueObject("a", 1);
        var valueObject2 = new TestValueObject("b", 2);
        valueObject1.GetHashCode().Should().NotBe(valueObject2.GetHashCode());
    }
}
