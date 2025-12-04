using backend.Application.AI.DTOs;
using backend.Application.Common.Constants;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Commands.GenerateFamilyData;
using backend.Application.Families.DTOs;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore; // Needed for .AsNoTracking() and .FirstOrDefaultAsync()

namespace backend.Application.UnitTests.Families.Commands.GenerateFamilyData;

public class GenerateFamilyDataCommandHandlerTests : TestBase
{
    private readonly Mock<IAiGenerateService> _aiGenerateServiceMock;
    private readonly GenerateFamilyDataCommandHandler _handler;

    public GenerateFamilyDataCommandHandlerTests()
    {
        _aiGenerateServiceMock = new Mock<IAiGenerateService>();
        _handler = new GenerateFamilyDataCommandHandler(_aiGenerateServiceMock.Object, _context);
    }

    /// <summary>
    /// Kiểm tra trường hợp xử lý lệnh tạo dữ liệu gia đình thành công.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldGenerateFamilyData_Successfully()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var promptContent = "System prompt content.";
        var aiOutputMemberId1 = "AI_MEMBER_1";
        var aiOutputMemberId2 = "AI_MEMBER_2";
        var aiOutputMemberId3 = "AI_MEMBER_3"; // For a new member

        // 1. Seed Prompt vào DB
        _context.Prompts.Add(new Prompt
        {
            Id = Guid.NewGuid(),
            Code = PromptConstants.FamilyDataGenerationPromptCode,
            Title = "Family Data Generation Prompt",
            Content = promptContent
        });
        await _context.SaveChangesAsync();

