using System.Text.Json;
using backend.Application.AI.Commands.AnalyzeNaturalLanguage;
using backend.Application.AI.Models;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.AI.Commands.AnalyzeNaturalLanguage;

public class AnalyzeNaturalLanguageCommandHandlerTests : TestBase
{
    private readonly Mock<IN8nService> _n8nServiceMock;
    private readonly AnalyzeNaturalLanguageCommandHandler _handler;

    public AnalyzeNaturalLanguageCommandHandlerTests() : base()
    {
        _n8nServiceMock = new Mock<IN8nService>();
        _handler = new AnalyzeNaturalLanguageCommandHandler(_n8nServiceMock.Object, _context);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithAnalyzedResult_WhenN8nServiceReturnsValidJson()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var command = new AnalyzeNaturalLanguageCommand
        {
            Content = "Thêm ông nội John Doe (mã JD101) sinh năm 1950. Ông có con là Jane Smith.",
            SessionId = "testSessionId",
            FamilyId = familyId
        };

        var existingMember = new Member("John", "Doe", "JD101", familyId, false) { Id = Guid.NewGuid() };
        _context.Members.Add(existingMember);
        await _context.SaveChangesAsync();

        var aiResponse = new AnalyzedDataDto
        {
            Members = new List<MemberDataDto>
            {
                new MemberDataDto { Id = "ai_mem_1", FirstName = "John", LastName = "Doe", Code = "JD101", DateOfBirth = "1950", Gender = "Male" },
                new MemberDataDto { Id = "ai_mem_2", FirstName = "Jane", LastName = "Smith", Gender = "Female" }
            },
            Relationships = new List<RelationshipDataDto>
            {
                new RelationshipDataDto { SourceMemberId = "ai_mem_1", TargetMemberId = "ai_mem_2", Type = "Father" }
            },
            Events = new List<EventDataDto>(),
            Feedback = "Looks good"
        };
        var n8nResponseJson = JsonSerializer.Serialize(aiResponse);

        _n8nServiceMock.Setup(x => x.CallChatWebhookAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(n8nResponseJson));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Members.Should().HaveCount(2);
        result.Value.Relationships.Should().HaveCount(1);
        result.Value.Events.Should().BeEmpty();
        result.Value.Feedback.Should().Be("Looks good");

        // Verify existing member is identified
        result.Value.Members[0].IsExisting.Should().BeTrue();
        result.Value.Members[0].Id.Should().Be(existingMember.Id);
        result.Value.Members[0].Code.Should().Be("JD101");

        // Verify new member has a new GUID
        result.Value.Members[1].IsExisting.Should().BeFalse();
        result.Value.Members[1].Id.Should().NotBe(Guid.Empty);

        // Verify relationship member IDs are mapped to GUIDs
        result.Value.Relationships[0].SourceMemberId.Should().Be(existingMember.Id);
        result.Value.Relationships[0].TargetMemberId.Should().Be(result.Value.Members[1].Id);
        result.Value.Relationships[0].Type.Should().Be(RelationshipType.Father);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nServiceCallFails()
    {
        // Arrange
        var command = new AnalyzeNaturalLanguageCommand
        {
            Content = "Some content",
            SessionId = "testSessionId",
            FamilyId = Guid.NewGuid()
        };

        _n8nServiceMock.Setup(x => x.CallChatWebhookAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure("N8n service error", "N8nService"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<backend.Application.Common.Exceptions.ValidationException>()
            .WithMessage("N8n service error");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nServiceReturnsEmptyResponse()
    {
        // Arrange
        var command = new AnalyzeNaturalLanguageCommand
        {
            Content = "Some content",
            SessionId = "testSessionId",
            FamilyId = Guid.NewGuid()
        };

        _n8nServiceMock.Setup(x => x.CallChatWebhookAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(""));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Phản hồi từ AI trống hoặc không hợp lệ.");
        result.ErrorSource.Should().Be("AIResponse");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nServiceReturnsInvalidJson()
    {
        // Arrange
        var command = new AnalyzeNaturalLanguageCommand
        {
            Content = "Some content",
            SessionId = "testSessionId",
            FamilyId = Guid.NewGuid()
        };

        _n8nServiceMock.Setup(x => x.CallChatWebhookAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("{invalid json}"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Không thể phân tích phản hồi JSON từ AI:");
        result.ErrorSource.Should().Be("AIResponseParsing");
    }

    [Fact]
    public async Task Handle_ShouldHandleMembersWithMissingRequiredFieldsCorrectly()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var command = new AnalyzeNaturalLanguageCommand
        {
            Content = "Thêm thành viên thiếu tên họ và tên.",
            SessionId = "testSessionId",
            FamilyId = familyId
        };

        var aiResponse = new AnalyzedDataDto
        {
            Members = new List<MemberDataDto>
            {
                new MemberDataDto { Id = "ai_mem_1", FirstName = "", LastName = "", Code = "ERR1" } // Missing required fields
            }
        };
        var n8nResponseJson = JsonSerializer.Serialize(aiResponse);

        _n8nServiceMock.Setup(x => x.CallChatWebhookAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(n8nResponseJson));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Members.Should().HaveCount(1);
        result.Value.Members[0].ErrorMessage.Should().Contain("Tên của thành viên không được để trống.");
        result.Value.Members[0].ErrorMessage.Should().Contain("Họ của thành viên không được để trống.");
    }

    [Fact]
    public async Task Handle_ShouldHandleRelationshipsWithInvalidMemberIdsCorrectly()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var command = new AnalyzeNaturalLanguageCommand
        {
            Content = "Một mối quan hệ với ID thành viên không tồn tại từ AI.",
            SessionId = "testSessionId",
            FamilyId = familyId
        };

        var aiResponse = new AnalyzedDataDto
        {
            Members = new List<MemberDataDto>
            {
                new MemberDataDto { Id = "ai_mem_1", FirstName = "Valid", LastName = "Member" }
            },
            Relationships = new List<RelationshipDataDto>
            {
                new RelationshipDataDto { SourceMemberId = "ai_mem_1", TargetMemberId = "ai_mem_invalid", Type = "Child" }
            }
        };
        var n8nResponseJson = JsonSerializer.Serialize(aiResponse);

        _n8nServiceMock.Setup(x => x.CallChatWebhookAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(n8nResponseJson));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Relationships.Should().BeEmpty(); // Invalid relationships should not be added
    }

    [Fact]
    public async Task Handle_ShouldHandleRelationshipsWithInvalidRelationshipTypeCorrectly()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var command = new AnalyzeNaturalLanguageCommand
        {
            Content = "Một mối quan hệ với loại không hợp lệ.",
            SessionId = "testSessionId",
            FamilyId = familyId
        };

        var aiResponse = new AnalyzedDataDto
        {
            Members = new List<MemberDataDto>
            {
                new MemberDataDto { Id = "ai_mem_1", FirstName = "Mem", LastName = "One" },
                new MemberDataDto { Id = "ai_mem_2", FirstName = "Mem", LastName = "Two" }
            },
            Relationships = new List<RelationshipDataDto>
            {
                new RelationshipDataDto { SourceMemberId = "ai_mem_1", TargetMemberId = "ai_mem_2", Type = "InvalidType" }
            }
        };
        var n8nResponseJson = JsonSerializer.Serialize(aiResponse);

        _n8nServiceMock.Setup(x => x.CallChatWebhookAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(n8nResponseJson));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Relationships.Should().BeEmpty(); // Invalid relationships should not be added
    }

    [Fact]
    public async Task Handle_ShouldHandleEventsWithMissingRequiredFieldsCorrectly()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var command = new AnalyzeNaturalLanguageCommand
        {
            Content = "Một sự kiện thiếu thông tin.",
            SessionId = "testSessionId",
            FamilyId = familyId
        };

        var aiResponse = new AnalyzedDataDto
        {
            Events = new List<EventDataDto>
            {
                new EventDataDto { Type = "", Description = "" } // Missing required fields
            }
        };
        var n8nResponseJson = JsonSerializer.Serialize(aiResponse);

        _n8nServiceMock.Setup(x => x.CallChatWebhookAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(n8nResponseJson));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Events.Should().HaveCount(1);
        result.Value.Events[0].ErrorMessage.Should().Contain("Loại sự kiện không được để trống.");
        result.Value.Events[0].ErrorMessage.Should().Contain("Mô tả sự kiện không được để trống.");
    }
}
