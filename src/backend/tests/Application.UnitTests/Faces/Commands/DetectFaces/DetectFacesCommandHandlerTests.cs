using AutoFixture;
using backend.Application.AI.VectorStore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Faces.Commands;
using backend.Application.Faces.Commands.DetectFaces;
using backend.Application.Faces.Common;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Faces.Commands.DetectFaces;

public class DetectFacesCommandHandlerTests : TestBase
{
    private readonly DetectFacesCommandHandler _handler;
    private readonly Mock<IFaceApiService> _mockFaceApiService;
    private readonly Mock<IVectorStoreFactory> _mockVectorStoreFactory;
    private readonly Mock<IVectorStore> _mockVectorStore;
    private readonly Mock<ILogger<DetectFacesCommandHandler>> _mockLogger;
    private readonly Mock<IConfigProvider> _mockConfigProvider;

    public DetectFacesCommandHandlerTests()
    {
        _mockFaceApiService = _fixture.Freeze<Mock<IFaceApiService>>();
        _mockVectorStoreFactory = _fixture.Freeze<Mock<IVectorStoreFactory>>();
        _mockVectorStore = _fixture.Freeze<Mock<IVectorStore>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<DetectFacesCommandHandler>>>();
        _mockConfigProvider = _fixture.Freeze<Mock<IConfigProvider>>();

        _mockConfigProvider.Setup(cp => cp.GetSection<VectorStoreSettings>())
                           .Returns(new VectorStoreSettings { Provider = VectorStoreProviderType.InMemory.ToString() });
        _mockVectorStoreFactory.Setup(vsf => vsf.CreateVectorStore(It.IsAny<VectorStoreProviderType>()))
                               .Returns(_mockVectorStore.Object);

        _handler = new DetectFacesCommandHandler(
            _mockFaceApiService.Object,
            _context,
            _mockVectorStoreFactory.Object,
            _mockConfigProvider.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDetectedFaces_WhenNoEmbeddings()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về các khuôn mặt được phát hiện
        // khi dịch vụ Face API phát hiện khuôn mặt nhưng không có embedding (ví dụ: khuôn mặt không rõ ràng).

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Thiết lập _mockFaceApiService để trả về một danh sách FaceDetectionResultDto không có embedding.
        // 2. Tạo một DetectFacesCommand bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về không phải là null.
        // 2. Kiểm tra xem số lượng khuôn mặt được phát hiện khớp với số lượng trả về từ Face API.
        // 3. Kiểm tra xem không có MemberId nào được gán (vì không có embedding để tìm kiếm).

        // Arrange
        var faceResults = new List<FaceDetectionResultDto>
        {
            new() {
                Id = Guid.NewGuid().ToString(),
                BoundingBox = new BoundingBoxDto { X = 10, Y = 10, Width = 50, Height = 50 },
                Confidence = 0.9f,
                Thumbnail = "base64thumb",
                Embedding = null // No embedding
            }
        };
        _mockFaceApiService.Setup(s => s.DetectFacesAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<bool>()))
                           .ReturnsAsync(faceResults);

        var command = _fixture.Create<DetectFacesCommand>();

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        var detectedFacesResponse = response.Value!;
        detectedFacesResponse.DetectedFaces.Should().HaveCount(1);
        _mockVectorStore.Verify(vs => vs.QueryAsync(It.IsAny<double[]>(), It.IsAny<int>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        // 💡 Giải thích:
        // Test này đảm bảo rằng handler xử lý đúng trường hợp không có embedding từ Face API,
        // trả về các khuôn mặt được phát hiện mà không cố gắng truy vấn vector store.
    }

    [Fact]
    public async Task Handle_ShouldReturnDetectedFacesWithMemberInfo_WhenEmbeddingMatches()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về các khuôn mặt được phát hiện với thông tin thành viên
        // khi một embedding khuôn mặt khớp với một thành viên hiện có trong vector store và cơ sở dữ liệu.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một Family và Member, sau đó thêm vào DB.
        // 2. Tạo một FaceDetectionResultDto với embedding.
        // 3. Thiết lập _mockFaceApiService để trả về danh sách FaceDetectionResultDto.
        // 4. Thiết lập _mockVectorStore để trả về một VectorStoreQueryResult khớp với MemberId.
        // 5. Tạo một DetectFacesCommand bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về không phải là null.
        // 2. Kiểm tra xem số lượng khuôn mặt được phát hiện khớp với số lượng trả về từ Face API.
        // 3. Kiểm tra xem MemberId và MemberName đã được gán chính xác.

        // Arrange
        _context.Families.RemoveRange(_context.Families);
        _context.Members.RemoveRange(_context.Members);
        await _context.SaveChangesAsync(CancellationToken.None);

        var family = new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF1" };
        var member = new Member
        {
            Id = Guid.NewGuid(),
            FamilyId = family.Id,
            FirstName = "John",
            LastName = "Doe",
            Code = "JD001",
            DateOfBirth = new DateTime(1990, 1, 1),
            DateOfDeath = new DateTime(2050, 1, 1)
        };
        _context.Families.Add(family);
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var embedding = new double[] { 0.1, 0.2, 0.3 };
        var faceResults = new List<FaceDetectionResultDto>
        {
            new() {
                Id = Guid.NewGuid().ToString(),
                BoundingBox = new BoundingBoxDto { X = 10, Y = 10, Width = 50, Height = 50 },
                Confidence = 0.9f,
                Thumbnail = "base64thumb",
                Embedding = embedding
            }
        };
        _mockFaceApiService.Setup(s => s.DetectFacesAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<bool>()))
                           .ReturnsAsync(faceResults);

        var queryResult = new VectorStoreQueryResult
        {
            Id = "some_id",
            Score = 0.9f,
            Metadata = new Dictionary<string, string>
            {
                { "member_id", member.Id.ToString() },
                { "family_id", family.Id.ToString() },
                { "family_name", family.Name },
                { "birth_year", member.DateOfBirth?.Year.ToString() ?? string.Empty },
                { "death_year", member.DateOfDeath?.Year.ToString() ?? string.Empty }
            }
        };
        _mockVectorStore.Setup(vs => vs.QueryAsync(embedding, 1, It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync([queryResult]);

        var command = _fixture.Create<DetectFacesCommand>();

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        var detectedFacesResponse = response.Value!;
        detectedFacesResponse.DetectedFaces.Should().HaveCount(1);
        detectedFacesResponse.DetectedFaces.First().MemberId.Should().Be(member.Id);
        detectedFacesResponse.DetectedFaces.First().MemberName.Should().Be(member.FullName);
        detectedFacesResponse.DetectedFaces.First().FamilyId.Should().Be(family.Id);
        detectedFacesResponse.DetectedFaces.First().FamilyName.Should().Be(family.Name);
        detectedFacesResponse.DetectedFaces.First().BirthYear.Should().Be(member.DateOfBirth?.Year);
        detectedFacesResponse.DetectedFaces.First().DeathYear.Should().Be(member.DateOfDeath?.Year);

        // 💡 Giải thích:
        // Test này đảm bảo rằng khi một embedding khuôn mặt khớp với một thành viên hiện có,
        // handler sẽ truy xuất và gán thông tin thành viên đó vào DetectedFaceDto.
    }

    [Fact]
    public async Task Handle_ShouldReturnDetectedFacesWithoutMemberInfo_WhenEmbeddingDoesNotMatch()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler trả về các khuôn mặt được phát hiện mà không có thông tin thành viên
        // khi một embedding khuôn mặt không khớp với bất kỳ thành viên hiện có nào trong vector store.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một FaceDetectionResultDto với embedding.
        // 2. Thiết lập _mockFaceApiService để trả về danh sách FaceDetectionResultDto.
        // 3. Thiết lập _mockVectorStore để trả về một danh sách QueryResult trống (không khớp).
        // 4. Tạo một DetectFacesCommand bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về không phải là null.
        // 2. Kiểm tra xem số lượng khuôn mặt được phát hiện khớp với số lượng trả về từ Face API.
        // 3. Kiểm tra xem MemberId và MemberName vẫn là null (vì không có khớp).

        // Arrange
        var embedding = new double[] { 0.4, 0.5, 0.6 };
        var faceResults = new List<FaceDetectionResultDto>
        {
            new() {
                Id = Guid.NewGuid().ToString(),
                BoundingBox = new BoundingBoxDto { X = 10, Y = 10, Width = 50, Height = 50 },
                Confidence = 0.9f,
                Thumbnail = "base64thumb",
                Embedding = embedding
            }
        };
        _mockFaceApiService.Setup(s => s.DetectFacesAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<bool>()))
                           .ReturnsAsync(faceResults);

        _mockVectorStore.Setup(vs => vs.QueryAsync(embedding, 1, It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync([]); // No match

        var command = _fixture.Create<DetectFacesCommand>();

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        var detectedFacesResponse = response.Value!;
        detectedFacesResponse.DetectedFaces.Should().HaveCount(1);
        detectedFacesResponse.DetectedFaces.First().MemberId.Should().BeNull();
        detectedFacesResponse.DetectedFaces.First().MemberName.Should().BeNull();

        // 💡 Giải thích:
        // Test này đảm bảo rằng khi không có khớp nào trong vector store,
        // handler sẽ trả về khuôn mặt được phát hiện mà không có thông tin thành viên.
    }

    [Fact]
    public async Task Handle_ShouldLogErrors_WhenVectorStoreQueryFails()
    {
        // 🎯 Mục tiêu của test:
        // Xác minh rằng handler ghi lại lỗi khi truy vấn vector store thất bại.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange:
        // 1. Tạo một FaceDetectionResultDto với embedding.
        // 2. Thiết lập _mockFaceApiService để trả về danh sách FaceDetectionResultDto.
        // 3. Thiết lập _mockVectorStore để ném một ngoại lệ khi QueryAsync được gọi.
        // 4. Tạo một DetectFacesCommand bất kỳ.
        // Act:
        // 1. Gọi phương thức Handle của handler.
        // Assert:
        // 1. Kiểm tra xem kết quả trả về không phải là null.
        // 2. Kiểm tra xem số lượng khuôn mặt được phát hiện khớp với số lượng trả về từ Face API.
        // 3. Kiểm tra xem lỗi đã được ghi lại thông qua ILogger.

        // Arrange
        var embedding = new double[] { 0.7, 0.8, 0.9 };
        var faceResults = new List<FaceDetectionResultDto>
        {
            new() {
                Id = Guid.NewGuid().ToString(),
                BoundingBox = new BoundingBoxDto { X = 10, Y = 10, Width = 50, Height = 50 },
                Confidence = 0.9f,
                Thumbnail = "base64thumb",
                Embedding = embedding
            }
        };
        _mockFaceApiService.Setup(s => s.DetectFacesAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<bool>()))
                           .ReturnsAsync(faceResults);

        _mockVectorStore.Setup(vs => vs.QueryAsync(embedding, 1, It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new Exception("Vector store query failed."));

        var command = _fixture.Create<DetectFacesCommand>();

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        var detectedFacesResponse = response.Value!;
        detectedFacesResponse.DetectedFaces.Should().HaveCount(1);
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error querying vector store for face detection.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);

        // 💡 Giải thích:
        // Test này đảm bảo rằng handler ghi lại lỗi một cách thích hợp khi có sự cố
        // trong quá trình truy vấn vector store, giúp dễ dàng gỡ lỗi và giám sát.
    }
}