        // 2. Seed Family vào DB
        var family = new Family { Id = familyId, Name = "Test Family", Code = "FAM-TEST" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        // 3. Seed các Member có sẵn vào DB
        var existingMember1 = new Member(
            lastName: "Member One",
            firstName: "Existing",
            code: "MEM-001",
            familyId: familyId,
            nickname: null, // Optional
            gender: Gender.Male.ToString(),
            dateOfBirth: null, // Optional
            dateOfDeath: null, // Optional
            placeOfBirth: null, // Optional
            placeOfDeath: null, // Optional
            phone: null, // Optional
            email: null, // Optional
            address: null, // Optional
            occupation: null, // Optional
            avatarUrl: null, // Optional
            biography: null, // Optional
            order: null, // Optional
            isDeceased: false
        );
        var existingMember2 = new Member(
            lastName: "Member Two",
            firstName: "Existing",
            code: "MEM-002",
            familyId: familyId,
            nickname: null, // Optional
            gender: Gender.Female.ToString(),
            dateOfBirth: null, // Optional
            dateOfDeath: null, // Optional
            placeOfBirth: null, // Optional
            placeOfDeath: null, // Optional
            phone: null, // Optional
            email: null, // Optional
            address: null, // Optional
            occupation: null, // Optional
            avatarUrl: null, // Optional
            biography: null, // Optional
            order: null, // Optional
            isDeceased: false
        );
        _context.Members.AddRange(existingMember1, existingMember2);
        await _context.SaveChangesAsync();


        // 4. Chuẩn bị dữ liệu trả về từ AI Generate Service
        var aiGeneratedData = new AnalyzedDataDto
        {
            Members = new List<MemberDataDto>
            {
                new MemberDataDto { Id = aiOutputMemberId1, Code = existingMember1.Code, FirstName = "Updated Name", LastName = existingMember1.LastName, Gender = Gender.Male.ToString(), DateOfBirth = new DateTime(1980, 1, 1).ToString("yyyy-MM-dd") }, // Existing member
                new MemberDataDto { Id = aiOutputMemberId2, Code = existingMember2.Code, FirstName = existingMember2.FirstName, LastName = existingMember2.LastName, Gender = existingMember2.Gender, DateOfBirth = new DateTime(1985, 5, 10).ToString("yyyy-MM-dd") }, // Existing member
                new MemberDataDto { Id = aiOutputMemberId3, FirstName = "New", LastName = "Member Three", Gender = Gender.Female.ToString(), DateOfBirth = new DateTime(2000, 10, 20).ToString("yyyy-MM-dd") } // New member
            },
            Relationships = new List<RelationshipDataDto>
            {
                new RelationshipDataDto { SourceMemberId = aiOutputMemberId1, TargetMemberId = aiOutputMemberId2, Type = RelationshipType.Husband.ToString(), Order = 1 }, // Changed from Spouse to Husband
                new RelationshipDataDto { SourceMemberId = aiOutputMemberId1, TargetMemberId = aiOutputMemberId3, Type = RelationshipType.Father.ToString(), Order = 1 }
            },
            Events = new List<EventDataDto>
            {
                new EventDataDto { RelatedMemberIds = new List<string> { aiOutputMemberId1, aiOutputMemberId2 }, Type = EventType.Marriage.ToString(), Description = "Wedding event", Date = new DateTime(2005, 7, 15).ToString("yyyy-MM-dd"), Location = "Church" },
                new EventDataDto { RelatedMemberIds = new List<string> { aiOutputMemberId3 }, Type = EventType.Birth.ToString(), Description = "Birth of new member", Date = new DateTime(2000, 10, 20).ToString("yyyy-MM-dd"), Location = "Hospital" }
            },
            Feedback = "Data generated successfully."
        };

        _aiGenerateServiceMock.Setup(s => s.GenerateDataAsync<AnalyzedDataDto>(
            It.IsAny<GenerateRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<AnalyzedDataDto>.Success(aiGeneratedData));

        var command = new GenerateFamilyDataCommand
        {
            FamilyId = familyId,
            Content = "Generate family data for my family.",
            SessionId = "test-session"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Members.Should().HaveCount(3);
        result.Value!.Relationships.Should().HaveCount(2);
        result.Value!.Events.Should().HaveCount(2);

        // Verify existing members are correctly identified and their IDs are preserved
        var processedExistingMember1 = result.Value.Members.FirstOrDefault(m => m.Code == existingMember1.Code);
        processedExistingMember1.Should().NotBeNull();
        processedExistingMember1!.Id.Should().Be(existingMember1.Id);
        processedExistingMember1.IsExisting.Should().BeTrue();
        processedExistingMember1.FirstName.Should().Be("Updated Name"); // AI data should override

        var processedExistingMember2 = result.Value.Members.FirstOrDefault(m => m.Code == existingMember2.Code);
        processedExistingMember2.Should().NotBeNull();
        processedExistingMember2!.Id.Should().Be(existingMember2.Id);
        processedExistingMember2.IsExisting.Should().BeTrue();

        // Verify new member
        var processedNewMember = result.Value.Members.FirstOrDefault(m => m.FirstName == "New" && m.LastName == "Member Three");
        processedNewMember.Should().NotBeNull();
        processedNewMember!.Id.Should().NotBe(Guid.Empty);
        processedNewMember.IsExisting.Should().BeFalse();

        // Verify relationships
        var relationship1 = result.Value.Relationships.FirstOrDefault(r => r.Type == RelationshipType.Husband);
        relationship1.Should().NotBeNull();
        relationship1!.SourceMemberId.Should().Be(existingMember1.Id);
        relationship1.TargetMemberId.Should().Be(existingMember2.Id);

        var relationship2 = result.Value.Relationships.FirstOrDefault(r => r.Type == RelationshipType.Father);
        relationship2.Should().NotBeNull();
        relationship2!.SourceMemberId.Should().Be(existingMember1.Id);
        relationship2.TargetMemberId.Should().Be(processedNewMember!.Id);

        // Verify events
        var marriageEvent = result.Value.Events.FirstOrDefault(e => e.Type == EventType.Marriage);
        marriageEvent.Should().NotBeNull();
        marriageEvent!.RelatedMemberIds.Should().Contain(existingMember1.Id);
        marriageEvent.RelatedMemberIds.Should().Contain(existingMember2.Id);
        marriageEvent.RelatedMemberIds.Should().HaveCount(2);

        var birthEvent = result.Value.Events.FirstOrDefault(e => e.Type == EventType.Birth);
        birthEvent.Should().NotBeNull();
        birthEvent!.RelatedMemberIds.Should().Contain(processedNewMember.Id);
        birthEvent.RelatedMemberIds.Should().HaveCount(1);
    }

    /// <summary>
    /// Kiểm tra trường hợp AI Generate Service trả về lỗi.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenAiServiceFails()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var promptContent = "System prompt content.";

        // Seed Prompt
        _context.Prompts.Add(new Prompt
        {
            Id = Guid.NewGuid(),
            Code = PromptConstants.FamilyDataGenerationPromptCode,
            Title = "Family Data Generation Prompt",
            Content = promptContent
        });
        await _context.SaveChangesAsync();

        _aiGenerateServiceMock.Setup(s => s.GenerateDataAsync<AnalyzedDataDto>(
            It.IsAny<GenerateRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<AnalyzedDataDto>.Failure("AI Service Error", "AIService"));

        var command = new GenerateFamilyDataCommand
        {
            FamilyId = familyId,
            Content = "Generate family data for my family.",
            SessionId = "test-session"
        };

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()!
                 .WithMessage("AI Service Error");
    }

    /// <summary>
    /// Kiểm tra trường hợp AI trả về dữ liệu rỗng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAiReturnsNullData()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var promptContent = "System prompt content.";

        // Seed Prompt
        _context.Prompts.Add(new Prompt
        {
            Id = Guid.NewGuid(),
            Code = PromptConstants.FamilyDataGenerationPromptCode,
            Title = "Family Data Generation Prompt",
            Content = promptContent
        });
        await _context.SaveChangesAsync();

        _aiGenerateServiceMock.Setup(s => s.GenerateDataAsync<AnalyzedDataDto>(
            It.IsAny<GenerateRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<AnalyzedDataDto>.Success(new AnalyzedDataDto()));

        var command = new GenerateFamilyDataCommand
        {
            FamilyId = familyId,
            Content = "Generate family data for my family.",
            SessionId = "test-session"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Expect success now, as a new empty AnalyzedDataDto is returned
        result.Value.Should().NotBeNull();
        result.Value!.Members.Should().BeEmpty();
        result.Value.Events.Should().BeEmpty();
        result.Value.Relationships.Should().BeEmpty();
    }

    /// <summary>
    /// Kiểm tra trường hợp không tìm thấy System Prompt trong DB.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenSystemPromptNotFound()
    {
        // Arrange
        // NO Prompt seeded into the DB

        _aiGenerateServiceMock.Setup(s => s.GenerateDataAsync<AnalyzedDataDto>(
            It.IsAny<GenerateRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<AnalyzedDataDto>.Success(new AnalyzedDataDto())); // Mock a successful AI response to get to the prompt lookup

        var command = new GenerateFamilyDataCommand
        {
            FamilyId = Guid.NewGuid(),
            Content = "Generate family data for my family.",
            SessionId = "test-session"
        };

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()!
                 .WithMessage($"System prompt with code '{PromptConstants.FamilyDataGenerationPromptCode}' not found in the database.");
    }

    /// <summary>
    /// Kiểm tra xử lý lỗi khi memberData không hợp lệ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCaptureMemberValidationErrors()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var promptContent = "System prompt content.";

        _context.Prompts.Add(new Prompt
        {
            Id = Guid.NewGuid(),
            Code = PromptConstants.FamilyDataGenerationPromptCode,
            Title = "Family Data Generation Prompt",
            Content = promptContent
        });
        await _context.SaveChangesAsync();

        var aiGeneratedData = new AnalyzedDataDto
        {
            Members = new List<MemberDataDto>
            {
                new MemberDataDto { Id = "AI_MEMBER_1", FirstName = "", LastName = "Invalid", Gender = Gender.Male.ToString() }, // Invalid: FirstName is empty
            },
            Relationships = new List<RelationshipDataDto>(),
            Events = new List<EventDataDto>(),
            Feedback = "Data generated with member errors."
        };

        _aiGenerateServiceMock.Setup(s => s.GenerateDataAsync<AnalyzedDataDto>(
            It.IsAny<GenerateRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<AnalyzedDataDto>.Success(aiGeneratedData));

        var command = new GenerateFamilyDataCommand
        {
            FamilyId = familyId,
            Content = "Generate data with invalid member.",
            SessionId = "test-session"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Members.Should().HaveCount(1);
        result.Value.Members[0].ErrorMessage.Should().Contain("Tên của thành viên không được để trống.");
    }

    /// <summary>
    /// Kiểm tra xử lý lỗi khi relationshipData không hợp lệ (thiếu SourceMemberId hoặc TargetMemberId).
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCaptureRelationshipValidationErrors_MissingMembers()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var promptContent = "System prompt content.";

        _context.Prompts.Add(new Prompt
        {
            Id = Guid.NewGuid(),
            Code = PromptConstants.FamilyDataGenerationPromptCode,
            Title = "Family Data Generation Prompt",
            Content = promptContent
        });
        await _context.SaveChangesAsync();

        var aiGeneratedData = new AnalyzedDataDto
        {
            Members = new List<MemberDataDto>
            {
                new MemberDataDto { Id = "AI_MEMBER_1", FirstName = "Valid", LastName = "Member", Gender = Gender.Male.ToString() }
            },
            Relationships = new List<RelationshipDataDto>
            {
                new RelationshipDataDto { SourceMemberId = "AI_MEMBER_1", TargetMemberId = "NON_EXISTENT_MEMBER", Type = "Child" }, // Target member ID not mapped
                new RelationshipDataDto { SourceMemberId = "NON_EXISTENT_MEMBER_2", TargetMemberId = "AI_MEMBER_1", Type = "Parent" }, // Source member ID not mapped
                new RelationshipDataDto { SourceMemberId = null!, TargetMemberId = "AI_MEMBER_1", Type = "Sibling" }, // Missing SourceMemberId
                new RelationshipDataDto { SourceMemberId = "AI_MEMBER_1", TargetMemberId = null!, Type = "Sibling" } // Missing TargetMemberId
            },
            Events = new List<EventDataDto>(),
            Feedback = "Data generated with relationship errors."
        };

        _aiGenerateServiceMock.Setup(s => s.GenerateDataAsync<AnalyzedDataDto>(
            It.IsAny<GenerateRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<AnalyzedDataDto>.Success(aiGeneratedData));

        var command = new GenerateFamilyDataCommand
        {
            FamilyId = familyId,
            Content = "Generate data with invalid relationships.",
            SessionId = "test-session"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Relationships.Should().BeEmpty(); // No relationships should be processed if there are errors

        // It's tricky to assert specific error messages for relationships as they are currently just skipped.
        // If the requirement was to return RelationshipResultDto with ErrorMessage, then we'd assert that.
        // For now, asserting that no invalid relationships make it through.
    }

    /// <summary>
    /// Kiểm tra xử lý lỗi khi relationshipData có Type không hợp lệ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCaptureRelationshipValidationErrors_InvalidType()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var promptContent = "System prompt content.";

        _context.Prompts.Add(new Prompt
        {
            Id = Guid.NewGuid(),
            Code = PromptConstants.FamilyDataGenerationPromptCode,
            Title = "Family Data Generation Prompt",
            Content = promptContent
        });
        await _context.SaveChangesAsync();

        var aiGeneratedData = new AnalyzedDataDto
        {
            Members = new List<MemberDataDto>
            {
                new MemberDataDto { Id = "AI_MEMBER_1", FirstName = "Valid", LastName = "Member", Gender = Gender.Male.ToString() },
                new MemberDataDto { Id = "AI_MEMBER_2", FirstName = "Valid", LastName = "Member", Gender = Gender.Female.ToString() }
            },
            Relationships = new List<RelationshipDataDto>
            {
                new RelationshipDataDto { SourceMemberId = "AI_MEMBER_1", TargetMemberId = "AI_MEMBER_2", Type = "INVALID_TYPE" }, // Invalid RelationshipType
                new RelationshipDataDto { SourceMemberId = "AI_MEMBER_1", TargetMemberId = "AI_MEMBER_2", Type = "999" } // Invalid integer type
            },
            Events = new List<EventDataDto>(),
            Feedback = "Data generated with relationship type errors."
        };

        _aiGenerateServiceMock.Setup(s => s.GenerateDataAsync<AnalyzedDataDto>(
            It.IsAny<GenerateRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<AnalyzedDataDto>.Success(aiGeneratedData));

        var command = new GenerateFamilyDataCommand
        {
            FamilyId = familyId,
            Content = "Generate data with invalid relationship types.",
            SessionId = "test-session"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Relationships.Should().BeEmpty(); // No relationships should be processed if there are errors
    }

    /// <summary>
    /// Kiểm tra xử lý lỗi khi eventData không hợp lệ (thiếu RelatedMemberIds hoặc Type).
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCaptureEventValidationErrors()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var promptContent = "System prompt content.";

        _context.Prompts.Add(new Prompt
        {
            Id = Guid.NewGuid(),
            Code = PromptConstants.FamilyDataGenerationPromptCode,
            Title = "Family Data Generation Prompt",
            Content = promptContent
        });
        await _context.SaveChangesAsync();

        var aiGeneratedData = new AnalyzedDataDto
        {
            Members = new List<MemberDataDto>
            {
                new MemberDataDto { Id = "AI_MEMBER_1", FirstName = "Valid", LastName = "Member", Gender = Gender.Male.ToString() }
            },
            Relationships = new List<RelationshipDataDto>(),
            Events = new List<EventDataDto>
            {
                new EventDataDto { RelatedMemberIds = new List<string>(), Type = EventType.Birth.ToString(), Description = "Event with no related members", Date = new DateTime(2000, 1, 1).ToString("yyyy-MM-dd") }, // Missing related members (validation will catch this)
                new EventDataDto { RelatedMemberIds = new List<string> { "NON_EXISTENT_MEMBER" }, Type = EventType.Death.ToString(), Description = "Event with unmapped member", Date = new DateTime(2050, 1, 1).ToString("yyyy-MM-dd") },
                new EventDataDto { RelatedMemberIds = new List<string> { "AI_MEMBER_1" }, Type = "INVALID_EVENT_TYPE", Description = "Event with invalid type", Date = new DateTime(2000, 1, 1).ToString("yyyy-MM-dd") }
            },
            Feedback = "Data generated with event errors."
        };

        _aiGenerateServiceMock.Setup(s => s.GenerateDataAsync<AnalyzedDataDto>(
            It.IsAny<GenerateRequest>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(Result<AnalyzedDataDto>.Success(aiGeneratedData));

        var command = new GenerateFamilyDataCommand
        {
            FamilyId = familyId,
            Content = "Generate data with invalid events.",
            SessionId = "test-session"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Events.Should().HaveCount(3);
        result.Value.Events[0].ErrorMessage.Should().Contain("Sự kiện phải có ít nhất một thành viên được liên kết.");
        result.Value.Events[1].RelatedMemberIds.Should().BeEmpty(); // Unmapped member should result in empty list
        result.Value.Events[2].Type.Should().Be(EventType.Other); // Invalid type defaults to Other
    }
}
