using backend.Infrastructure.Services;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.ValueObjects;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace backend.Infrastructure.UnitTests.Services;

/// <summary>
/// 🎯 Mục tiêu: Kiểm thử hành vi của RelationshipRuleEngine.
/// ⚙️ Các bước: Arrange - Act - Assert.
/// 💡 Giải thích: Đảm bảo công cụ quy tắc suy luận chính xác các mối quan hệ dựa trên đường đi và điều kiện.
/// </summary>
public class RelationshipRuleEngineTests
{
    private readonly RelationshipRuleEngine _ruleEngine;
    private readonly IReadOnlyDictionary<Guid, Member> _allMembers;

    public RelationshipRuleEngineTests()
    {
        _ruleEngine = new RelationshipRuleEngine();

        // Setup dummy members for testing conditions
        var familyId = Guid.NewGuid();
        var membersList = new List<Member>
        {
            new Member("A", "Male", "A1", familyId) { Id = Guid.NewGuid(), Gender = Gender.Male },
            new Member("B", "Female", "B1", familyId) { Id = Guid.NewGuid(), Gender = Gender.Female },
            new Member("C", "Male", "C1", familyId) { Id = Guid.NewGuid(), Gender = Gender.Male },
            new Member("D", "Female", "D1", familyId) { Id = Guid.NewGuid(), Gender = Gender.Female },
            new Member("E", "Other", "E1", familyId) { Id = Guid.NewGuid(), Gender = Gender.Other }
        };
        _allMembers = membersList.ToDictionary(m => m.Id);
    }

