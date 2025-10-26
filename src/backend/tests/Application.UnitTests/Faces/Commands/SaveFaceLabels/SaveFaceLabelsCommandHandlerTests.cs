using AutoFixture;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Faces.Commands.SaveFaceLabels;
using backend.Application.Faces.Queries;
using backend.Application.UnitTests.Common;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using backend.Application.Faces.Common;

namespace backend.Application.UnitTests.Faces.Commands.SaveFaceLabels;

public class SaveFaceLabelsCommandHandlerTests : TestBase
{
    private readonly SaveFaceLabelsCommandHandler _handler;
    private readonly Mock<ILogger<SaveFaceLabelsCommandHandler>> _mockLogger;
    private readonly Mock<IVectorStoreFactory> _mockVectorStoreFactory;
    private readonly Mock<IVectorStore> _mockVectorStore;
    private readonly Mock<IConfigProvider> _mockConfigProvider;

    public SaveFaceLabelsCommandHandlerTests()
    {
        _mockLogger = _fixture.Freeze<Mock<ILogger<SaveFaceLabelsCommandHandler>>>();
        _mockVectorStoreFactory = _fixture.Freeze<Mock<IVectorStoreFactory>>();
        _mockVectorStore = _fixture.Freeze<Mock<IVectorStore>>();
        _mockConfigProvider = _fixture.Freeze<Mock<IConfigProvider>>();

        _mockConfigProvider.Setup(cp => cp.GetSection<VectorStoreSettings>())
                           .Returns(new VectorStoreSettings { Provider = VectorStoreProviderType.InMemory.ToString() });
        _mockVectorStoreFactory.Setup(vsf => vsf.CreateVectorStore(It.IsAny<VectorStoreProviderType>()))
                               .Returns(_mockVectorStore.Object);

        _handler = new SaveFaceLabelsCommandHandler(
            _mockLogger.Object,
            _mockVectorStoreFactory.Object,
            _mockConfigProvider.Object);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler lưu thành công các nhãn khuôn mặt có embedding vào vector store.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một SaveFaceLabelsCommand với một danh sách các DetectedFaceDto có embedding.
    ///               Thiết lập _mockVectorStore để UpsertAsync không làm gì cả.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công. Kiểm tra xem UpsertAsync đã được gọi đúng số lần.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng quá trình lưu nhãn khuôn mặt
    /// và embedding của chúng vào vector store hoạt động chính xác.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldSaveFaceLabelsWithEmbeddingsSuccessfully()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var embedding = new List<double> { 0.1, 0.2, 0.3 };

        var faceLabels = new List<DetectedFaceDto>
        {
            new() {
                Id = Guid.NewGuid().ToString(),
                BoundingBox = new BoundingBoxDto { X = 10, Y = 10, Width = 50, Height = 50 },
                Confidence = 0.9f,
                Embedding = embedding,
                MemberId = memberId,
                MemberName = "John Doe",
                FamilyId = familyId,
                FamilyName = "Doe Family",
                BirthYear = 1990,
                DeathYear = 2050
            }
        };

        var command = new SaveFaceLabelsCommand
        {
            ImageId = imageId,
            FaceLabels = faceLabels
        };

        _mockVectorStore.Setup(vs => vs.UpsertAsync(
            It.IsAny<List<double>>(),
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _mockVectorStore.Verify(vs => vs.UpsertAsync(
            It.IsAny<List<double>>(),
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
