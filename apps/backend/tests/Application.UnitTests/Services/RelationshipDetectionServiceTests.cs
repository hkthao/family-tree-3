using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using backend.Application.AI; // Added
using backend.Application.AI.DTOs; // Added
using backend.Application.Common.Models; // Added
using backend.Application.Common.Interfaces; // Added
using backend.Application.Services;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Interfaces;
using backend.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Services;

/// <summary>
/// 🎯 Mục tiêu: Kiểm thử hành vi của RelationshipDetectionService.
/// ⚙️ Các bước: Arrange - Act - Assert.
/// 💡 Giải thích: Đảm bảo service có thể phát hiện và suy luận các mối quan hệ một cách chính xác dựa trên đồ thị và AI.
/// </summary>
public class RelationshipDetectionServiceTests : TestBase
{
    private readonly RelationshipDetectionService _service;
    private readonly Mock<IRelationshipGraph> _mockRelationshipGraph;
    private readonly Mock<IAiGenerateService> _mockAiGenerateService; // Changed from IRelationshipRuleEngine

    public RelationshipDetectionServiceTests()
    {
        _mockRelationshipGraph = new Mock<IRelationshipGraph>();
        _mockAiGenerateService = new Mock<IAiGenerateService>(); // Initialized
        _service = new RelationshipDetectionService(_context, _mockRelationshipGraph.Object, _mockAiGenerateService.Object); // Updated constructor
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra phát hiện quan hệ cha-con trực tiếp.
    /// ⚙️ Arrange: Thiết lập dữ liệu thành viên, quan hệ và các mock cho graph/rule engine.
    /// ⚙️ Act: Gọi DetectRelationshipAsync.
    /// ⚙️ Assert: Kết quả trả về phải là "cha" và "con" đúng như kỳ vọng.
    /// </summary>
    [Fact]
    public async Task DetectRelationshipAsync_ShouldReturnFatherChild_ForDirectRelation()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var father = new Member("Father", "Test", "F1", familyId, isDeceased: false) { Id = Guid.NewGuid() };
        father.UpdateGender(Gender.Male.ToString());
        var child = new Member("Child", "Test", "C1", familyId, isDeceased: false) { Id = Guid.NewGuid() };
        child.UpdateGender(Gender.Male.ToString());

        _context.Members.Add(father);
        _context.Members.Add(child);
        _context.Relationships.Add(new Relationship(familyId, father.Id, child.Id, RelationshipType.Father) { Id = Guid.NewGuid() });
        await _context.SaveChangesAsync();

        var members = _context.Members.ToList();
        var relationships = _context.Relationships.ToList();

        // Mock graph behavior
        _mockRelationshipGraph.Setup(g => g.BuildGraph(It.IsAny<IEnumerable<Member>>(), It.IsAny<IEnumerable<Relationship>>()))
            .Callback<IEnumerable<Member>, IEnumerable<Relationship>>((m, r) => { /* Simulate graph built */ });

        var pathToChild = new RelationshipPath(new List<Guid> { father.Id, child.Id }, new List<GraphEdge> { new GraphEdge(father.Id, child.Id, RelationshipType.Father) });
        var pathToFather = new RelationshipPath(new List<Guid> { child.Id, father.Id }, new List<GraphEdge> { new GraphEdge(child.Id, father.Id, RelationshipType.Child) });

        _mockRelationshipGraph.Setup(g => g.FindShortestPath(father.Id, child.Id)).Returns(pathToChild);
        _mockRelationshipGraph.Setup(g => g.FindShortestPath(child.Id, father.Id)).Returns(pathToFather);

        // Mock AI service behavior
        _mockAiGenerateService.Setup(s => s.GenerateDataAsync<RelationshipInferenceResultDto>(
            It.IsAny<GenerateRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RelationshipInferenceResultDto>.Success(new RelationshipInferenceResultDto { FromAToB = "cha", FromBToA = "con" }));


        // Act
        var result = await _service.DetectRelationshipAsync(familyId, father.Id, child.Id, CancellationToken.None); // Added CancellationToken.None

        // Assert
        result.Should().NotBeNull();
        result.FromAToB.Should().Be("cha");
        result.FromBToA.Should().Be("con");
        result.Path.Should().HaveCount(2);
        result.Edges.Should().HaveCount(1);
        result.Path.First().Should().Be(father.Id);
        result.Path.Last().Should().Be(child.Id);
        result.Edges.First().Should().Be(nameof(RelationshipType.Father));
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra phát hiện quan hệ ông nội-cháu nội.
    /// ⚙️ Arrange: Thiết lập dữ liệu thành viên, quan hệ và các mock cho graph/rule engine.
    /// ⚙️ Act: Gọi DetectRelationshipAsync.
    /// ⚙️ Assert: Kết quả trả về phải là "ông nội" và "cháu nội" đúng như kỳ vọng.
    /// </summary>
    [Fact]
    public async Task DetectRelationshipAsync_ShouldReturnGrandfatherGrandchild_ForTwoGenerationRelation()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var grandfather = new Member("Grandfather", "Test", "GF1", familyId, isDeceased: false) { Id = Guid.NewGuid() };
        grandfather.UpdateGender(Gender.Male.ToString());

        var father = new Member("Father", "Test", "F1", familyId, isDeceased: false) { Id = Guid.NewGuid() };
        father.UpdateGender(Gender.Male.ToString());

        var grandchild = new Member("Grandchild", "Test", "GC1", familyId, isDeceased: false) { Id = Guid.NewGuid() };
        grandchild.UpdateGender(Gender.Male.ToString());

        _context.Members.Add(grandfather);
        _context.Members.Add(father);
        _context.Members.Add(grandchild);
        _context.Relationships.Add(new Relationship(familyId, grandfather.Id, father.Id, RelationshipType.Father) { Id = Guid.NewGuid() });
        _context.Relationships.Add(new Relationship(familyId, father.Id, grandchild.Id, RelationshipType.Father) { Id = Guid.NewGuid() });
        await _context.SaveChangesAsync();

        var members = _context.Members.ToList();
        var relationships = _context.Relationships.ToList();

        // Mock graph behavior
        _mockRelationshipGraph.Setup(g => g.BuildGraph(It.IsAny<IEnumerable<Member>>(), It.IsAny<IEnumerable<Relationship>>()))
            .Callback<IEnumerable<Member>, IEnumerable<Relationship>>((m, r) => { /* Simulate graph built */ });

        var pathToGrandchild = new RelationshipPath(
            new List<Guid> { grandfather.Id, father.Id, grandchild.Id },
            new List<GraphEdge> {
                new GraphEdge(grandfather.Id, father.Id, RelationshipType.Father),
                new GraphEdge(father.Id, grandchild.Id, RelationshipType.Father)
            });
        var pathToGrandfather = new RelationshipPath(
            new List<Guid> { grandchild.Id, father.Id, grandfather.Id },
            new List<GraphEdge> {
                new GraphEdge(grandchild.Id, father.Id, RelationshipType.Child),
                new GraphEdge(father.Id, grandfather.Id, RelationshipType.Child)
            });

        _mockRelationshipGraph.Setup(g => g.FindShortestPath(grandfather.Id, grandchild.Id)).Returns(pathToGrandchild);
        _mockRelationshipGraph.Setup(g => g.FindShortestPath(grandchild.Id, grandfather.Id)).Returns(pathToGrandfather);

        // Mock AI service behavior
        _mockAiGenerateService.Setup(s => s.GenerateDataAsync<RelationshipInferenceResultDto>(
            It.IsAny<GenerateRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RelationshipInferenceResultDto>.Success(new RelationshipInferenceResultDto { FromAToB = "ông nội", FromBToA = "cháu nội" }));

        // Act
        var result = await _service.DetectRelationshipAsync(familyId, grandfather.Id, grandchild.Id, CancellationToken.None); // Added CancellationToken.None

        // Assert
        result.Should().NotBeNull();
        result.FromAToB.Should().Be("ông nội");
        result.FromBToA.Should().Be("cháu nội");
        result.Path.Should().HaveCount(3);
        result.Edges.Should().HaveCount(2);
        result.Path.First().Should().Be(grandfather.Id);
        result.Path.Last().Should().Be(grandchild.Id);
        result.Edges.First().Should().Be(nameof(RelationshipType.Father));
        result.Edges.Last().Should().Be(nameof(RelationshipType.Father));
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra phát hiện không có quan hệ khi hai thành viên không liên quan.
    /// ⚙️ Arrange: Thiết lập dữ liệu thành viên không liên quan và các mock.
    /// ⚙️ Act: Gọi DetectRelationshipAsync.
    /// ⚙️ Assert: Kết quả trả về phải là "unknown" cho cả hai chiều.
    /// </summary>
    [Fact]
    public async Task DetectRelationshipAsync_ShouldReturnUnknown_ForUnrelatedMembers()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberA = new Member("MemberA", "Test", "A1", familyId) { Id = Guid.NewGuid() };
        var memberB = new Member("MemberB", "Test", "B1", familyId) { Id = Guid.NewGuid() };

        _context.Members.Add(memberA);
        _context.Members.Add(memberB);
        await _context.SaveChangesAsync();

        var members = _context.Members.ToList();
        var relationships = _context.Relationships.ToList();

        // Mock graph behavior for no path
        _mockRelationshipGraph.Setup(g => g.BuildGraph(It.IsAny<IEnumerable<Member>>(), It.IsAny<IEnumerable<Relationship>>()))
            .Callback<IEnumerable<Member>, IEnumerable<Relationship>>((m, r) => { /* Simulate graph built */ });

        _mockRelationshipGraph.Setup(g => g.FindShortestPath(memberA.Id, memberB.Id)).Returns(new RelationshipPath());
        _mockRelationshipGraph.Setup(g => g.FindShortestPath(memberB.Id, memberA.Id)).Returns(new RelationshipPath());

        // Mock AI service behavior for unknown
        _mockAiGenerateService.Setup(s => s.GenerateDataAsync<RelationshipInferenceResultDto>(
            It.IsAny<GenerateRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RelationshipInferenceResultDto>.Success(new RelationshipInferenceResultDto { FromAToB = "unknown", FromBToA = "unknown" }));

        // Act
        var result = await _service.DetectRelationshipAsync(familyId, memberA.Id, memberB.Id, CancellationToken.None); // Added CancellationToken.None

        // Assert
        result.Should().NotBeNull();
        result.FromAToB.Should().Be("unknown");
        result.FromBToA.Should().Be("unknown");
        result.Path.Should().BeEmpty();
        result.Edges.Should().BeEmpty();
    }
}