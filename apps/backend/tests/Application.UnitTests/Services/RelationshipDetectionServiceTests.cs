using backend.Application.AI.DTOs;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Prompts.DTOs; // Corrected using directive
using backend.Application.Prompts.Queries.GetPromptById; // Add this
using backend.Application.Services;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Interfaces;
using backend.Domain.ValueObjects;
using FluentAssertions;
using MediatR; // Add this
using Microsoft.Extensions.Logging; // Add this
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Services;

/// <summary>
/// 🎯 Mục tiêu: Kiểm thử hành vi của RelationshipDetectionService.
/// ⚙️ Các bước: Arrange - Act - Assert.
/// 💡 Giải thích: Đảm bảo service có thể phát hiện và suy luận các mối quan hệ một cách chính xác dựa trên đồ thị, AI và các quy tắc cục bộ.
/// </summary>
public class RelationshipDetectionServiceTests : TestBase
{
    private readonly RelationshipDetectionService _service;
    private readonly Mock<IRelationshipGraph> _mockRelationshipGraph;
    private readonly Mock<IAiGenerateService> _mockAiGenerateService;

    private readonly Mock<IMediator> _mockMediator; // New mock for IMediator
    private readonly Mock<ILogger<RelationshipDetectionService>> _mockLogger; // New mock for ILogger

