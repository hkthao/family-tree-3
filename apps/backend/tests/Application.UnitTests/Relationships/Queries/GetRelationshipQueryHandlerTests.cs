using backend.Application.Relationships.Queries;
using backend.Application.Services;
using backend.Application.UnitTests.Common;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Application.UnitTests.Relationships.Queries;

/// <summary>
/// 🎯 Mục tiêu: Kiểm thử hành vi của GetRelationshipQueryHandler.
/// ⚙️ Các bước: Arrange - Act - Assert.
/// 💡 Giải thích: Đảm bảo handler gọi đúng service phát hiện quan hệ và trả về kết quả.
/// </summary>
public class GetRelationshipQueryHandlerTests : TestBase
{
    private readonly Mock<IRelationshipDetectionService> _mockRelationshipDetectionService;
    private readonly GetRelationshipQueryHandler _handler;

    public GetRelationshipQueryHandlerTests()
    {
        _mockRelationshipDetectionService = new Mock<IRelationshipDetectionService>();
        _handler = new GetRelationshipQueryHandler(_mockRelationshipDetectionService.Object);
    }

    /// <summary>
    /// 🎯 Mục tiêu: Kiểm tra handler trả về kết quả phát hiện quan hệ thành công.
    /// ⚙️ Arrange: Thiết lập mock service trả về kết quả cụ thể.
    /// ⚙️ Act: Gửi GetRelationshipQuery.
    /// ⚙️ Assert: Kết quả trả về phải khớp với kết quả từ service mock.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnRelationshipDetectionResult_WhenCalled()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberAId = Guid.NewGuid();
        var memberBId = Guid.NewGuid();
        var expectedResult = new RelationshipDetectionResult
        {
            FromAToB = "cha",
            FromBToA = "con",
            Path = new System.Collections.Generic.List<Guid> { memberAId, memberBId },
            Edges = new System.Collections.Generic.List<string> { "Father" }
        };

        _mockRelationshipDetectionService.Setup(s => s.DetectRelationshipAsync(familyId, memberAId, memberBId))
            .ReturnsAsync(expectedResult);

        var query = new GetRelationshipQuery(familyId, memberAId, memberBId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedResult);
        _mockRelationshipDetectionService.Verify(s => s.DetectRelationshipAsync(familyId, memberAId, memberBId), Times.Once);
    }
}
