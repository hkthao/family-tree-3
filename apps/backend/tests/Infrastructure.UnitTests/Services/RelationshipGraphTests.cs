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
/// 🎯 Mục tiêu: Kiểm thử hành vi của RelationshipGraph.
/// ⚙️ Các bước: Arrange - Act - Assert.
/// 💡 Giải thích: Đảm bảo đồ thị được xây dựng chính xác và thuật toán BFS tìm đường đi ngắn nhất hoạt động đúng.
/// </summary>
public class RelationshipGraphTests
{
    private readonly RelationshipGraph _relationshipGraph;
    private readonly Guid _familyId = Guid.NewGuid();

    public RelationshipGraphTests()
    {
        _relationshipGraph = new RelationshipGraph();
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra đồ thị xây dựng chính xác với các thành viên và quan hệ, bao gồm quan hệ ngược.
    /// ⚙️ Arrange: Tạo các thành viên và quan hệ cha-con.
    /// ⚙️ Act: Gọi BuildGraph.
    /// ⚙️ Assert: Đảm bảo các cạnh forward và reverse được thêm vào đồ thị.
    /// </summary>
    [Fact]
    public void BuildGraph_ShouldAddForwardAndReverseEdgesCorrectly()
    {
        // Arrange
        var father = new Member("Father", "Test", "F1", _familyId) { Id = Guid.NewGuid(), Gender = Gender.Male };
        var child = new Member("Child", "Test", "C1", _familyId) { Id = Guid.NewGuid(), Gender = Gender.Other };
        var members = new List<Member> { father, child };
        var relationships = new List<Relationship>
        {
            new Relationship(_familyId, father.Id, child.Id, RelationshipType.Father)
        };

        // Act
        _relationshipGraph.BuildGraph(members, relationships);

        // Assert
        // Check father -> child edge
        var fatherEdges = GetAdjacencyList(_relationshipGraph).GetValueOrDefault(father.Id);
        fatherEdges.Should().NotBeNull().And.ContainEquivalentOf(new GraphEdge(father.Id, child.Id, RelationshipType.Father));

        // Check child -> father edge (reverse)
        var childEdges = GetAdjacencyList(_relationshipGraph).GetValueOrDefault(child.Id);
        childEdges.Should().NotBeNull().And.ContainEquivalentOf(new GraphEdge(child.Id, father.Id, RelationshipType.Child));
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra tìm đường đi ngắn nhất cho quan hệ trực tiếp (cha -> con).
    /// ⚙️ Arrange: Tạo đồ thị với quan hệ cha -> con.
    /// ⚙️ Act: Gọi FindShortestPath từ cha đến con.
    /// ⚙️ Assert: Đường đi phải chứa đúng các nút và cạnh.
    /// </summary>
    [Fact]
    public void FindShortestPath_ShouldFindDirectPath_FatherToChild()
    {
        // Arrange
        var father = new Member("Father", "Test", "F1", _familyId) { Id = Guid.NewGuid(), Gender = Gender.Male };
        var child = new Member("Child", "Test", "C1", _familyId) { Id = Guid.NewGuid(), Gender = Gender.Other };
        var members = new List<Member> { father, child };
        var relationships = new List<Relationship>
        {
            new Relationship(_familyId, father.Id, child.Id, RelationshipType.Father)
        };
        _relationshipGraph.BuildGraph(members, relationships);

        // Act
        var path = _relationshipGraph.FindShortestPath(father.Id, child.Id);

        // Assert
        path.NodeIds.Should().BeEquivalentTo(new List<Guid> { father.Id, child.Id }, options => options.WithStrictOrdering());
        path.Edges.Should().BeEquivalentTo(new List<GraphEdge> { new GraphEdge(father.Id, child.Id, RelationshipType.Father) }, options => options.WithStrictOrdering());
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra tìm đường đi ngắn nhất cho quan hệ hai thế hệ (ông -> cháu).
    /// ⚙️ Arrange: Tạo đồ thị với quan hệ ông -> cha -> cháu.
    /// ⚙️ Act: Gọi FindShortestPath từ ông đến cháu.
    /// ⚙️ Assert: Đường đi phải chứa đúng các nút và cạnh.
    /// </summary>
    [Fact]
    public void FindShortestPath_ShouldFindTwoGenerationPath_GrandfatherToGrandchild()
    {
        // Arrange
        var grandfather = new Member("Grandfather", "Test", "GF1", _familyId) { Id = Guid.NewGuid(), Gender = Gender.Male };
        var father = new Member("Father", "Test", "F1", _familyId) { Id = Guid.NewGuid(), Gender = Gender.Male };
        var grandchild = new Member("Grandchild", "Test", "GC1", _familyId) { Id = Guid.NewGuid(), Gender = Gender.Other };
        var members = new List<Member> { grandfather, father, grandchild };
        var relationships = new List<Relationship>
        {
            new Relationship(_familyId, grandfather.Id, father.Id, RelationshipType.Father),
            new Relationship(_familyId, father.Id, grandchild.Id, RelationshipType.Father)
        };
        _relationshipGraph.BuildGraph(members, relationships);

        // Act
        var path = _relationshipGraph.FindShortestPath(grandfather.Id, grandchild.Id);

        // Assert
        path.NodeIds.Should().BeEquivalentTo(new List<Guid> { grandfather.Id, father.Id, grandchild.Id }, options => options.WithStrictOrdering());
        path.Edges.Should().BeEquivalentTo(new List<GraphEdge>
        {
            new GraphEdge(grandfather.Id, father.Id, RelationshipType.Father),
            new GraphEdge(father.Id, grandchild.Id, RelationshipType.Father)
        }, options => options.WithStrictOrdering());
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra tìm đường đi giữa các thành viên không liên quan.
    /// ⚙️ Arrange: Tạo đồ thị với các thành viên không liên quan.
    /// ⚙️ Act: Gọi FindShortestPath giữa hai thành viên không liên quan.
    /// ⚙️ Assert: Đường đi trả về phải rỗng.
    /// </summary>
    [Fact]
    public void FindShortestPath_ShouldReturnEmptyPath_ForUnrelatedMembers()
    {
        // Arrange
        var memberA = new Member("MemberA", "Test", "A1", _familyId) { Id = Guid.NewGuid() };
        var memberB = new Member("MemberB", "Test", "B1", _familyId) { Id = Guid.NewGuid() };
        var memberC = new Member("MemberC", "Test", "C1", _familyId) { Id = Guid.NewGuid() }; // Unrelated
        var members = new List<Member> { memberA, memberB, memberC };
        var relationships = new List<Relationship>
        {
            new Relationship(_familyId, memberA.Id, memberB.Id, RelationshipType.Father)
        };
        _relationshipGraph.BuildGraph(members, relationships);

        // Act
        var path = _relationshipGraph.FindShortestPath(memberA.Id, memberC.Id);

        // Assert
        path.NodeIds.Should().BeEmpty();
        path.Edges.Should().BeEmpty();
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra tìm đường đi đến chính nó.
    /// ⚙️ Arrange: Tạo đồ thị với một thành viên.
    /// ⚙️ Act: Gọi FindShortestPath từ thành viên đến chính nó.
    /// ⚙️ Assert: Đường đi phải chứa chính thành viên đó và không có cạnh.
    /// </summary>
    [Fact]
    public void FindShortestPath_ShouldReturnPathToSelf_WhenStartAndEndAreSame()
    {
        // Arrange
        var member = new Member("Self", "Test", "S1", _familyId) { Id = Guid.NewGuid() };
        var members = new List<Member> { member };
        _relationshipGraph.BuildGraph(members, new List<Relationship>());

        // Act
        var path = _relationshipGraph.FindShortestPath(member.Id, member.Id);

        // Assert
        path.NodeIds.Should().BeEquivalentTo(new List<Guid> { member.Id });
        path.Edges.Should().BeEmpty();
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra GetReverseRelationshipType hoạt động đúng.
    /// ⚙️ Arrange: Các loại quan hệ khác nhau.
    /// ⚙️ Act & Assert: Gọi GetReverseRelationshipType và kiểm tra kết quả.
    /// </summary>
    [Theory]
    [InlineData(RelationshipType.Father, RelationshipType.Child)]
    [InlineData(RelationshipType.Mother, RelationshipType.Child)]
    [InlineData(RelationshipType.Husband, RelationshipType.Wife)]
    [InlineData(RelationshipType.Wife, RelationshipType.Husband)]
    public void GetReverseRelationshipType_ShouldReturnCorrectReverseType(RelationshipType originalType, RelationshipType expectedReverseType)
    {
        // This test uses reflection to access the private method GetReverseRelationshipType
        // For a more robust solution, this method might be made internal or protected internal,
        // or a test helper class could expose it.
        var method = typeof(RelationshipGraph).GetMethod("GetReverseRelationshipType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Should().NotBeNull();
        
        var result = (RelationshipType)method!.Invoke(_relationshipGraph, new object[] { originalType })!;
        result.Should().Be(expectedReverseType);
    }

    // Helper to access private _adjacencyList for assertion in BuildGraph_ShouldAddForwardAndReverseEdgesCorrectly
    private Dictionary<Guid, List<GraphEdge>> GetAdjacencyList(RelationshipGraph graph)
    {
        var field = typeof(RelationshipGraph).GetField("_adjacencyList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (Dictionary<Guid, List<GraphEdge>>)field!.GetValue(graph)!;
    }
}
