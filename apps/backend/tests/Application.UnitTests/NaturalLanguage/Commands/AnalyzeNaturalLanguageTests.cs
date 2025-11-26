using System.Text.Json;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.NaturalLanguage.Commands.AnalyzeNaturalLanguage;
using backend.Application.NaturalLanguage.Models;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.NaturalLanguage.Commands;

public class AnalyzeNaturalLanguageTests : TestBase
{
    private readonly Mock<IN8nService> _mockN8nService;
    private readonly AnalyzeNaturalLanguageCommandHandler _handler;

    public AnalyzeNaturalLanguageTests()
    {
        _mockN8nService = new Mock<IN8nService>();
        _handler = new AnalyzeNaturalLanguageCommandHandler(_mockN8nService.Object, _context);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenN8nServiceFails()
    {
        // Arrange
        var command = new AnalyzeNaturalLanguageCommand { Content = "Test content", SessionId = "session1", FamilyId = Guid.NewGuid() };
        _mockN8nService.Setup(s => s.CallChatWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure("N8n service error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("N8n service error");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenAIResponseIsEmpty()
    {
        // Arrange
        var command = new AnalyzeNaturalLanguageCommand { Content = "Test content", SessionId = "session1", FamilyId = Guid.NewGuid() };
        _mockN8nService.Setup(s => s.CallChatWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("")); // Empty AI response

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Phản hồi từ AI trống hoặc không hợp lệ.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenAIResponseIsInvalidJson()
    {
        // Arrange
        var command = new AnalyzeNaturalLanguageCommand { Content = "Test content", SessionId = "session1", FamilyId = Guid.NewGuid() };
        _mockN8nService.Setup(s => s.CallChatWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("{invalid json")); // Invalid JSON

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Không thể phân tích phản hồi JSON từ AI");
    }

    [Fact]
    public async Task Handle_ShouldProcessNewMemberSuccessfully()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var command = new AnalyzeNaturalLanguageCommand { Content = "Add John Doe", SessionId = "session1", FamilyId = familyId };
        var aiResponse = new AnalyzedDataDto
        {
            Members = new List<MemberDataDto>
            {
                new MemberDataDto { Id = "temp_1", FirstName = "John", LastName = "Doe", Gender = "Male" }
            }
        };
        _mockN8nService.Setup(s => s.CallChatWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(JsonSerializer.Serialize(aiResponse)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        var analyzedResult = result.Value!; // Assign to non-nullable local variable
        analyzedResult.Members.Should().ContainSingle();
        var memberResult = analyzedResult.Members.First();
        memberResult.Id.Should().NotBe(Guid.Empty); // New GUID generated
        memberResult.FirstName.Should().Be("John");
        memberResult.LastName.Should().Be("Doe");
        memberResult.IsExisting.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldProcessExistingMemberSuccessfully()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var existingMemberId = Guid.NewGuid();
        var existingMemberCode = "M001";
        var existingMember = new Member("Doe", "John", existingMemberCode, familyId) { Id = existingMemberId };

        var command = new AnalyzeNaturalLanguageCommand { Content = "Update John Doe", SessionId = "session1", FamilyId = familyId };
        var aiResponse = new AnalyzedDataDto
        {
            Members = new List<MemberDataDto>
            {
                new MemberDataDto { Id = "temp_1", FirstName = "John", LastName = "Doe", Code = existingMemberCode, Gender = "Male" }
            }
        };
        _mockN8nService.Setup(s => s.CallChatWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(JsonSerializer.Serialize(aiResponse)));

        await _context.Members.AddAsync(existingMember); // Use AddAsync
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        var analyzedResult = result.Value!; // Assign to non-nullable local variable
        analyzedResult.Members.Should().ContainSingle();
        var memberResult = analyzedResult.Members.First();
        memberResult.Id.Should().Be(existingMemberId); // Existing GUID used
        memberResult.Code.Should().Be(existingMemberCode);
        memberResult.IsExisting.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldProcessRelationshipSuccessfully()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var member1Guid = Guid.NewGuid();
        var member2Guid = Guid.NewGuid();

        var command = new AnalyzeNaturalLanguageCommand { Content = "John is husband of Jane", SessionId = "session1", FamilyId = familyId };
        var aiResponse = new AnalyzedDataDto
        {
            Members = new List<MemberDataDto>
            {
                new MemberDataDto { Id = "temp_1", FirstName = "John", LastName = "Doe", Gender = "Male" },
                new MemberDataDto { Id = "temp_2", FirstName = "Jane", LastName = "Doe", Gender = "Female" }
            },
            Relationships = new List<RelationshipDataDto>
            {
                new RelationshipDataDto { SourceMemberId = "temp_1", TargetMemberId = "temp_2", Type = "Husband" }
            }
        };
        _mockN8nService.Setup(s => s.CallChatWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(JsonSerializer.Serialize(aiResponse)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        var analyzedResult = result.Value!; // Assign to non-nullable local variable
        analyzedResult.Relationships.Should().ContainSingle();
        var relationshipResult = analyzedResult.Relationships.First();
        relationshipResult.SourceMemberId.Should().NotBe(Guid.Empty);
        relationshipResult.TargetMemberId.Should().NotBe(Guid.Empty);
        relationshipResult.Type.Should().Be(Domain.Enums.RelationshipType.Husband);
    }

    [Fact]
    public async Task Handle_ShouldProcessEventSuccessfully()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var member1Guid = Guid.NewGuid();

        var command = new AnalyzeNaturalLanguageCommand { Content = "John's birthday on 2000-01-01", SessionId = "session1", FamilyId = familyId };
        var aiResponse = new AnalyzedDataDto
        {
            Members = new List<MemberDataDto>
            {
                new MemberDataDto { Id = "temp_1", FirstName = "John", LastName = "Doe", Gender = "Male" }
            },
            Events = new List<EventDataDto>
            {
                new EventDataDto { Description = "Birthday", Date = "2000-01-01", RelatedMemberIds = new List<string> { "temp_1" }, Type = "Birthday" }
            }
        };
        _mockN8nService.Setup(s => s.CallChatWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(JsonSerializer.Serialize(aiResponse)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        var analyzedResult = result.Value!; // Assign to non-nullable local variable
        analyzedResult.Events.Should().ContainSingle();
        var eventResult = analyzedResult.Events.First();
        eventResult.Id.Should().NotBe(Guid.Empty); // New GUID generated
        eventResult.Description.Should().Be("Birthday");
        eventResult.Date.Should().Be("2000-01-01");
        eventResult.RelatedMemberIds.Should().ContainSingle();
        eventResult.RelatedMemberIds.First().Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ShouldReturnMemberValidationErrors()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var command = new AnalyzeNaturalLanguageCommand { Content = "Add member with no name", SessionId = "session1", FamilyId = familyId };
        var aiResponse = new AnalyzedDataDto
        {
            Members = new List<MemberDataDto>
            {
                new MemberDataDto { Id = "temp_1", FirstName = "", LastName = "" } // Invalid member data
            }
        };
        _mockN8nService.Setup(s => s.CallChatWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(JsonSerializer.Serialize(aiResponse)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Still succeeds, but member has error message
        result.Value.Should().NotBeNull();
        var analyzedResult = result.Value!; // Assign to non-nullable local variable
        analyzedResult.Members.Should().ContainSingle();
        var memberResult = analyzedResult.Members.First();
        memberResult.ErrorMessage.Should().NotBeNullOrWhiteSpace();
        memberResult.ErrorMessage.Should().Contain("Tên của thành viên");
        memberResult.ErrorMessage.Should().Contain("Họ của thành viên");
    }

    [Fact]
    public async Task Handle_ShouldReturnRelationshipValidationErrors_WhenSourceOrTargetMemberIdMissing()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var command = new AnalyzeNaturalLanguageCommand { Content = "Relationship with missing members", SessionId = "session1", FamilyId = familyId };
        var aiResponse = new AnalyzedDataDto
        {
            Relationships = new List<RelationshipDataDto>
            {
                new RelationshipDataDto { SourceMemberId = "non_existent", TargetMemberId = "temp_2", Type = "Husband" } // SourceMemberId not mapped
            }
        };
        _mockN8nService.Setup(s => s.CallChatWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(JsonSerializer.Serialize(aiResponse)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Still succeeds, but relationship is not added
        result.Value.Should().NotBeNull();
        var analyzedResult = result.Value!; // Assign to non-nullable local variable
        analyzedResult.Relationships.Should().BeEmpty(); // Relationship should not be processed due to error
    }

    [Fact]
    public async Task Handle_ShouldReturnRelationshipValidationErrors_WhenInvalidRelationshipType()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var member1Guid = Guid.NewGuid();
        var member2Guid = Guid.NewGuid();

        var command = new AnalyzeNaturalLanguageCommand { Content = "Relationship with invalid type", SessionId = "session1", FamilyId = familyId };
        var aiResponse = new AnalyzedDataDto
        {
            Members = new List<MemberDataDto>
            {
                new MemberDataDto { Id = "temp_1", FirstName = "John", LastName = "Doe", Gender = "Male" },
                new MemberDataDto { Id = "temp_2", FirstName = "Jane", LastName = "Doe", Gender = "Female" }
            },
            Relationships = new List<RelationshipDataDto>
            {
                new RelationshipDataDto { SourceMemberId = "temp_1", TargetMemberId = "temp_2", Type = "InvalidType" } // Invalid type
            }
        };
        _mockN8nService.Setup(s => s.CallChatWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(JsonSerializer.Serialize(aiResponse)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Still succeeds, but relationship is not added
        result.Value.Should().NotBeNull();
        var analyzedResult = result.Value!; // Assign to non-nullable local variable
        analyzedResult.Relationships.Should().BeEmpty(); // Relationship should not be processed due to error
    }
}
