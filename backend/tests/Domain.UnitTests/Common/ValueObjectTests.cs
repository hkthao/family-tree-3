using backend.Domain.Common;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;

namespace backend.Domain.UnitTests.Common;

public class ValueObjectTests
{
    private class TestValueObject : ValueObject
    {
        public string? Property1 { get; set; }
        public int Property2 { get; set; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            if (Property1 != null) yield return Property1;
            yield return Property2;
        }

        // Expose protected static methods for testing
        public static bool TestEqualOperator(ValueObject left, ValueObject right)
        {
            return EqualOperator(left, right);
        }

        public static bool TestNotEqualOperator(ValueObject left, ValueObject right)
        {
            return NotEqualOperator(left, right);
        }
    }

    [Fact]
    public void EqualOperator_ShouldReturnTrueForEqualObjects()
    {
        var vo1 = new TestValueObject { Property1 = "test", Property2 = 1 };
        var vo2 = new TestValueObject { Property1 = "test", Property2 = 1 };

        (vo1 == vo2).Should().BeTrue();
    }

    [Fact]
    public void EqualOperator_ShouldReturnFalseForUnequalObjects()
    {
        var vo1 = new TestValueObject { Property1 = "test", Property2 = 1 };
        var vo2 = new TestValueObject { Property1 = "test", Property2 = 2 };

        (vo1 == vo2).Should().BeFalse();
    }

    [Fact]
    public void EqualOperator_ShouldHandleNullsCorrectly()
    {
        TestValueObject? vo1 = null;
        var vo2 = new TestValueObject { Property1 = "test", Property2 = 1 };

        (vo1 == (TestValueObject?)null).Should().BeTrue();
        ((TestValueObject?)null == vo1).Should().BeTrue();
        (vo1 == vo2).Should().BeFalse();
        (vo2 == vo1).Should().BeFalse();
    }

    [Fact]
    public void NotEqualOperator_ShouldReturnTrueForUnequalObjects()
    {
        var vo1 = new TestValueObject { Property1 = "test", Property2 = 1 };
        var vo2 = new TestValueObject { Property1 = "test", Property2 = 2 };

        (vo1 != vo2).Should().BeTrue();
    }

    [Fact]
    public void NotEqualOperator_ShouldReturnFalseForEqualObjects()
    {
        var vo1 = new TestValueObject { Property1 = "test", Property2 = 1 };
        var vo2 = new TestValueObject { Property1 = "test", Property2 = 1 };

        (vo1 != vo2).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnTrueForEqualObjects()
    {
        var vo1 = new TestValueObject { Property1 = "test", Property2 = 1 };
        var vo2 = new TestValueObject { Property1 = "test", Property2 = 1 };

        vo1.Equals(vo2).Should().BeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnFalseForUnequalObjects()
    {
        var vo1 = new TestValueObject { Property1 = "test", Property2 = 1 };
        var vo2 = new TestValueObject { Property1 = "test", Property2 = 2 };

        vo1.Equals(vo2).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalseForDifferentTypes()
    {
        var vo1 = new TestValueObject { Property1 = "test", Property2 = 1 };
        var obj = new object();

        vo1.Equals(obj).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalseForNull()
    {
        var vo1 = new TestValueObject { Property1 = "test", Property2 = 1 };

        vo1.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_ShouldReturnSameHashCodeForEqualObjects()
    {
        var vo1 = new TestValueObject { Property1 = "test", Property2 = 1 };
        var vo2 = new TestValueObject { Property1 = "test", Property2 = 1 };

        vo1.GetHashCode().Should().Be(vo2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_ShouldReturnDifferentHashCodeForUnequalObjects()
    {
        var vo1 = new TestValueObject { Property1 = "test", Property2 = 1 };
        var vo2 = new TestValueObject { Property1 = "test", Property2 = 2 };

        vo1.GetHashCode().Should().NotBe(vo2.GetHashCode());
    }

    // New tests for protected static operators
    [Fact]
    public void TestEqualOperator_ShouldReturnTrueForEqualObjects()
    {
        var vo1 = new TestValueObject { Property1 = "test", Property2 = 1 };
        var vo2 = new TestValueObject { Property1 = "test", Property2 = 1 };

        TestValueObject.TestEqualOperator(vo1, vo2).Should().BeTrue();
    }

    [Fact]
    public void TestEqualOperator_ShouldReturnFalseForUnequalObjects()
    {
        var vo1 = new TestValueObject { Property1 = "test", Property2 = 1 };
        var vo2 = new TestValueObject { Property1 = "test", Property2 = 2 };

        TestValueObject.TestEqualOperator(vo1, vo2).Should().BeFalse();
    }

    [Fact]
    public void TestEqualOperator_ShouldHandleNullLeft()
    {
        TestValueObject? vo1 = null;
        var vo2 = new TestValueObject { Property1 = "test", Property2 = 1 };

        TestValueObject.TestEqualOperator(vo1!, vo2).Should().BeFalse();
    }

    [Fact]
    public void TestEqualOperator_ShouldHandleNullRight()
    {
        var vo1 = new TestValueObject { Property1 = "test", Property2 = 1 };
        TestValueObject? vo2 = null;

        TestValueObject.TestEqualOperator(vo1, vo2!).Should().BeFalse();
    }

    [Fact]
    public void TestEqualOperator_ShouldHandleBothNull()
    {
        TestValueObject? vo1 = null;
        TestValueObject? vo2 = null;

        TestValueObject.TestEqualOperator(vo1!, vo2!).Should().BeTrue();
    }

    [Fact]
    public void TestNotEqualOperator_ShouldReturnTrueForUnequalObjects()
    {
        var vo1 = new TestValueObject { Property1 = "test", Property2 = 1 };
        var vo2 = new TestValueObject { Property1 = "test", Property2 = 2 };

        TestValueObject.TestNotEqualOperator(vo1, vo2).Should().BeTrue();
    }

    [Fact]
    public void TestNotEqualOperator_ShouldReturnFalseForEqualObjects()
    {
        var vo1 = new TestValueObject { Property1 = "test", Property2 = 1 };
        var vo2 = new TestValueObject { Property1 = "test", Property2 = 1 };

        TestValueObject.TestNotEqualOperator(vo1, vo2).Should().BeFalse();
    }
}