    public RelationshipDetectionServiceTests()
    {
        _mockRelationshipGraph = new Mock<IRelationshipGraph>();
        _mockAiGenerateService = new Mock<IAiGenerateService>();
        _mockMediator = new Mock<IMediator>(); // Initialize IMediator mock
        _mockMediator.Setup(m => m.Send(It.IsAny<GetPromptByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PromptDto>.Success(new PromptDto { Content = "Default AI System Prompt for testing" }));
        _mockMediator.Setup(m => m.Send(It.IsAny<backend.Application.Families.Commands.IncrementFamilyAiChatUsage.IncrementFamilyAiChatUsageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success()); // ADDED: Setup for IncrementFamilyAiChatUsageCommand
        _mockLogger = new Mock<ILogger<RelationshipDetectionService>>(); // Initialize ILogger mock
        _service = new RelationshipDetectionService(
            _context,
            _mockRelationshipGraph.Object,
            _mockAiGenerateService.Object,
            _mockMediator.Object, // Pass mediator mock
            _mockLogger.Object); // Pass logger mock
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra phát hiện quan hệ cha-con trực tiếp thông qua AI khi không có quy tắc cục bộ.
    /// ⚙️ Arrange: Thiết lập dữ liệu thành viên, quan hệ và các mock cho graph/rule engine (không suy luận cục bộ).
    /// ⚙️ Act: Gọi DetectRelationshipAsync.
    /// ⚙️ Assert: Kết quả trả về phải là "cha" và "con" đúng như kỳ vọng từ AI.
    /// </summary>
    [Fact]
    public async Task DetectRelationshipAsync_ShouldReturnFatherChild_ForDirectRelation_ViaAI()
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


        // Mock AI service behavior
        _mockAiGenerateService.Setup(s => s.GenerateDataAsync<RelationshipInferenceResultDto>(
            It.IsAny<GenerateRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RelationshipInferenceResultDto>.Success(new RelationshipInferenceResultDto { InferredRelationship = "cha (từ A đến B) và con (từ B đến A)" }));


        // Act
        var result = await _service.DetectRelationshipAsync(familyId, father.Id, child.Id, CancellationToken.None); // Added CancellationToken.None

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().Contain("cha (từ A đến B)");
        result.Description.Should().Contain("con (từ B đến A)");
        result.Path.Should().HaveCount(2);
        result.Edges.Should().HaveCount(1);
        result.Path.First().Should().Be(father.Id);
        result.Path.Last().Should().Be(child.Id);
        result.Edges.First().Should().Be(nameof(RelationshipType.Father));

        _mockAiGenerateService.Verify(s => s.GenerateDataAsync<RelationshipInferenceResultDto>(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()), Times.Once); // Verify AI was called
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra phát hiện quan hệ ông nội-cháu nội thông qua AI khi không có quy tắc cục bộ.
    /// ⚙️ Arrange: Thiết lập dữ liệu thành viên, quan hệ và các mock cho graph/rule engine (không suy luận cục bộ).
    /// ⚙️ Act: Gọi DetectRelationshipAsync.
    /// ⚙️ Assert: Kết quả trả về phải là "ông nội" và "cháu nội" đúng như kỳ vọng từ AI.
    /// </summary>
    [Fact]
    public async Task DetectRelationshipAsync_ShouldReturnGrandfatherGrandchild_ForTwoGenerationRelation_ViaAI()
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


        // Mock AI service behavior
        _mockAiGenerateService.Setup(s => s.GenerateDataAsync<RelationshipInferenceResultDto>(
            It.IsAny<GenerateRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RelationshipInferenceResultDto>.Success(new RelationshipInferenceResultDto { InferredRelationship = "ông nội (từ A đến B) và cháu nội (từ B đến A)" }));

        // Act
        var result = await _service.DetectRelationshipAsync(familyId, grandfather.Id, grandchild.Id, CancellationToken.None); // Added CancellationToken.None

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().Contain("ông nội (từ A đến B)");
        result.Description.Should().Contain("cháu nội (từ B đến A)");
        result.Path.Should().HaveCount(3);
        result.Edges.Should().HaveCount(2);
        result.Path.First().Should().Be(grandfather.Id);
        result.Path.Last().Should().Be(grandchild.Id);
        result.Edges.First().Should().Be(nameof(RelationshipType.Father));
        result.Edges.Last().Should().Be(nameof(RelationshipType.Father));

        _mockAiGenerateService.Verify(s => s.GenerateDataAsync<RelationshipInferenceResultDto>(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()), Times.Once); // Verify AI was called
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra phát hiện không có quan hệ khi hai thành viên không liên quan.
    /// ⚙️ Arrange: Thiết lập dữ liệu thành viên không liên quan và các mock.
    /// ⚙️ Act: Gọi DetectRelationshipAsync.
    /// ⚙️ Assert: Kết quả trả về phải là "Không tìm thấy đường dẫn quan hệ."
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



        // Mock AI service behavior (should NOT be called)
        _mockAiGenerateService.Setup(s => s.GenerateDataAsync<RelationshipInferenceResultDto>(
            It.IsAny<GenerateRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RelationshipInferenceResultDto>.Success(new RelationshipInferenceResultDto { InferredRelationship = "Should not be called" }));

        // Act
        var result = await _service.DetectRelationshipAsync(familyId, memberA.Id, memberB.Id, CancellationToken.None); // Added CancellationToken.None

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().Be("Không tìm thấy đường dẫn quan hệ.");
        result.Path.Should().BeEmpty();
        result.Edges.Should().BeEmpty();

        _mockAiGenerateService.Verify(s => s.GenerateDataAsync<RelationshipInferenceResultDto>(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()), Times.Never); // Verify AI was NOT called
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra xem việc gọi AI có tăng hạn mức sử dụng AI hay không.
    /// ⚙️ Arrange: Thiết lập dữ liệu thành viên, quan hệ và các mock cho graph/rule engine (không suy luận cục bộ), và AI trả về thành công.
    /// ⚙️ Act: Gọi DetectRelationshipAsync.
    /// ⚙️ Assert: Đảm bảo IncrementFamilyAiChatUsageCommand được gửi và AiGenerateService được gọi.
    /// </summary>
    [Fact]
    public async Task DetectRelationshipAsync_ShouldIncrementAiChatUsage_WhenAiIsCalled()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberA = new Member("MemberA", "Test", "A1", familyId, isDeceased: false) { Id = Guid.NewGuid() };
        var memberB = new Member("MemberB", "Test", "B1", familyId, isDeceased: false) { Id = Guid.NewGuid() };

        _context.Members.Add(memberA);
        _context.Members.Add(memberB);
        await _context.SaveChangesAsync();

        var members = _context.Members.ToList();
        var relationships = _context.Relationships.ToList();

        _mockRelationshipGraph.Setup(g => g.BuildGraph(It.IsAny<IEnumerable<Member>>(), It.IsAny<IEnumerable<Relationship>>()))
            .Callback<IEnumerable<Member>, IEnumerable<Relationship>>((m, r) => { /* Simulate graph built */ });

        var pathToB = new RelationshipPath(new List<Guid> { memberA.Id, memberB.Id }, new List<GraphEdge> { new GraphEdge(memberA.Id, memberB.Id, RelationshipType.Child) });
        _mockRelationshipGraph.Setup(g => g.FindShortestPath(memberA.Id, memberB.Id)).Returns(pathToB);


        _mockMediator.Setup(m => m.Send(It.IsAny<backend.Application.Families.Commands.IncrementFamilyAiChatUsage.IncrementFamilyAiChatUsageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success()); // Simulate successful quota increment

        _mockAiGenerateService.Setup(s => s.GenerateDataAsync<RelationshipInferenceResultDto>(
            It.IsAny<GenerateRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RelationshipInferenceResultDto>.Success(new RelationshipInferenceResultDto { InferredRelationship = "friend" }));

        // Act
        var result = await _service.DetectRelationshipAsync(familyId, memberA.Id, memberB.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().Be("friend");
        _mockMediator.Verify(m => m.Send(It.IsAny<backend.Application.Families.Commands.IncrementFamilyAiChatUsage.IncrementFamilyAiChatUsageCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockAiGenerateService.Verify(s => s.GenerateDataAsync<RelationshipInferenceResultDto>(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }


}