    private RelationshipPath CreatePath(List<RelationshipType> types, List<Guid> nodeIds)
    {
        var edges = new List<GraphEdge>();
        for (int i = 0; i < types.Count; i++)
        {
            edges.Add(new GraphEdge(nodeIds[i], nodeIds[i + 1], types[i]));
        }
        return new RelationshipPath(nodeIds, edges);
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra quy tắc quan hệ "cha".
    /// </summary>
    [Fact]
    public void InferRelationship_ShouldReturnCha_ForFatherPath()
    {
        // Arrange
        var father = _allMembers.Values.First(m => m.Gender == Gender.Male);
        var child = _allMembers.Values.First(m => m.Gender == Gender.Other);
        var path = CreatePath(new List<RelationshipType> { RelationshipType.Father }, new List<Guid> { father.Id, child.Id });

        // Act
        var result = _ruleEngine.InferRelationship(path, _allMembers);

        // Assert
        result.Should().Be("cha");
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra quy tắc quan hệ "mẹ".
    /// </summary>
    [Fact]
    public void InferRelationship_ShouldReturnMe_ForMotherPath()
    {
        // Arrange
        var mother = _allMembers.Values.First(m => m.Gender == Gender.Female);
        var child = _allMembers.Values.First(m => m.Gender == Gender.Other);
        var path = CreatePath(new List<RelationshipType> { RelationshipType.Mother }, new List<Guid> { mother.Id, child.Id });

        // Act
        var result = _ruleEngine.InferRelationship(path, _allMembers);

        // Assert
        result.Should().Be("mẹ");
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra quy tắc quan hệ "ông nội".
    /// </summary>
    [Fact]
    public void InferRelationship_ShouldReturnOngNoi()
    {
        // Arrange: Grandfather (Male) -> Father (Male) -> Child (Other)
        var grandfather = _allMembers.Values.First(m => m.Id == _allMembers.Keys.First()); // A - Male
        var father = _allMembers.Values.First(m => m.Id == _allMembers.Keys.Skip(2).First()); // C - Male
        var child = _allMembers.Values.First(m => m.Id == _allMembers.Keys.Last()); // E - Other
        
        // Path from child to grandfather: child (Child) -> father (Child) -> grandfather
        var path = CreatePath(
            new List<RelationshipType> { RelationshipType.Child, RelationshipType.Child },
            new List<Guid> { child.Id, father.Id, grandfather.Id }
        );

        // Act
        var result = _ruleEngine.InferRelationship(path, _allMembers);

        // Assert
        result.Should().Be("ông nội");
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra quy tắc quan hệ "bà ngoại".
    /// </summary>
    [Fact]
    public void InferRelationship_ShouldReturnBaNgoai()
    {
        // Arrange: Grandmother (Female) -> Mother (Female) -> Child (Other)
        var grandmother = _allMembers.Values.First(m => m.Id == _allMembers.Keys.Skip(1).First()); // B - Female
        var mother = _allMembers.Values.First(m => m.Id == _allMembers.Keys.Skip(3).First()); // D - Female
        var child = _allMembers.Values.First(m => m.Id == _allMembers.Keys.Last()); // E - Other

        // Path from child to grandmother: child (Child) -> mother (Child) -> grandmother
        var path = CreatePath(
            new List<RelationshipType> { RelationshipType.Child, RelationshipType.Child },
            new List<Guid> { child.Id, mother.Id, grandmother.Id }
        );

        // Act
        var result = _ruleEngine.InferRelationship(path, _allMembers);

        // Assert
        result.Should().Be("bà ngoại");
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra quy tắc quan hệ "cháu nội".
    /// </summary>
    [Fact]
    public void InferRelationship_ShouldReturnChauNoi()
    {
        // Arrange: Grandfather (Male) -> Father (Male) -> Grandchild (Other)
        var grandfather = _allMembers.Values.First(m => m.Id == _allMembers.Keys.First()); // A - Male
        var father = _allMembers.Values.First(m => m.Id == _allMembers.Keys.Skip(2).First()); // C - Male
        var grandchild = _allMembers.Values.First(m => m.Id == _allMembers.Keys.Last()); // E - Other
        
        // Path from grandfather to grandchild: grandfather (Father) -> father (Child) -> grandchild
        var path = CreatePath(
            new List<RelationshipType> { RelationshipType.Father, RelationshipType.Child },
            new List<Guid> { grandfather.Id, father.Id, grandchild.Id }
        );

        // Act
        var result = _ruleEngine.InferRelationship(path, _allMembers);

        // Assert
        result.Should().Be("cháu nội");
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra quy tắc quan hệ "anh/chị/em (cùng cha)".
    /// </summary>
    [Fact]
    public void InferRelationship_ShouldReturnAnhChiEmCungCha()
    {
        // Arrange: MemberA (Other) -> Father (Male) -> MemberB (Other)
        var memberA = _allMembers.Values.First(m => m.Id == _allMembers.Keys.Last()); // E - Other
        var commonFather = _allMembers.Values.First(m => m.Id == _allMembers.Keys.First()); // A - Male
        var memberB = _allMembers.Values.First(m => m.Id == _allMembers.Keys.Skip(1).First()); // B - Female

        // Path from memberA to memberB: memberA (Child) -> commonFather (Father) -> memberB
        var path = CreatePath(
            new List<RelationshipType> { RelationshipType.Child, RelationshipType.Father },
            new List<Guid> { memberA.Id, commonFather.Id, memberB.Id }
        );

        // Act
        var result = _ruleEngine.InferRelationship(path, _allMembers);

        // Assert
        result.Should().Be("anh/chị/em (cùng cha)");
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra quy tắc quan hệ "cô (bên nội)".
    /// </summary>
    [Fact]
    public void InferRelationship_ShouldReturnCoBenNoi()
    {
        // Arrange: Niece/Nephew (Other) -> Father (Male) -> Grandfather (Male) -> Aunt (Female)
        var child = _allMembers.Values.First(m => m.Id == _allMembers.Keys.Last()); // E - Other
        var father = _allMembers.Values.First(m => m.Id == _allMembers.Keys.First()); // A - Male
        var grandfather = _allMembers.Values.First(m => m.Id == _allMembers.Keys.Skip(2).First()); // C - Male
        var aunt = _allMembers.Values.First(m => m.Id == _allMembers.Keys.Skip(3).First()); // D - Female

        // Path from child to aunt: child (Child) -> father (Child) -> grandfather (Mother) -> aunt
        var path = CreatePath(
            new List<RelationshipType> { RelationshipType.Child, RelationshipType.Child, RelationshipType.Mother },
            new List<Guid> { child.Id, father.Id, grandfather.Id, aunt.Id }
        );

        // Act
        var result = _ruleEngine.InferRelationship(path, _allMembers);

        // Assert
        result.Should().Be("cô (bên nội)");
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra trường hợp không tìm thấy quy tắc khớp.
    /// </summary>
    [Fact]
    public void InferRelationship_ShouldReturnUnknown_ForNoMatchingRule()
    {
        // Arrange: A path that does not match any defined rule
        var path = CreatePath(
            new List<RelationshipType> { RelationshipType.Father, RelationshipType.Wife, RelationshipType.Husband },
            new List<Guid> { _allMembers.Keys.First(), _allMembers.Keys.Skip(1).First(), _allMembers.Keys.Skip(2).First(), _allMembers.Keys.Skip(3).First() }
        );

        // Act
        var result = _ruleEngine.InferRelationship(path, _allMembers);

        // Assert
        result.Should().Be("unknown");
    }
}
